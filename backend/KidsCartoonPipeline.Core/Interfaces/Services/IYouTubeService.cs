using KidsCartoonPipeline.Core.Entities;

namespace KidsCartoonPipeline.Core.Interfaces.Services;

public interface IYouTubeService
{
    Task<string> UploadVideoAsync(Episode episode);
    Task<string> UploadToYouTubeAsync(int episodeId);
    Task<string> GetAuthorizationUrlAsync();
    Task<bool> ExchangeCodeAsync(string code);
    Task<bool> TestConnectionAsync();
}
