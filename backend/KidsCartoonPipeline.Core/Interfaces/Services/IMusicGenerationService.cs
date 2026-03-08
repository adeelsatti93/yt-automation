namespace KidsCartoonPipeline.Core.Interfaces.Services;

public interface IMusicGenerationService
{
    Task<string> GenerateBackgroundMusicAsync(string episodeSummary, int episodeId, int durationSeconds = 180);
}
