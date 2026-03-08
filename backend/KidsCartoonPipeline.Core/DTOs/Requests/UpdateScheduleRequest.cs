namespace KidsCartoonPipeline.Core.DTOs.Requests;

public class UpdateScheduleRequest
{
    public string CronExpression { get; set; } = string.Empty;
}
