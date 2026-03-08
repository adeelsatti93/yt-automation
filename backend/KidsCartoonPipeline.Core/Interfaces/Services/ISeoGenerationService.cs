using KidsCartoonPipeline.Core.Entities;

namespace KidsCartoonPipeline.Core.Interfaces.Services;

public interface ISeoGenerationService
{
    Task<SeoResult> GenerateSeoMetadataAsync(Episode episode, List<Character> characters);
}

public class SeoResult
{
    public string SeoTitle { get; set; } = string.Empty;
    public string SeoDescription { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public string ThumbnailPrompt { get; set; } = string.Empty;
}
