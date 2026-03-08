using KidsCartoonPipeline.Core.DTOs.Requests;
using KidsCartoonPipeline.Core.DTOs.Responses;
using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Exceptions;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KidsCartoonPipeline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CharactersController : ControllerBase
{
    private readonly ICharacterRepository _repo;
    private readonly IVoiceGenerationService _voiceService;
    private readonly ICacheService _cache;

    public CharactersController(ICharacterRepository repo, IVoiceGenerationService voiceService, ICacheService cache)
    {
        _repo = repo;
        _voiceService = voiceService;
        _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult<List<CharacterResponse>>> GetAll()
    {
        var characters = await _repo.GetAllAsync();
        return Ok(characters.Select(MapToResponse).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CharacterResponse>> GetById(int id)
    {
        var character = await _repo.GetByIdAsync(id);
        if (character == null) return NotFound();
        return Ok(MapToResponse(character));
    }

    [HttpPost]
    public async Task<ActionResult<CharacterResponse>> Create([FromBody] CreateCharacterRequest request)
    {
        var character = new Character
        {
            Name = request.Name, Description = request.Description, VoiceId = request.VoiceId,
            VoiceName = request.VoiceName, ImagePromptStyle = request.ImagePromptStyle, AvatarUrl = request.AvatarUrl
        };
        var created = await _repo.CreateAsync(character);
        _cache.RemoveByPrefix("characters:");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CharacterResponse>> Update(int id, [FromBody] CreateCharacterRequest request)
    {
        var character = await _repo.GetByIdAsync(id);
        if (character == null) return NotFound();
        character.Name = request.Name;
        character.Description = request.Description;
        character.VoiceId = request.VoiceId;
        character.VoiceName = request.VoiceName;
        character.ImagePromptStyle = request.ImagePromptStyle;
        character.AvatarUrl = request.AvatarUrl;
        await _repo.UpdateAsync(character);
        _cache.RemoveByPrefix("characters:");
        return Ok(MapToResponse(character));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _repo.DeleteAsync(id);
        _cache.RemoveByPrefix("characters:");
        return NoContent();
    }

    [HttpGet("voices")]
    public async Task<ActionResult> GetVoices()
    {
        if (_cache.TryGet<List<VoiceInfo>>("elevenlabs:voices", out var cached) && cached != null)
            return Ok(cached);
        var voices = await _voiceService.GetAvailableVoicesAsync();
        _cache.Set("elevenlabs:voices", voices, TimeSpan.FromHours(1));
        return Ok(voices);
    }

    [HttpPost("{id}/test-voice")]
    public async Task<ActionResult> TestVoice(int id)
    {
        var character = await _repo.GetByIdAsync(id);
        if (character == null) return NotFound();
        if (string.IsNullOrEmpty(character.VoiceId)) return BadRequest(new { message = "No voice assigned" });
        var audioPath = await _voiceService.GenerateTestAudioAsync(character.VoiceId, $"Hi! I'm {character.Name}!");
        return Ok(new { audioUrl = $"/api/assets/audio-file?path={Uri.EscapeDataString(audioPath)}" });
    }

    private static CharacterResponse MapToResponse(Character c) => new()
    {
        Id = c.Id, Name = c.Name, Description = c.Description, VoiceId = c.VoiceId,
        VoiceName = c.VoiceName, ImagePromptStyle = c.ImagePromptStyle, AvatarUrl = c.AvatarUrl,
        IsActive = c.IsActive, EpisodeCount = c.Episodes?.Count ?? 0, CreatedAt = c.CreatedAt
    };
}
