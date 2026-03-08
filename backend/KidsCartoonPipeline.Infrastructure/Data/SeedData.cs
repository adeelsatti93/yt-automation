using KidsCartoonPipeline.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsCartoonPipeline.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();

        await SeedSettingsAsync(db);

        if (!await db.Characters.AnyAsync())
        {
            db.Characters.AddRange(
                new Character
                {
                    Name = "Leo",
                    Description = "A small friendly orange lion cub with a fluffy mane, big brown eyes, and a warm smile. Wears a red bandana.",
                    ImagePromptStyle = "soft rounded features, warm lighting, bright and cheerful expression",
                    VoiceName = "Choose in Settings",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Character
                {
                    Name = "Luna",
                    Description = "A small curious purple bunny with long floppy ears, big blue eyes, and a tiny pink nose. Wears a yellow sunflower hairclip.",
                    ImagePromptStyle = "soft pastel tones, gentle expression, whimsical feel",
                    VoiceName = "Choose in Settings",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }

        if (!await db.TopicSeeds.AnyAsync())
        {
            db.TopicSeeds.AddRange(
                new TopicSeed { Title = "Leo learns to count to 10", TargetMoral = "Learning is fun", Priority = 10, CreatedAt = DateTime.UtcNow },
                new TopicSeed { Title = "Luna shares her carrots with friends", TargetMoral = "Sharing is caring", Priority = 9, CreatedAt = DateTime.UtcNow },
                new TopicSeed { Title = "Leo and Luna discover the colors of the rainbow", TargetMoral = "The world is beautiful", Priority = 8, CreatedAt = DateTime.UtcNow },
                new TopicSeed { Title = "Luna learns why we wash our hands", TargetMoral = "Staying clean keeps us healthy", Priority = 7, CreatedAt = DateTime.UtcNow },
                new TopicSeed { Title = "Leo helps a lost bird find its way home", TargetMoral = "Kindness matters", Priority = 6, CreatedAt = DateTime.UtcNow }
            );
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedSettingsAsync(AppDbContext db)
    {
        var defaults = new List<AppSetting>
        {
            new() { Key = "Anthropic:ApiKey", Category = "ApiKeys", DisplayName = "Anthropic API Key", IsSecret = true, IsRequired = true, Description = "Claude AI for scripts and SEO" },
            new() { Key = "OpenAI:ApiKey", Category = "ApiKeys", DisplayName = "OpenAI API Key", IsSecret = true, IsRequired = true, Description = "DALL-E 3 for image generation" },
            new() { Key = "ElevenLabs:ApiKey", Category = "ApiKeys", DisplayName = "ElevenLabs API Key", IsSecret = true, IsRequired = true, Description = "Text-to-speech for character voices" },
            new() { Key = "Suno:ApiKey", Category = "ApiKeys", DisplayName = "Suno API Key", IsSecret = true, IsRequired = false, Description = "Background music generation" },
            new() { Key = "Fal:ApiKey", Category = "ApiKeys", DisplayName = "Fal.ai API Key", IsSecret = true, IsRequired = false, Description = "Required for Kling AI animation (get free $10 credits at fal.ai)" },
            new() { Key = "YouTube:ClientId", Category = "ApiKeys", DisplayName = "YouTube Client ID", IsSecret = false, IsRequired = true, Description = "Google OAuth Client ID" },
            new() { Key = "YouTube:ClientSecret", Category = "ApiKeys", DisplayName = "YouTube Client Secret", IsSecret = true, IsRequired = true, Description = "Google OAuth Client Secret" },
            new() { Key = "YouTube:RefreshToken", Category = "ApiKeys", DisplayName = "YouTube Refresh Token", IsSecret = true, IsRequired = true, Description = "OAuth refresh token for your channel" },

            new() { Key = "Pipeline:IsActive", Category = "Pipeline", DisplayName = "Auto-run Pipeline", Value = "true", IsSecret = false },
            new() { Key = "Pipeline:CronSchedule", Category = "Pipeline", DisplayName = "Schedule (Cron)", Value = "0 */6 * * *", IsSecret = false },
            new() { Key = "Pipeline:MaxConcurrent", Category = "Pipeline", DisplayName = "Max Concurrent Episodes", Value = "1", IsSecret = false },
            new() { Key = "Pipeline:DefaultPublishTime", Category = "Pipeline", DisplayName = "Default Publish Time", Value = "09:00", IsSecret = false },
            new() { Key = "Pipeline:PublishDays", Category = "Pipeline", DisplayName = "Publish Days", Value = "Saturday,Sunday", IsSecret = false },
            new() { Key = "Video:Provider", Category = "Pipeline", DisplayName = "Video Engine", Value = "FFmpeg", IsSecret = false, Description = "FFmpeg = 2D animated slideshow (free). Kling = AI animated video with lip sync (~$6/episode, requires Fal.ai API key)." },

            new() { Key = "Images:GlobalStyle", Category = "Images", DisplayName = "Global Art Style", Value = "2D flat cartoon, bright saturated colors, Pixar-inspired, child-friendly, soft shadows, no text, no watermarks, 16:9 aspect ratio", IsSecret = false },
            new() { Key = "Images:Quality", Category = "Images", DisplayName = "Image Quality", Value = "hd", IsSecret = false },
            new() { Key = "Images:Size", Category = "Images", DisplayName = "Image Size", Value = "1792x1024", IsSecret = false },

            new() { Key = "Prompts:Script", Category = "Prompts", DisplayName = "Script Generation Prompt", Value = DefaultScriptPrompt, IsSecret = false },
            new() { Key = "Prompts:SeoTitle", Category = "Prompts", DisplayName = "SEO Title Prompt", IsSecret = false },
            new() { Key = "Prompts:SeoDescription", Category = "Prompts", DisplayName = "SEO Description Prompt", IsSecret = false },
            new() { Key = "Prompts:ImageBuilder", Category = "Prompts", DisplayName = "Image Prompt Builder", IsSecret = false },

            new() { Key = "YouTube:CategoryId", Category = "YouTube", DisplayName = "Video Category", Value = "27", IsSecret = false },
            new() { Key = "YouTube:Privacy", Category = "YouTube", DisplayName = "Default Privacy", Value = "public", IsSecret = false },
            new() { Key = "YouTube:MadeForKids", Category = "YouTube", DisplayName = "Made For Kids", Value = "true", IsSecret = false },
            new() { Key = "YouTube:AutoPublish", Category = "YouTube", DisplayName = "Auto-Publish", Value = "true", IsSecret = false },
            new() { Key = "YouTube:DescriptionSuffix", Category = "YouTube", DisplayName = "Description Suffix", Value = "\n\n#KidsCartoon #AnimatedCartoon #ChildrensEducation #KidsTV", IsSecret = false },
        };

        foreach (var setting in defaults)
        {
            if (!await db.AppSettings.AnyAsync(s => s.Key == setting.Key))
            {
                setting.CreatedAt = DateTime.UtcNow;
                setting.UpdatedAt = DateTime.UtcNow;
                db.AppSettings.Add(setting);
            }
        }

        await db.SaveChangesAsync();
    }

    private const string DefaultScriptPrompt = """
You are an expert children's cartoon scriptwriter creating content for kids aged 3-7.
Your scripts must be:
- Safe, positive, and educational
- Simple language (max 8 words per sentence)
- Feature the exact characters provided
- Include a clear moral lesson
- Be exactly 8-12 scenes (approx 3 minutes total)

Topic: {topic}
Characters: {characters_json}
Target moral: {moral}

Return ONLY valid JSON, no markdown, no code fences, no explanations:
{"title":"Catchy episode title","summary":"2-sentence description","moral":"One-sentence lesson","scenes":[{"scene_number":1,"duration_seconds":20,"background":"Visual description","action":"What happens","dialogue":[{"character":"Name","line":"Text","tone":"excited"}]}]}
""";
}
