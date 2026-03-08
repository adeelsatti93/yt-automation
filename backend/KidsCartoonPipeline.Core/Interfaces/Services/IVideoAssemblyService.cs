using KidsCartoonPipeline.Core.Entities;

namespace KidsCartoonPipeline.Core.Interfaces.Services;

public interface IVideoAssemblyService
{
    /// <summary>Full FFmpeg pipeline: generates scene clips + assembles final video.</summary>
    Task<string> AssembleVideoAsync(Episode episode);

    /// <summary>
    /// Concat pre-made silent clips (e.g. from Kling AI), overlay dialogue audio,
    /// then optionally mix background music.
    /// </summary>
    Task<string> AssembleFromClipsAsync(List<string> clipRelPaths, Episode episode);
}
