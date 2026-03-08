using KidsCartoonPipeline.Core.Entities;

namespace KidsCartoonPipeline.Core.Interfaces.Services;

public interface IVideoAssemblyService
{
    Task<string> AssembleVideoAsync(Episode episode);
}
