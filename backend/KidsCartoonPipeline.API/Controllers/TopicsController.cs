using Hangfire;
using KidsCartoonPipeline.Core.DTOs.Requests;
using KidsCartoonPipeline.Core.DTOs.Responses;
using KidsCartoonPipeline.Core.Entities;
using KidsCartoonPipeline.Core.Enums;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KidsCartoonPipeline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly ITopicRepository _repo;
    private readonly IEpisodeRepository _episodeRepo;
    private readonly ICharacterRepository _characterRepo;
    private readonly IScriptGenerationService _scriptService;
    private readonly IPipelineOrchestrator _orchestrator;

    public TopicsController(ITopicRepository repo, IEpisodeRepository episodeRepo, ICharacterRepository characterRepo,
        IScriptGenerationService scriptService, IPipelineOrchestrator orchestrator)
    {
        _repo = repo;
        _episodeRepo = episodeRepo;
        _characterRepo = characterRepo;
        _scriptService = scriptService;
        _orchestrator = orchestrator;
    }

    [HttpGet]
    public async Task<ActionResult<List<TopicResponse>>> GetAll()
    {
        var topics = await _repo.GetAllAsync();
        return Ok(topics.Select(MapToResponse).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<TopicResponse>> Create([FromBody] CreateTopicRequest request)
    {
        var topic = new TopicSeed
        {
            Title = request.Title, Description = request.Description,
            TargetMoral = request.TargetMoral, Priority = request.Priority
        };
        var created = await _repo.CreateAsync(topic);
        return CreatedAtAction(nameof(GetAll), null, MapToResponse(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TopicResponse>> Update(int id, [FromBody] CreateTopicRequest request)
    {
        var topic = await _repo.GetByIdAsync(id);
        if (topic == null) return NotFound();
        topic.Title = request.Title;
        topic.Description = request.Description;
        topic.TargetMoral = request.TargetMoral;
        topic.Priority = request.Priority;
        await _repo.UpdateAsync(topic);
        return Ok(MapToResponse(topic));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("generate-ideas")]
    public async Task<ActionResult<List<string>>> GenerateIdeas()
    {
        var characters = await _characterRepo.GetActiveAsync();
        var ideas = await _scriptService.GenerateTopicIdeasAsync(characters);
        return Ok(ideas);
    }

    [HttpPost("{id}/trigger")]
    public async Task<ActionResult> Trigger(int id)
    {
        var topic = await _repo.GetByIdAsync(id);
        if (topic == null) return NotFound();

        var characters = await _characterRepo.GetActiveAsync();
        var episode = new Episode
        {
            TopicSeedId = topic.Id,
            Title = topic.Title,
            Status = EpisodeStatus.TopicQueued,
            Characters = characters
        };
        var created = await _episodeRepo.CreateAsync(episode);

        topic.IsUsed = true;
        topic.UsedAt = DateTime.UtcNow;
        topic.EpisodeId = created.Id;
        await _repo.UpdateAsync(topic);

        BackgroundJob.Enqueue<IPipelineOrchestrator>(o => o.RunFullPipelineAsync(created.Id));

        return Accepted(new { episodeId = created.Id });
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderTopicsRequest request)
    {
        await _repo.UpdatePrioritiesAsync(request.Items.Select(i => (i.Id, i.Priority)).ToList());
        return Ok();
    }

    private static TopicResponse MapToResponse(TopicSeed t) => new()
    {
        Id = t.Id, Title = t.Title, Description = t.Description, TargetMoral = t.TargetMoral,
        Priority = t.Priority, IsUsed = t.IsUsed, UsedAt = t.UsedAt, EpisodeId = t.EpisodeId,
        CreatedAt = t.CreatedAt
    };
}
