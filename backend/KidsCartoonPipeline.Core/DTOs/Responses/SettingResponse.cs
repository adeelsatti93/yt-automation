namespace KidsCartoonPipeline.Core.DTOs.Responses;

public class SettingResponse
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSecret { get; set; }
    public bool IsRequired { get; set; }
}

public class SettingsStatusResponse
{
    public List<string> Configured { get; set; } = [];
    public List<string> Missing { get; set; } = [];
    public bool IsFullyConfigured { get; set; }
    public List<string> RequiredMissing { get; set; } = [];
}
