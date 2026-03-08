using KidsCartoonPipeline.Core.Entities;

namespace KidsCartoonPipeline.Core.Interfaces.Services;

public interface IImageGenerationService
{
    Task<string> GenerateSceneImageAsync(Scene scene, List<Character> characters, int episodeId);
    Task<string> GenerateThumbnailAsync(string prompt, int episodeId);
}
