using KidsCartoonPipeline.Core.DTOs.Requests;
using KidsCartoonPipeline.Core.DTOs.Responses;
using KidsCartoonPipeline.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KidsCartoonPipeline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settings;

    public SettingsController(ISettingsService settings) => _settings = settings;

    [HttpGet]
    public async Task<ActionResult<List<SettingResponse>>> GetAll()
    {
        var settings = await _settings.GetAllAsync();
        return Ok(settings.Select(s => new SettingResponse
        {
            Id = s.Id, Key = s.Key, Category = s.Category, DisplayName = s.DisplayName,
            Description = s.Description, IsSecret = s.IsSecret, IsRequired = s.IsRequired,
            Value = s.IsSecret && !string.IsNullOrEmpty(s.Value) ? "••••••••" : s.Value
        }).ToList());
    }

    [HttpGet("{key}")]
    public async Task<ActionResult<SettingResponse>> GetByKey(string key)
    {
        var s = await _settings.GetSettingEntityAsync(key);
        if (s == null) return NotFound();
        return Ok(new SettingResponse
        {
            Id = s.Id, Key = s.Key, Category = s.Category, DisplayName = s.DisplayName,
            Description = s.Description, IsSecret = s.IsSecret, IsRequired = s.IsRequired,
            Value = s.IsSecret && !string.IsNullOrEmpty(s.Value) ? "••••••••" : s.Value
        });
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> Update(string key, [FromBody] SaveSettingRequest request)
    {
        await _settings.SetAsync(key, request.Value);
        return Ok(new { message = "Setting updated" });
    }

    [HttpPut("batch")]
    public async Task<IActionResult> UpdateBatch([FromBody] List<BatchSaveSettingRequest> requests)
    {
        await _settings.SaveBatchAsync(requests.Select(r => (r.Key, r.Value)).ToList());
        return Ok(new { message = "Settings updated" });
    }

    [HttpPost("test/{service}")]
    public async Task<ActionResult> TestConnection(string service)
    {
        var success = await _settings.TestApiKeyAsync(service);
        return Ok(new { success, message = success ? "Connected successfully" : "Connection failed" });
    }

    [HttpGet("status")]
    public async Task<ActionResult<SettingsStatusResponse>> GetStatus()
    {
        var (configured, missing, requiredMissing) = await _settings.GetConfigurationStatusAsync();
        return Ok(new SettingsStatusResponse
        {
            Configured = configured, Missing = missing, RequiredMissing = requiredMissing,
            IsFullyConfigured = requiredMissing.Count == 0
        });
    }
}
