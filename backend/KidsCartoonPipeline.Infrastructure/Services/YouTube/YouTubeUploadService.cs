using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Exceptions;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace KidsCartoonPipeline.Infrastructure.Services.YouTube;

public class YouTubeUploadService : IYouTubeService
{
    private readonly IEpisodeRepository _episodes;
    private readonly ISettingsService _settings;
    private readonly IAssetStorageService _storage;
    private readonly ILogger<YouTubeUploadService> _logger;

    public YouTubeUploadService(IEpisodeRepository episodes, ISettingsService settings, IAssetStorageService storage, ILogger<YouTubeUploadService> logger)
    {
        _episodes = episodes;
        _settings = settings;
        _storage = storage;
        _logger = logger;
    }

    public async Task<string> UploadToYouTubeAsync(int episodeId)
    {
        var episode = await _episodes.GetByIdAsync(episodeId);
        if (episode == null) throw new Exception($"Episode {episodeId} not found");

        try
        {
            var videoId = await UploadVideoAsync(episode);
            episode.YouTubeVideoId = videoId;
            episode.YouTubeUrl = $"https://youtu.be/{videoId}";
            episode.Status = KidsCartoonPipeline.Core.Enums.EpisodeStatus.Published;
            episode.UpdatedAt = DateTime.UtcNow;
            await _episodes.UpdateAsync(episode);
            return videoId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background YouTube upload failed for episode {EpisodeId}", episodeId);
            episode.Status = KidsCartoonPipeline.Core.Enums.EpisodeStatus.Failed;
            episode.CurrentStageError = $"YouTube Upload: {ex.Message}";
            await _episodes.UpdateAsync(episode);
            throw;
        }
    }

    public async Task<string> UploadVideoAsync(Episode episode)
    {
        var credential = await GetCredentialAsync();
        var youtubeService = new YouTubeService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "KidsCartoonPipeline"
        });

        var categoryId = await _settings.GetAsync("YouTube:CategoryId") ?? "27";
        var madeForKids = bool.Parse(await _settings.GetAsync("YouTube:MadeForKids") ?? "true");
        var descSuffix = await _settings.GetAsync("YouTube:DescriptionSuffix") ?? "";
        var tags = !string.IsNullOrEmpty(episode.SeoTags)
            ? JsonSerializer.Deserialize<List<string>>(episode.SeoTags)
            : new List<string>();

        var video = new Google.Apis.YouTube.v3.Data.Video
        {
            Snippet = new VideoSnippet
            {
                Title = episode.SeoTitle ?? episode.Title ?? "Kids Cartoon Episode",
                Description = (episode.SeoDescription ?? "") + descSuffix,
                Tags = tags,
                CategoryId = categoryId,
                DefaultLanguage = "en",
                DefaultAudioLanguage = "en"
            },
            Status = new VideoStatus
            {
                PrivacyStatus = "private",
                PublishAtRaw = episode.ScheduledPublishAt.ToString(),
                MadeForKids = madeForKids,
                SelfDeclaredMadeForKids = madeForKids
            }
        };

        var videoPath = _storage.GetFullPath(episode.VideoPath!);
        using var videoStream = File.OpenRead(videoPath);
        var insertRequest = youtubeService.Videos.Insert(video, "snippet,status", videoStream, "video/*");
        var uploadResponse = await insertRequest.UploadAsync();

        if (uploadResponse.Exception != null)
            throw new ExternalServiceException("YouTube", $"Upload failed: {uploadResponse.Exception.Message}", innerException: uploadResponse.Exception);

        var videoId = insertRequest.ResponseBody?.Id
            ?? throw new ExternalServiceException("YouTube", "Upload completed but no video ID returned");

        if (!string.IsNullOrEmpty(episode.ThumbnailPath))
        {
            try
            {
                var thumbPath = _storage.GetFullPath(episode.ThumbnailPath);
                using var thumbStream = File.OpenRead(thumbPath);
                await youtubeService.Thumbnails.Set(videoId, thumbStream, "image/png").UploadAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Thumbnail upload failed for video {VideoId}", videoId);
            }
        }

        _logger.LogInformation("Uploaded video {VideoId} for episode {EpisodeId}", videoId, episode.Id);
        return videoId;
    }

    private async Task<string> GetRedirectUriAsync()
    {
        return await _settings.GetAsync("YouTube:RedirectUri") 
            ?? "http://localhost:5018/api/youtube/callback";
    }

    public async Task<string> GetAuthorizationUrlAsync()
    {
        var clientId = await _settings.GetRequiredAsync("YouTube:ClientId");
        var clientSecret = await _settings.GetRequiredAsync("YouTube:ClientSecret");
        var redirectUri = await GetRedirectUriAsync();

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
            Scopes = [YouTubeService.Scope.YoutubeUpload, YouTubeService.Scope.Youtube]
        });

        var request = flow.CreateAuthorizationCodeRequest(redirectUri);
        // access_type=offline is already set by the SDK; just force re-consent to ensure refresh token
        request.ResponseType = "code";
        var uri = request.Build();
        // Append prompt=consent (not a built-in property on the request object)
        var url = uri.ToString() + "&prompt=consent";
        return url;
    }

    public async Task<bool> ExchangeCodeAsync(string code)
    {
        var clientId = await _settings.GetRequiredAsync("YouTube:ClientId");
        var clientSecret = await _settings.GetRequiredAsync("YouTube:ClientSecret");
        var redirectUri = await GetRedirectUriAsync();

        _logger.LogInformation("Exchanging auth code with redirect URI: {RedirectUri}", redirectUri);

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
            Scopes = [YouTubeService.Scope.YoutubeUpload, YouTubeService.Scope.Youtube]
        });

        try
        {
            var tokenResponse = await flow.ExchangeCodeForTokenAsync("user", code, redirectUri, CancellationToken.None);
            if (tokenResponse?.RefreshToken != null)
            {
                await _settings.SetAsync("YouTube:RefreshToken", tokenResponse.RefreshToken);
                _logger.LogInformation("YouTube OAuth refresh token saved successfully");
                return true;
            }

            _logger.LogWarning("Token exchange succeeded but no refresh token returned. " +
                "Ensure 'access_type=offline' and 'prompt=consent' are set, or revoke app access at https://myaccount.google.com/permissions and re-authorize.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange YouTube auth code. RedirectUri={RedirectUri}", redirectUri);
            throw;
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var credential = await GetCredentialAsync();
            var youtubeService = new YouTubeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "KidsCartoonPipeline"
            });
            var channelsRequest = youtubeService.Channels.List("snippet");
            channelsRequest.Mine = true;
            var response = await channelsRequest.ExecuteAsync();
            return response.Items?.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<UserCredential> GetCredentialAsync()
    {
        var clientId = await _settings.GetRequiredAsync("YouTube:ClientId");
        var clientSecret = await _settings.GetRequiredAsync("YouTube:ClientSecret");
        var refreshToken = await _settings.GetRequiredAsync("YouTube:RefreshToken");

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
            Scopes = [YouTubeService.Scope.YoutubeUpload, YouTubeService.Scope.Youtube]
        });

        return new UserCredential(flow, "user", new TokenResponse { RefreshToken = refreshToken });
    }
}
