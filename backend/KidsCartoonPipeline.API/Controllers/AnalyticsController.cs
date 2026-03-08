using KidsCartoonPipeline.Core.DTOs.Responses;
using KidsCartoonPipeline.Core.Enums;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidsCartoonPipeline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AnalyticsController(AppDbContext db) => _db = db;

    [HttpGet("summary")]
    public async Task<ActionResult<AnalyticsSummaryResponse>> GetSummary()
    {
        var published = await _db.Episodes.CountAsync(e => e.Status == EpisodeStatus.Published);
        var total = await _db.Episodes.CountAsync();
        var uploads = await _db.YoutubeUploads.ToListAsync();

        return Ok(new AnalyticsSummaryResponse
        {
            TotalEpisodes = total,
            Published = published,
            TotalViews = uploads.Sum(u => u.Views ?? 0),
            TotalWatchTimeHours = uploads.Sum(u => u.WatchTimeHours ?? 0),
            TotalRevenue = uploads.Sum(u => u.EstimatedRevenue ?? 0),
            AvgViewsPerEpisode = published > 0 ? uploads.Sum(u => u.Views ?? 0) / (double)published : 0
        });
    }

    [HttpGet("episodes")]
    public async Task<ActionResult<List<EpisodeAnalyticsResponse>>> GetEpisodeAnalytics()
    {
        var uploads = await _db.YoutubeUploads.Include(u => u.Episode).ToListAsync();
        return Ok(uploads.Select(u => new EpisodeAnalyticsResponse
        {
            EpisodeId = u.EpisodeId, YoutubeId = u.VideoId, Title = u.Episode?.Title,
            Views = u.Views ?? 0, WatchTimeHours = u.WatchTimeHours ?? 0,
            Revenue = u.EstimatedRevenue ?? 0, PublishedDate = u.Episode?.PublishedAt,
            YouTubeLink = u.Url
        }).ToList());
    }

    [HttpGet("sync")]
    public async Task<IActionResult> Sync()
    {
        // Placeholder - would fetch from YouTube Analytics API
        return Ok(new { message = "Analytics sync triggered" });
    }
}
