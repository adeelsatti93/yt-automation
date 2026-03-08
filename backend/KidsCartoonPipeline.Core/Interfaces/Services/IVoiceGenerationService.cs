using KidsCartoonPipeline.Core.Entities;

namespace KidsCartoonPipeline.Core.Interfaces.Services;

public interface IVoiceGenerationService
{
    Task<string> GenerateDialogueAudioAsync(DialogueLine line, Character character, int episodeId);
    Task<List<VoiceInfo>> GetAvailableVoicesAsync();
    Task<string> GenerateTestAudioAsync(string voiceId, string text);
}

public class VoiceInfo
{
    public string VoiceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PreviewUrl { get; set; }
    public string? Category { get; set; }
}
