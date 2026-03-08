using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KidsCartoonPipeline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class YouTubeController : ControllerBase
{
    private readonly IYouTubeService _youtubeService;
    private readonly ISettingsService _settings;

    public YouTubeController(IYouTubeService youtubeService, ISettingsService settings)
    {
        _youtubeService = youtubeService;
        _settings = settings;
    }

    /// <summary>
    /// Get the Google OAuth authorization URL. Frontend opens this in a new tab.
    /// </summary>
    [HttpGet("auth-url")]
    public async Task<ActionResult> GetAuthUrl()
    {
        var url = await _youtubeService.GetAuthorizationUrlAsync();
        return Ok(new { url });
    }

    /// <summary>
    /// OAuth callback — Google redirects here with a code. Exchanges it for a refresh token.
    /// </summary>
    [HttpGet("callback")]
    public async Task<ActionResult> Callback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest(new { message = "Missing authorization code" });

        bool success;
        try
        {
            success = await _youtubeService.ExchangeCodeAsync(code);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Failed to exchange authorization code: {ex.Message}" });
        }

        if (!success)
            return BadRequest(new { message = "Token exchange succeeded but no refresh token was returned. Try revoking app access at https://myaccount.google.com/permissions and re-authorizing." });

        // Return a simple HTML page that closes itself and notifies the parent window
        var html = """
            <!DOCTYPE html>
            <html>
            <body>
                <h3>✅ YouTube connected successfully!</h3>
                <p>You can close this window.</p>
                <script>
                    if (window.opener) {
                        window.opener.postMessage({ type: 'youtube-auth-success' }, '*');
                    }
                    setTimeout(() => window.close(), 2000);
                </script>
            </body>
            </html>
            """;
        return Content(html, "text/html");
    }
}
