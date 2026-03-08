namespace KidsCartoonPipeline.Core.Entities;

public class AppSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSecret { get; set; }
    public bool IsRequired { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
