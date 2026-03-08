using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Core.Interfaces.Services;
using KidsCartoonPipeline.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidsCartoonPipeline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly IAssetStorageService _storage;
    private readonly AppDbContext _db;

    public AssetsController(IAssetStorageService storage, AppDbContext db)
    {
        _storage = storage;
        _db = db;
    }

    [HttpGet("video/{episodeId}")]
    public async Task<IActionResult> GetVideo(int episodeId)
    {
        var episode = await _db.Episodes.FindAsync(episodeId);
        if (episode?.VideoPath == null) return NotFound();
        var stream = await _storage.GetFileStreamAsync(episode.VideoPath);
        return File(stream, "video/mp4", enableRangeProcessing: true);
    }

    [HttpGet("thumbnail/{episodeId}")]
    public async Task<IActionResult> GetThumbnail(int episodeId)
    {
        var episode = await _db.Episodes.FindAsync(episodeId);
        if (episode?.ThumbnailPath == null) return NotFound();
        var stream = await _storage.GetFileStreamAsync(episode.ThumbnailPath);
        return File(stream, "image/png");
    }

    [HttpGet("audio/{dialogueId}")]
    public async Task<IActionResult> GetAudio(int dialogueId)
    {
        var line = await _db.DialogueLines.FindAsync(dialogueId);
        if (line?.AudioPath == null) return NotFound();
        var stream = await _storage.GetFileStreamAsync(line.AudioPath);
        return File(stream, "audio/mpeg");
    }

    [HttpGet("image/{sceneId}")]
    public async Task<IActionResult> GetImage(int sceneId)
    {
        var scene = await _db.Scenes.FindAsync(sceneId);
        if (scene?.ImagePath == null) return NotFound();
        var stream = await _storage.GetFileStreamAsync(scene.ImagePath);
        return File(stream, "image/png");
    }

    [HttpGet("audio-file")]
    public async Task<IActionResult> GetAudioFile([FromQuery] string path)
    {
        if (string.IsNullOrEmpty(path)) return BadRequest();
        var stream = await _storage.GetFileStreamAsync(path);
        return File(stream, "audio/mpeg");
    }
}
