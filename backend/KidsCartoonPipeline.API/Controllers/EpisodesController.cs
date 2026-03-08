using System.Text.Json;
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
public class EpisodesController : ControllerBase
{
    private readonly IEpisodeRepository _repo;
    private readonly IPipelineJobRepository _jobRepo;
    private readonly IPipelineOrchestrator _orchestrator;
    private readonly IYouTubeService _youtubeService;

    public EpisodesController(IEpisodeRepository repo, IPipelineJobRepository jobRepo,
        IPipelineOrchestrator orchestrator, IYouTubeService youtubeService)
    {
        _repo = repo;
        _jobRepo = jobRepo;
        _orchestrator = orchestrator;
        _youtubeService = youtubeService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<EpisodeResponse>>> GetAll(
        [FromQuery] string? status, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        EpisodeStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<EpisodeStatus>(status, true, out var parsed))
            statusEnum = parsed;

        var (items, totalCount) = await _repo.GetPagedAsync(statusEnum, search, page, pageSize);
        return Ok(new PagedResult<EpisodeResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EpisodeResponse>> GetById(int id)
    {
        var episode = await _repo.GetByIdWithDetailsAsync(id);
        if (episode == null) return NotFound();
        return Ok(MapToResponse(episode));
    }

    [HttpPut("{id}/metadata")]
    public async Task<ActionResult<EpisodeResponse>> UpdateMetadata(int id, [FromBody] UpdateEpisodeMetadataRequest request)
    {
        var episode = await _repo.GetByIdWithDetailsAsync(id);
        if (episode == null) return NotFound();
        if (request.Title != null) episode.Title = request.Title;
        if (request.SeoTitle != null) episode.SeoTitle = request.SeoTitle;
        if (request.SeoDescription != null) episode.SeoDescription = request.SeoDescription;
        if (request.SeoTags != null) episode.SeoTags = request.SeoTags;
        if (request.ScheduledPublishAt.HasValue) episode.ScheduledPublishAt = request.ScheduledPublishAt;
        await _repo.UpdateAsync(episode);
        return Ok(MapToResponse(episode));
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult> Approve(int id, [FromBody] ApproveEpisodeRequest? request)
    {
        var episode = await _repo.GetByIdAsync(id);
        if (episode == null) return NotFound();
        if (episode.Status != EpisodeStatus.PendingReview)
            return BadRequest(new { message = "Episode is not pending review" });

        episode.Status = EpisodeStatus.Approved;
        episode.ReviewedAt = DateTime.UtcNow;
        if (request?.ScheduledPublishAt.HasValue == true)
            episode.ScheduledPublishAt = request.ScheduledPublishAt;
        await _repo.UpdateAsync(episode);

        BackgroundJob.Enqueue<IYouTubeService>(y => y.UploadToYouTubeAsync(id));

        return Ok(new { message = "Episode approved and upload started" });
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult> Reject(int id, [FromBody] RejectEpisodeRequest? request)
    {
        var episode = await _repo.GetByIdAsync(id);
        if (episode == null) return NotFound();
        episode.Status = EpisodeStatus.Rejected;
        episode.ReviewNotes = request?.ReviewNotes;
        episode.ReviewedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(episode);
        return Ok(new { message = "Episode rejected" });
    }

    [HttpPost("{id}/regenerate/{stage}")]
    public async Task<ActionResult> Regenerate(int id, string stage)
    {
        var episode = await _repo.GetByIdAsync(id);
        if (episode == null) return NotFound();
        if (!Enum.TryParse<PipelineStage>(stage, true, out var pipelineStage))
            return BadRequest(new { message = "Invalid pipeline stage" });

        BackgroundJob.Enqueue<IPipelineOrchestrator>(o => o.RunStageAsync(id, pipelineStage));
        return Accepted(new { message = $"Regenerating {stage}" });
    }

    [HttpPost("{id}/resume/{stage}")]
    public async Task<ActionResult> Resume(int id, string stage)
    {
        var episode = await _repo.GetByIdAsync(id);
        if (episode == null) return NotFound();
        if (!Enum.TryParse<PipelineStage>(stage, true, out var pipelineStage))
            return BadRequest(new { message = "Invalid pipeline stage" });

        BackgroundJob.Enqueue<IPipelineOrchestrator>(o => o.ResumePipelineAsync(id, pipelineStage));
        return Accepted(new { message = $"Resuming pipeline from {stage}" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/pipeline-jobs")]
    public async Task<ActionResult<List<PipelineJobResponse>>> GetPipelineJobs(int id)
    {
        var jobs = await _jobRepo.GetByEpisodeIdAsync(id);
        return Ok(jobs.Select(j => new PipelineJobResponse
        {
            Id = j.Id, EpisodeId = j.EpisodeId, EpisodeTitle = j.Episode?.Title,
            Stage = j.Stage.ToString(), Status = j.Status.ToString(),
            ErrorMessage = j.ErrorMessage, LogOutput = j.LogOutput,
            CreatedAt = j.CreatedAt, StartedAt = j.StartedAt, CompletedAt = j.CompletedAt,
            DurationSeconds = j.CompletedAt.HasValue && j.StartedAt.HasValue
                ? (j.CompletedAt.Value - j.StartedAt.Value).TotalSeconds : null
        }).ToList());
    }

    private static EpisodeResponse MapToResponse(Episode e) => new()
    {
        Id = e.Id, Title = e.Title, Summary = e.Summary, Moral = e.Moral,
        Status = e.Status.ToString(), StatusLabel = FormatStatus(e.Status),
        VideoUrl = e.VideoPath != null ? $"/api/assets/video/{e.Id}" : null,
        ThumbnailUrl = e.ThumbnailPath != null ? $"/api/assets/thumbnail/{e.Id}" : null,
        SeoTitle = e.SeoTitle, SeoDescription = e.SeoDescription,
        SeoTags = !string.IsNullOrEmpty(e.SeoTags) ? JsonSerializer.Deserialize<List<string>>(e.SeoTags) : null,
        ScheduledPublishAt = e.ScheduledPublishAt, YouTubeVideoId = e.YouTubeVideoId, YouTubeUrl = e.YouTubeUrl,
        CurrentStageError = e.CurrentStageError, ReviewNotes = e.ReviewNotes,
        CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt,
        Scenes = e.Scenes?.Select(s => new SceneResponse
        {
            Id = s.Id, SceneNumber = s.SceneNumber, DurationSeconds = s.DurationSeconds,
            BackgroundDescription = s.BackgroundDescription, ActionDescription = s.ActionDescription,
            ImageUrl = s.ImagePath != null ? $"/api/assets/image/{s.Id}" : null,
            ImagePromptUsed = s.ImagePromptUsed,
            DialogueLines = s.DialogueLines?.Select(d => new DialogueLineResponse
            {
                Id = d.Id, LineOrder = d.LineOrder, CharacterName = d.CharacterName,
                Text = d.Text, Tone = d.Tone,
                AudioUrl = d.AudioPath != null ? $"/api/assets/audio/{d.Id}" : null
            }).OrderBy(d => d.LineOrder).ToList() ?? []
        }).OrderBy(s => s.SceneNumber).ToList() ?? [],
        Characters = e.Characters?.Select(c => new CharacterResponse
        {
            Id = c.Id, Name = c.Name, AvatarUrl = c.AvatarUrl
        }).ToList() ?? []
    };

    private static string FormatStatus(EpisodeStatus status) => status switch
    {
        EpisodeStatus.TopicQueued => "Topic Queued",
        EpisodeStatus.GeneratingScript => "Generating Script",
        EpisodeStatus.GeneratingImages => "Generating Images",
        EpisodeStatus.GeneratingAudio => "Generating Audio",
        EpisodeStatus.GeneratingMusic => "Generating Music",
        EpisodeStatus.RenderingVideo => "Rendering Video",
        EpisodeStatus.GeneratingSeo => "Generating SEO",
        EpisodeStatus.PendingReview => "Pending Review",
        _ => status.ToString()
    };
}
