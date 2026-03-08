namespace KidsCartoonPipeline.Core.DTOs.Requests;

public class SaveSettingRequest
{
    public string Value { get; set; } = string.Empty;
}

public class BatchSaveSettingRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
