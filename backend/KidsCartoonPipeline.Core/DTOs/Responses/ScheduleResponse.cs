namespace KidsCartoonPipeline.Core.DTOs.Responses;

public class ScheduleResponse
{
    public string CronExpression { get; set; } = string.Empty;
    public DateTime? NextRun { get; set; }
    public bool IsActive { get; set; }
}
