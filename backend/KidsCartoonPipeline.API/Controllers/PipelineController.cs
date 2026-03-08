using KidsCartoonPipeline.Core.DTOs.Requests;
using KidsCartoonPipeline.Core.DTOs.Responses;
using KidsCartoonPipeline.Core.Enums;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KidsCartoonPipeline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PipelineController : ControllerBase
{
    private readonly IEpisodeRepository _episodeRepo;
    private readonly ITopicRepository _topicRepo;
    private readonly IPipelineJobRepository _jobRepo;
    private readonly IPipelineOrchestrator _orchestrator;
    private readonly ICharacterRepository _characterRepo;
    private readonly ISettingsService _settings;

    public PipelineController(IEpisodeRepository episodeRepo, ITopicRepository topicRepo,
        IPipelineJobRepository jobRepo, IPipelineOrchestrator orchestrator,
        ICharacterRepository characterRepo, ISettingsService settings)
    {
        _episodeRepo = episodeRepo;
        _topicRepo = topicRepo;
        _jobRepo = jobRepo;
        _orchestrator = orchestrator;
        _characterRepo = characterRepo;
        _settings = settings;
    }

    [HttpGet("status")]
    public async Task<ActionResult<PipelineStatusResponse>> GetStatus()
    {
        var isActive = (await _settings.GetAsync("Pipeline:IsActive")) == "true";
        var activeEpisodes = (await _episodeRepo.GetActiveEpisodesAsync()).Count;
        var queuedTopics = await _topicRepo.GetUnusedCountAsync();
        var pendingReview = await _episodeRepo.GetCountByStatusAsync(EpisodeStatus.PendingReview);
        var recentJobs = await _jobRepo.GetRecentAsync(10);

        return Ok(new PipelineStatusResponse
        {
            IsActive = isActive,
            ActiveEpisodes = activeEpisodes,
            QueuedTopics = queuedTopics,
            PendingReview = pendingReview,
            RecentJobs = recentJobs.Select(j => new PipelineJobResponse
            {
                Id = j.Id, EpisodeId = j.EpisodeId, EpisodeTitle = j.Episode?.Title,
                Stage = j.Stage.ToString(), Status = j.Status.ToString(),
                ErrorMessage = j.ErrorMessage, CreatedAt = j.CreatedAt,
                StartedAt = j.StartedAt, CompletedAt = j.CompletedAt,
                DurationSeconds = j.CompletedAt.HasValue && j.StartedAt.HasValue
                    ? (j.CompletedAt.Value - j.StartedAt.Value).TotalSeconds : null
            }).ToList()
        });
    }

    [HttpPost("trigger")]
    public async Task<ActionResult> Trigger()
    {
        var topic = await _topicRepo.GetNextUnusedAsync();
        if (topic == null) return BadRequest(new { message = "No queued topics available" });

        var characters = await _characterRepo.GetActiveAsync();
        var episode = new Core.Entities.Episode
        {
            TopicSeedId = topic.Id, Title = topic.Title,
            Status = EpisodeStatus.TopicQueued, Characters = characters
        };
        var created = await _episodeRepo.CreateAsync(episode);

        topic.IsUsed = true;
        topic.UsedAt = DateTime.UtcNow;
        topic.EpisodeId = created.Id;
        await _topicRepo.UpdateAsync(topic);

        _ = Task.Run(async () =>
        {
            try { await _orchestrator.RunFullPipelineAsync(created.Id); }
            catch { /* logged in orchestrator */ }
        });

        return Accepted(new { message = "Pipeline triggered", episodeId = created.Id });
    }

    [HttpPost("pause")]
    public async Task<IActionResult> Pause()
    {
        await _settings.SetAsync("Pipeline:IsActive", "false");
        return Ok(new { message = "Pipeline paused" });
    }

    [HttpPost("resume")]
    public async Task<IActionResult> Resume()
    {
        await _settings.SetAsync("Pipeline:IsActive", "true");
        return Ok(new { message = "Pipeline resumed" });
    }

    [HttpGet("logs/{episodeId}")]
    public async Task<ActionResult<List<PipelineJobResponse>>> GetLogs(int episodeId)
    {
        var jobs = await _jobRepo.GetByEpisodeIdAsync(episodeId);
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

    [HttpGet("schedule")]
    public async Task<ActionResult<ScheduleResponse>> GetSchedule()
    {
        var cron = await _settings.GetAsync("Pipeline:CronSchedule") ?? "0 */6 * * *";
        var isActive = (await _settings.GetAsync("Pipeline:IsActive")) == "true";
        return Ok(new ScheduleResponse { CronExpression = cron, IsActive = isActive });
    }

    [HttpPut("schedule")]
    public async Task<IActionResult> UpdateSchedule([FromBody] UpdateScheduleRequest request)
    {
        await _settings.SetAsync("Pipeline:CronSchedule", request.CronExpression);
        return Ok(new { message = "Schedule updated" });
    }
}
