namespace KidsCartoonPipeline.Core.DTOs.Requests;

public class CreateTopicRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TargetMoral { get; set; }
    public int Priority { get; set; } = 0;
}
