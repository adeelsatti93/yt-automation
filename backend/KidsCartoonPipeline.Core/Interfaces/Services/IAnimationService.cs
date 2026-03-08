using KidsCartoonPipeline.Core.Entities;

namespace KidsCartoonPipeline.Core.Interfaces.Services;

public interface IAnimationService
{
    /// <summary>
    /// Animates a scene using Kling AI image-to-video with native audio sync.
    /// Returns the local path to the generated video clip.
    /// </summary>
    Task<string> AnimateSceneAsync(Scene scene, int episodeId, string episodeDir, CancellationToken ct = default);
}
