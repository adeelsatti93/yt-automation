using KidsCartoonPipeline.Core.Entities;

namespace KidsCartoonPipeline.Core.Interfaces.Services;

public interface IScriptGenerationService
{
    Task<Episode> GenerateScriptAsync(string topic, List<Character> characters, string? moral = null);
    Task<List<string>> GenerateTopicIdeasAsync(List<Character> characters, int count = 10);
}
