# 🎬 AI Kids Cartoon YouTube Pipeline — Complete E2E Agent Build Plan

> **📌 AGENT INSTRUCTIONS:**
> This document is a **complete, self-contained build specification**. You are to build every file listed, implement every feature described, write every test first (TDD), and produce a fully working application. Do **not** skip any section. Do **not** ask clarifying questions — every decision is already made here. If a section says "build X", build X completely and correctly. Proceed section by section in the order given. After completing each section, confirm it compiles and all tests pass before moving to the next.

---

## 📋 Table of Contents

1. [Project Overview & Goals](#1-project-overview--goals)
2. [Tech Stack & Exact Versions](#2-tech-stack--exact-versions)
3. [TDD Strategy & Testing Rules](#3-tdd-strategy--testing-rules)
4. [Full Solution & File Tree](#4-full-solution--file-tree)
5. [Database Schema](#5-database-schema)
6. [Backend — .NET 9 API (Full Implementation)](#6-backend--net-9-api)
7. [Pipeline Worker Services](#7-pipeline-worker-services)
8. [Frontend — React + Bootstrap 5](#8-frontend--react--bootstrap-5)
9. [UI/UX Guidelines & Screen Specs](#9-uiux-guidelines--screen-specs)
10. [Settings & Configuration System](#10-settings--configuration-system)
11. [AI Service Integrations](#11-ai-service-integrations)
12. [YouTube Integration](#12-youtube-integration)
13. [In-Memory Cache Strategy](#13-in-memory-cache-strategy)
14. [Docker & Environment Setup](#14-docker--environment-setup)
15. [Seed Data & Initial Setup](#15-seed-data--initial-setup)
16. [Full Implementation Order](#16-full-implementation-order)

---

## 1. Project Overview & Goals

### What This App Does
A **semi-automated AI pipeline** that produces and publishes children's cartoon YouTube videos with minimal human effort:

1. AI writes episode scripts (Claude API)
2. AI generates cartoon scene images (DALL-E 3)
3. AI synthesizes character voices (ElevenLabs TTS)
4. AI generates background music (Suno API)
5. FFmpeg assembles the final MP4 video
6. AI generates SEO metadata + thumbnail
7. Human reviews & approves in dashboard (~5 min/video)
8. Auto-uploads approved video to YouTube

### Key Principle: **Everything Configurable from UI**
The user should be able to:
- Enter all API keys from the Settings page (no `.env` file editing needed)
- Configure all pipeline behavior (prompts, styles, schedules) from the UI
- The app must work end-to-end once API keys are entered in Settings

### Human Touchpoints
- **Topic seeds**: Add/manage episode ideas
- **Character library**: Create/edit characters
- **Review gate**: Watch preview, approve or request changes
- **Settings**: Enter API keys and configure pipeline behavior

---

## 2. Tech Stack & Exact Versions

### Backend
```
Runtime:        .NET 9
Web Framework:  ASP.NET Core 9 Web API (minimal API style with Controllers)
ORM:            Entity Framework Core 9
Database:       MySQL 8.x (via Pomelo.EntityFrameworkCore.MySql 9.x)
Cache:          Microsoft.Extensions.Caching.Memory (IMemoryCache, built-in)
Job Scheduling: Hangfire 1.8.x (with Hangfire.MySql storage)
HTTP Client:    System.Net.Http.HttpClient (typed clients)
JSON:           System.Text.Json
Validation:     FluentValidation 11.x
Mapping:        Mapster 7.x
Logging:        Serilog 4.x (console + file sinks)
Testing:        xUnit 2.9.x + Moq 4.20.x + FluentAssertions 6.x
Test HTTP:      Microsoft.AspNetCore.Mvc.Testing
```

### Frontend
```
Runtime:        Node 20+
Framework:      React 18.x
Build Tool:     Vite 5.x
UI Library:     Bootstrap 5.3.x + Bootstrap Icons 1.11.x
HTTP Client:    Axios 1.7.x
Routing:        React Router DOM 6.x
State:          React Context API + useReducer
Video Player:   react-player 2.x
Toasts:         react-toastify 10.x
Forms:          react-hook-form 7.x
Charts:         recharts 2.x
Testing:        Vitest + React Testing Library
```

### Infrastructure
```
Containerization: Docker + Docker Compose
Database:         MySQL 8 (Docker container)
File Storage:     Local filesystem (./storage/ directory, Docker volume)
FFmpeg:           ffmpeg 6.x (installed in Docker image)
```

---

## 3. TDD Strategy & Testing Rules

> **AGENT RULE: Write the test FIRST. Then write the implementation. Every single service, controller, and utility must have tests written before the implementation file is created.**

### Test Project Structure
```
backend/
  KidsCartoonPipeline.Tests/
    Unit/
      Services/
        ClaudeScriptServiceTests.cs
        DalleImageServiceTests.cs
        ElevenLabsVoiceServiceTests.cs
        FfmpegVideoServiceTests.cs
        SeoGenerationServiceTests.cs
        MemoryCacheServiceTests.cs
        PipelineOrchestratorTests.cs
      Repositories/
        EpisodeRepositoryTests.cs
        CharacterRepositoryTests.cs
        TopicRepositoryTests.cs
        SettingsRepositoryTests.cs
      Validators/
        CreateEpisodeRequestValidatorTests.cs
        CharacterValidatorTests.cs
        ApiKeyValidatorTests.cs
    Integration/
      Controllers/
        EpisodesControllerTests.cs
        CharactersControllerTests.cs
        TopicsControllerTests.cs
        SettingsControllerTests.cs
        PipelineControllerTests.cs
      Pipeline/
        FullPipelineIntegrationTests.cs

frontend/
  src/
    __tests__/
      components/
        EpisodeCard.test.jsx
        PipelineStatus.test.jsx
        VideoPlayer.test.jsx
        SettingsForm.test.jsx
        CharacterForm.test.jsx
      hooks/
        useEpisodes.test.js
        usePipeline.test.js
        useSettings.test.js
      pages/
        Dashboard.test.jsx
        Settings.test.jsx
        Characters.test.jsx
```

### TDD Rules the Agent Must Follow

1. **Red → Green → Refactor** for every feature
2. Every public method on every service must have at minimum:
   - A test for the happy path
   - A test for null/invalid input
   - A test for external API failure (mock throws exception)
3. Every API controller endpoint must have:
   - A test for 200 OK with valid input
   - A test for 400 Bad Request with invalid input
   - A test for 404 Not Found
   - A test for 500 when service throws
4. Use `Moq` to mock all external dependencies
5. Use `FluentAssertions` for all assertions (`result.Should().Be(...)`)
6. Integration tests use `WebApplicationFactory<Program>` with in-memory SQLite for DB
7. All tests must pass with `dotnet test` before moving to next layer

### Example TDD Pattern to Follow

```csharp
// STEP 1: Write test first
// File: Tests/Unit/Services/ClaudeScriptServiceTests.cs

public class ClaudeScriptServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ISettingsService> _settingsServiceMock;
    private readonly Mock<ILogger<ClaudeScriptService>> _loggerMock;
    private readonly ClaudeScriptService _sut;

    public ClaudeScriptServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _settingsServiceMock = new Mock<ISettingsService>();
        _loggerMock = new Mock<ILogger<ClaudeScriptService>>();
        _sut = new ClaudeScriptService(
            _httpClientFactoryMock.Object,
            _settingsServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GenerateScriptAsync_ValidTopic_ReturnsStructuredEpisode()
    {
        // Arrange
        var topic = "Leo the Lion counts to 10";
        var characters = new List<Character> { new() { Name = "Leo", Description = "A friendly orange lion cub" } };
        _settingsServiceMock.Setup(s => s.GetApiKeyAsync("Anthropic"))
            .ReturnsAsync("test-api-key");
        // ... mock HTTP response

        // Act
        var result = await _sut.GenerateScriptAsync(topic, characters);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().NotBeNullOrEmpty();
        result.Scenes.Should().HaveCountGreaterThan(0);
        result.Scenes.All(s => s.DialogueLines.Any()).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateScriptAsync_MissingApiKey_ThrowsInvalidOperationException()
    {
        _settingsServiceMock.Setup(s => s.GetApiKeyAsync("Anthropic")).ReturnsAsync((string?)null);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GenerateScriptAsync("topic", new List<Character>())
        );
    }

    [Fact]
    public async Task GenerateScriptAsync_ApiReturnsError_ThrowsExternalServiceException()
    {
        // ... mock 500 response
        await Assert.ThrowsAsync<ExternalServiceException>(
            () => _sut.GenerateScriptAsync("topic", new List<Character>())
        );
    }
}

// STEP 2: THEN write ClaudeScriptService.cs to make these tests pass
```

---

## 4. Full Solution & File Tree

```
KidsCartoonPipeline/
│
├── docker-compose.yml
├── docker-compose.override.yml
├── .env.example
├── README.md
├── storage/                          # Runtime asset storage (Docker volume)
│   ├── images/
│   ├── audio/
│   ├── music/
│   ├── videos/
│   └── thumbnails/
│
├── backend/
│   ├── KidsCartoonPipeline.sln
│   │
│   ├── KidsCartoonPipeline.API/
│   │   ├── KidsCartoonPipeline.API.csproj
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Dockerfile
│   │   ├── Controllers/
│   │   │   ├── EpisodesController.cs
│   │   │   ├── CharactersController.cs
│   │   │   ├── TopicsController.cs
│   │   │   ├── PipelineController.cs
│   │   │   ├── SettingsController.cs
│   │   │   ├── AnalyticsController.cs
│   │   │   └── AssetsController.cs
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   │
│   ├── KidsCartoonPipeline.Core/
│   │   ├── KidsCartoonPipeline.Core.csproj
│   │   ├── Entities/
│   │   │   ├── Episode.cs
│   │   │   ├── Scene.cs
│   │   │   ├── DialogueLine.cs
│   │   │   ├── Character.cs
│   │   │   ├── TopicSeed.cs
│   │   │   ├── PipelineJob.cs
│   │   │   ├── AppSetting.cs
│   │   │   └── YoutubeUpload.cs
│   │   ├── Enums/
│   │   │   ├── EpisodeStatus.cs
│   │   │   ├── PipelineStage.cs
│   │   │   └── JobStatus.cs
│   │   ├── DTOs/
│   │   │   ├── Requests/
│   │   │   │   ├── CreateEpisodeRequest.cs
│   │   │   │   ├── CreateCharacterRequest.cs
│   │   │   │   ├── CreateTopicRequest.cs
│   │   │   │   ├── ApproveEpisodeRequest.cs
│   │   │   │   ├── RegenerateStageRequest.cs
│   │   │   │   └── SaveSettingRequest.cs
│   │   │   └── Responses/
│   │   │       ├── EpisodeResponse.cs
│   │   │       ├── SceneResponse.cs
│   │   │       ├── CharacterResponse.cs
│   │   │       ├── TopicResponse.cs
│   │   │       ├── PipelineJobResponse.cs
│   │   │       ├── PipelineStatusResponse.cs
│   │   │       ├── SettingResponse.cs
│   │   │       └── AnalyticsResponse.cs
│   │   ├── Exceptions/
│   │   │   ├── ExternalServiceException.cs
│   │   │   ├── NotFoundException.cs
│   │   │   └── PipelineException.cs
│   │   └── Interfaces/
│   │       ├── Repositories/
│   │       │   ├── IEpisodeRepository.cs
│   │       │   ├── ICharacterRepository.cs
│   │       │   ├── ITopicRepository.cs
│   │       │   ├── IPipelineJobRepository.cs
│   │       │   └── ISettingsRepository.cs
│   │       └── Services/
│   │           ├── IScriptGenerationService.cs
│   │           ├── IImageGenerationService.cs
│   │           ├── IVoiceGenerationService.cs
│   │           ├── IMusicGenerationService.cs
│   │           ├── IVideoAssemblyService.cs
│   │           ├── ISeoGenerationService.cs
│   │           ├── IYouTubeService.cs
│   │           ├── IAssetStorageService.cs
│   │           ├── ICacheService.cs
│   │           ├── ISettingsService.cs
│   │           └── IPipelineOrchestrator.cs
│   │
│   ├── KidsCartoonPipeline.Infrastructure/
│   │   ├── KidsCartoonPipeline.Infrastructure.csproj
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── EpisodeConfiguration.cs
│   │   │   │   ├── SceneConfiguration.cs
│   │   │   │   ├── CharacterConfiguration.cs
│   │   │   │   ├── TopicSeedConfiguration.cs
│   │   │   │   ├── AppSettingConfiguration.cs
│   │   │   │   └── PipelineJobConfiguration.cs
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   │   ├── EpisodeRepository.cs
│   │   │   ├── CharacterRepository.cs
│   │   │   ├── TopicRepository.cs
│   │   │   ├── PipelineJobRepository.cs
│   │   │   └── SettingsRepository.cs
│   │   ├── Services/
│   │   │   ├── AI/
│   │   │   │   ├── ClaudeScriptService.cs
│   │   │   │   ├── DalleImageService.cs
│   │   │   │   ├── ElevenLabsVoiceService.cs
│   │   │   │   ├── SunoMusicService.cs
│   │   │   │   └── SeoGenerationService.cs
│   │   │   ├── Video/
│   │   │   │   └── FfmpegVideoService.cs
│   │   │   ├── Storage/
│   │   │   │   └── LocalAssetStorageService.cs
│   │   │   ├── Cache/
│   │   │   │   └── MemoryCacheService.cs
│   │   │   ├── Settings/
│   │   │   │   └── SettingsService.cs
│   │   │   ├── YouTube/
│   │   │   │   └── YouTubeUploadService.cs
│   │   │   └── Pipeline/
│   │   │       └── PipelineOrchestrator.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── KidsCartoonPipeline.Worker/
│   │   ├── KidsCartoonPipeline.Worker.csproj
│   │   ├── Program.cs
│   │   └── Jobs/
│   │       ├── PipelineTriggerJob.cs
│   │       └── AnalyticsSyncJob.cs
│   │
│   └── KidsCartoonPipeline.Tests/
│       ├── KidsCartoonPipeline.Tests.csproj
│       ├── TestHelpers/
│       │   ├── TestWebApplicationFactory.cs
│       │   ├── MockHttpMessageHandler.cs
│       │   └── TestDataBuilder.cs
│       ├── Unit/
│       │   ├── Services/  (all service test files)
│       │   ├── Repositories/  (all repo test files)
│       │   └── Validators/  (all validator test files)
│       └── Integration/
│           ├── Controllers/  (all controller test files)
│           └── Pipeline/
│
└── frontend/
    ├── package.json
    ├── vite.config.js
    ├── index.html
    ├── Dockerfile
    ├── .env.example
    └── src/
        ├── main.jsx
        ├── App.jsx
        ├── api/
        │   ├── axiosInstance.js
        │   ├── episodesApi.js
        │   ├── charactersApi.js
        │   ├── topicsApi.js
        │   ├── pipelineApi.js
        │   ├── settingsApi.js
        │   └── analyticsApi.js
        ├── context/
        │   ├── AppContext.jsx
        │   ├── PipelineContext.jsx
        │   └── SettingsContext.jsx
        ├── hooks/
        │   ├── useEpisodes.js
        │   ├── usePipeline.js
        │   ├── useCharacters.js
        │   ├── useTopics.js
        │   └── useSettings.js
        ├── pages/
        │   ├── Dashboard.jsx
        │   ├── Episodes.jsx
        │   ├── EpisodeDetail.jsx
        │   ├── Characters.jsx
        │   ├── Topics.jsx
        │   ├── Settings.jsx
        │   └── Analytics.jsx
        ├── components/
        │   ├── layout/
        │   │   ├── Sidebar.jsx
        │   │   ├── TopBar.jsx
        │   │   └── Layout.jsx
        │   ├── episodes/
        │   │   ├── EpisodeCard.jsx
        │   │   ├── EpisodeList.jsx
        │   │   ├── EpisodeStatusBadge.jsx
        │   │   └── VideoPreviewModal.jsx
        │   ├── pipeline/
        │   │   ├── PipelineStatusBar.jsx
        │   │   ├── PipelineStageIndicator.jsx
        │   │   └── PipelineLogFeed.jsx
        │   ├── characters/
        │   │   ├── CharacterCard.jsx
        │   │   └── CharacterForm.jsx
        │   ├── topics/
        │   │   ├── TopicCard.jsx
        │   │   └── TopicForm.jsx
        │   ├── settings/
        │   │   ├── ApiKeyField.jsx
        │   │   ├── SettingsSection.jsx
        │   │   └── ConnectionTestButton.jsx
        │   ├── shared/
        │   │   ├── LoadingSpinner.jsx
        │   │   ├── ConfirmModal.jsx
        │   │   ├── EmptyState.jsx
        │   │   └── ErrorAlert.jsx
        │   └── analytics/
        │       ├── StatsCard.jsx
        │       └── PerformanceChart.jsx
        ├── utils/
        │   ├── formatters.js
        │   ├── statusHelpers.js
        │   └── validators.js
        └── __tests__/  (all test files)
```

---

## 5. Database Schema

### Entity: `AppSetting` (All API keys and config stored here)
```csharp
// Core/Entities/AppSetting.cs
public class AppSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;        // e.g. "Anthropic:ApiKey"
    public string? Value { get; set; }                     // encrypted at rest
    public string Category { get; set; } = string.Empty;  // "ApiKeys", "Pipeline", "YouTube", "Prompts"
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSecret { get; set; }                     // masks value in UI
    public bool IsRequired { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Entity: `Character`
```csharp
public class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;    // Used in every image prompt
    public string? VoiceId { get; set; }                       // ElevenLabs voice ID
    public string? VoiceName { get; set; }                     // ElevenLabs display name
    public string? ImagePromptStyle { get; set; }              // Locked visual style descriptor
    public string? AvatarUrl { get; set; }                     // Preview image URL
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Episode> Episodes { get; set; } = [];
}
```

### Entity: `TopicSeed`
```csharp
public class TopicSeed
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;          // e.g. "Leo counts to 10"
    public string? Description { get; set; }
    public string? TargetMoral { get; set; }                   // e.g. "sharing is caring"
    public int Priority { get; set; } = 0;
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public int? EpisodeId { get; set; }
    public Episode? Episode { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Entity: `Episode`
```csharp
public class Episode
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Moral { get; set; }
    public EpisodeStatus Status { get; set; } = EpisodeStatus.TopicQueued;
    public int? TopicSeedId { get; set; }
    public TopicSeed? TopicSeed { get; set; }
    
    // Pipeline asset paths (relative to storage root)
    public string? VideoPath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? AudioMixPath { get; set; }
    
    // SEO metadata
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoTags { get; set; }               // JSON array string
    
    // YouTube
    public string? YouTubeVideoId { get; set; }
    public string? YouTubeUrl { get; set; }
    public DateTime? ScheduledPublishAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    
    // Review
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    
    // Pipeline tracking
    public string? CurrentStageError { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public ICollection<Scene> Scenes { get; set; } = [];
    public ICollection<PipelineJob> PipelineJobs { get; set; } = [];
    public ICollection<Character> Characters { get; set; } = [];
}
```

### Entity: `Scene`
```csharp
public class Scene
{
    public int Id { get; set; }
    public int EpisodeId { get; set; }
    public Episode Episode { get; set; } = null!;
    public int SceneNumber { get; set; }
    public int DurationSeconds { get; set; }
    public string? BackgroundDescription { get; set; }   // Used as image prompt base
    public string? ActionDescription { get; set; }
    public string? ImagePath { get; set; }               // Generated scene image
    public string? ImagePromptUsed { get; set; }         // For debugging/regeneration
    public string? VideoClipPath { get; set; }           // Individual scene video clip
    public ICollection<DialogueLine> DialogueLines { get; set; } = [];
}
```

### Entity: `DialogueLine`
```csharp
public class DialogueLine
{
    public int Id { get; set; }
    public int SceneId { get; set; }
    public Scene Scene { get; set; } = null!;
    public int LineOrder { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Tone { get; set; }                   // "excited", "sad", "calm"
    public string? AudioPath { get; set; }              // Generated TTS audio file
}
```

### Entity: `PipelineJob`
```csharp
public class PipelineJob
{
    public int Id { get; set; }
    public int EpisodeId { get; set; }
    public Episode Episode { get; set; } = null!;
    public PipelineStage Stage { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public string? ErrorMessage { get; set; }
    public string? LogOutput { get; set; }              // JSON array of log lines
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

### Enums
```csharp
// EpisodeStatus.cs
public enum EpisodeStatus
{
    TopicQueued = 0,
    GeneratingScript = 1,
    GeneratingImages = 2,
    GeneratingAudio = 3,
    GeneratingMusic = 4,
    RenderingVideo = 5,
    GeneratingSeo = 6,
    PendingReview = 7,      // <-- Human review gate
    Approved = 8,
    Uploading = 9,
    Scheduled = 10,
    Published = 11,
    Failed = 12,
    Rejected = 13
}

// PipelineStage.cs
public enum PipelineStage
{
    ScriptGeneration = 1,
    ImageGeneration = 2,
    VoiceGeneration = 3,
    MusicGeneration = 4,
    VideoAssembly = 5,
    SeoGeneration = 6,
    YouTubeUpload = 7
}

// JobStatus.cs
public enum JobStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Skipped = 4
}
```

### EF Core DbContext
```csharp
// Infrastructure/Data/AppDbContext.cs
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Episode> Episodes => Set<Episode>();
    public DbSet<Scene> Scenes => Set<Scene>();
    public DbSet<DialogueLine> DialogueLines => Set<DialogueLine>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<TopicSeed> TopicSeeds => Set<TopicSeed>();
    public DbSet<PipelineJob> PipelineJobs => Set<PipelineJob>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<YoutubeUpload> YoutubeUploads => Set<YoutubeUpload>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly
        );
    }
}
```

### EF Configuration Example
```csharp
// Infrastructure/Data/Configurations/EpisodeConfiguration.cs
public class EpisodeConfiguration : IEntityTypeConfiguration<Episode>
{
    public void Configure(EntityTypeBuilder<Episode> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(500);
        builder.Property(e => e.SeoTitle).HasMaxLength(100);
        builder.Property(e => e.SeoTags).HasColumnType("text");
        builder.Property(e => e.Status).HasConversion<string>();
        builder.HasMany(e => e.Scenes)
               .WithOne(s => s.Episode)
               .HasForeignKey(s => s.EpisodeId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.PipelineJobs)
               .WithOne(j => j.Episode)
               .HasForeignKey(j => j.EpisodeId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
    }
}
```

---

## 6. Backend — .NET 9 API

### 6.1 `Program.cs` — Full Setup
```csharp
// API/Program.cs
using KidsCartoonPipeline.Infrastructure;
using KidsCartoonPipeline.API.Middleware;
using Serilog;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Kids Cartoon Pipeline API", Version = "v1" });
});

// Infrastructure services (DB, Repos, AI Services, Cache, Hangfire)
builder.Services.AddInfrastructure(builder.Configuration);

// CORS for React frontend
builder.Services.AddCors(opts => opts.AddPolicy("Frontend", p =>
    p.WithOrigins(
        builder.Configuration["Frontend:Url"] ?? "http://localhost:5173"
    ).AllowAnyHeader().AllowAnyMethod()
));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// Run EF migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedAsync(scope.ServiceProvider);  // seed default settings
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseSerilogRequestLogging();
app.UseCors("Frontend");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

app.Run();

public partial class Program { } // for WebApplicationFactory in tests
```

### 6.2 `DependencyInjection.cs`
```csharp
// Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        // MySQL + EF Core
        var connStr = config.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseMySql(connStr, ServerVersion.AutoDetect(connStr),
                x => x.MigrationsAssembly("KidsCartoonPipeline.Infrastructure")));

        // Repositories
        services.AddScoped<IEpisodeRepository, EpisodeRepository>();
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<ITopicRepository, TopicRepository>();
        services.AddScoped<IPipelineJobRepository, PipelineJobRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();

        // Settings Service (reads from DB, cached)
        services.AddScoped<ISettingsService, SettingsService>();

        // AI Services (typed HTTP clients)
        services.AddHttpClient<IScriptGenerationService, ClaudeScriptService>();
        services.AddHttpClient<IImageGenerationService, DalleImageService>();
        services.AddHttpClient<IVoiceGenerationService, ElevenLabsVoiceService>();
        services.AddHttpClient<IMusicGenerationService, SunoMusicService>();
        services.AddScoped<ISeoGenerationService, SeoGenerationService>();

        // Video & Storage
        services.AddScoped<IVideoAssemblyService, FfmpegVideoService>();
        services.AddScoped<IAssetStorageService, LocalAssetStorageService>();

        // YouTube
        services.AddScoped<IYouTubeService, YouTubeUploadService>();

        // Pipeline Orchestrator
        services.AddScoped<IPipelineOrchestrator, PipelineOrchestrator>();

        // In-Memory Cache
        services.AddMemoryCache();
        services.AddScoped<ICacheService, MemoryCacheService>();

        // Hangfire (MySQL storage)
        services.AddHangfire(c => c
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseStorage(new MySqlStorage(connStr, new MySqlStorageOptions
            {
                TablesPrefix = "Hangfire_"
            })));
        services.AddHangfireServer();

        return services;
    }
}
```

### 6.3 All Controllers

#### `EpisodesController.cs`
```csharp
[ApiController]
[Route("api/[controller]")]
public class EpisodesController : ControllerBase
{
    // Inject: IEpisodeRepository, IPipelineOrchestrator, ICacheService, ILogger

    // GET    /api/episodes                    → PagedResult<EpisodeResponse>
    //   Query params: status, page, pageSize, search
    // GET    /api/episodes/{id}               → EpisodeResponse (404 if not found)
    // GET    /api/episodes/{id}/preview-url   → { url: string } signed URL for video
    // POST   /api/episodes                    → 201 EpisodeResponse (creates from topicId)
    // PUT    /api/episodes/{id}/metadata      → 200 EpisodeResponse (update title/desc/tags)
    // POST   /api/episodes/{id}/approve       → 200 { message } triggers upload
    // POST   /api/episodes/{id}/reject        → 200 { message } sets Rejected status + notes
    // POST   /api/episodes/{id}/regenerate/{stage} → 202 { jobId } re-runs specific stage
    // DELETE /api/episodes/{id}               → 204 (soft delete, cleans up assets)
    // GET    /api/episodes/{id}/pipeline-jobs → List<PipelineJobResponse>
}
```

#### `CharactersController.cs`
```csharp
[ApiController]
[Route("api/[controller]")]
public class CharactersController : ControllerBase
{
    // GET    /api/characters           → List<CharacterResponse>
    // GET    /api/characters/{id}      → CharacterResponse
    // POST   /api/characters           → 201 CharacterResponse
    //   Body: { name, description, voiceId, voiceName, imagePromptStyle }
    // PUT    /api/characters/{id}      → 200 CharacterResponse
    // DELETE /api/characters/{id}      → 204
    // GET    /api/characters/voices    → List<{ voiceId, voiceName, preview_url }>
    //   Calls ElevenLabs API to list available voices (cached 1 hour)
    // POST   /api/characters/{id}/test-voice → 200 { audioUrl }
    //   Generates a short test audio clip with the character's assigned voice
}
```

#### `TopicsController.cs`
```csharp
[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    // GET    /api/topics               → List<TopicResponse> (unused first, then used)
    // POST   /api/topics               → 201 TopicResponse
    // PUT    /api/topics/{id}          → 200 TopicResponse
    // DELETE /api/topics/{id}          → 204
    // POST   /api/topics/generate-ideas → 200 List<string>
    //   Calls Claude to suggest 10 episode topic ideas based on existing characters
    // POST   /api/topics/{id}/trigger  → 202 { episodeId }
    //   Immediately starts pipeline for this topic
    // PUT    /api/topics/reorder       → 200 (update priority ordering)
}
```

#### `SettingsController.cs`
```csharp
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    // GET  /api/settings              → List<SettingResponse>
    //   Returns all settings grouped by Category.
    //   Secret values are masked: returns "••••••••" if IsSecret=true and Value is set
    // GET  /api/settings/{key}        → SettingResponse
    // PUT  /api/settings/{key}        → 200 SettingResponse
    //   Body: { value: string }. Saves to AppSettings table.
    // PUT  /api/settings/batch        → 200 List<SettingResponse>
    //   Body: [{ key, value }]. Saves multiple at once (used by Settings page Save All)
    // POST /api/settings/test/{service} → 200 { success: bool, message: string }
    //   Tests the API key for the named service (Anthropic, OpenAI, ElevenLabs, YouTube)
    //   by making a lightweight API call
    // GET  /api/settings/status       → 200 { configured: [service names], missing: [service names] }
    //   Returns which services are configured (have a non-empty API key)
}
```

#### `PipelineController.cs`
```csharp
[ApiController]
[Route("api/[controller]")]
public class PipelineController : ControllerBase
{
    // GET  /api/pipeline/status       → PipelineStatusResponse
    //   { activeJobs: int, queuedEpisodes: int, recentJobs: List<PipelineJobResponse> }
    // POST /api/pipeline/trigger      → 202 { message }
    //   Manually triggers the pipeline scheduler (processes next queued topic)
    // POST /api/pipeline/pause        → 200
    //   Pauses the Hangfire recurring job
    // POST /api/pipeline/resume       → 200
    //   Resumes the Hangfire recurring job
    // GET  /api/pipeline/logs/{episodeId} → List<PipelineJobResponse>
    //   Real-time log feed for an episode's pipeline jobs
    // GET  /api/pipeline/schedule     → { cronExpression, nextRun, isActive }
    // PUT  /api/pipeline/schedule     → 200 (update cron expression from UI)
}
```

#### `AnalyticsController.cs`
```csharp
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    // GET /api/analytics/summary      → { totalEpisodes, published, totalViews, totalRevenue }
    //   Fetches from YouTube Analytics API, cached 30 min
    // GET /api/analytics/episodes     → List<{ episodeId, youtubeId, title, views, watchTime, revenue }>
    // GET /api/analytics/sync         → 200 triggers re-sync from YouTube Analytics API
}
```

#### `AssetsController.cs`
```csharp
[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    // GET /api/assets/video/{episodeId}    → FileStreamResult (streams MP4)
    // GET /api/assets/thumbnail/{episodeId} → FileStreamResult (returns thumbnail PNG)
    // GET /api/assets/audio/{dialogueId}   → FileStreamResult (returns audio clip)
}
```

### 6.4 Exception Handling Middleware
```csharp
// API/Middleware/ExceptionHandlingMiddleware.cs
// Catches all unhandled exceptions and returns structured JSON:
// {
//   "type": "ExternalServiceException" | "NotFoundException" | "ValidationException" | "InternalServerError",
//   "title": "Human readable title",
//   "status": 400 | 404 | 422 | 500,
//   "detail": "error message",
//   "traceId": "..."
// }
// Log all 500s with Serilog at Error level with full stack trace
// Return 404 for NotFoundException
// Return 422 for FluentValidation.ValidationException
// Return 500 for all others
```

---

## 7. Pipeline Worker Services

### 7.1 Pipeline Orchestrator
```csharp
// Infrastructure/Services/Pipeline/PipelineOrchestrator.cs
// Implements IPipelineOrchestrator

public class PipelineOrchestrator : IPipelineOrchestrator
{
    // RunFullPipelineAsync(int episodeId) - orchestrates all stages in order
    // Each stage:
    //   1. Creates a PipelineJob record (Status=Running, StartedAt=now)
    //   2. Calls the relevant service
    //   3. Updates Episode status
    //   4. Updates PipelineJob (Status=Completed/Failed, CompletedAt=now)
    //   5. Appends to LogOutput as JSON array
    //   6. If any stage fails: sets Episode.Status=Failed, stores error, stops pipeline

    // Stage order:
    // 1. ScriptGeneration   → Episode.Status = GeneratingScript
    // 2. ImageGeneration    → Episode.Status = GeneratingImages  (parallel per scene)
    // 3. VoiceGeneration    → Episode.Status = GeneratingAudio   (parallel per dialogue line)
    // 4. MusicGeneration    → Episode.Status = GeneratingMusic
    // 5. VideoAssembly      → Episode.Status = RenderingVideo
    // 6. SeoGeneration      → Episode.Status = GeneratingSeo
    // 7. Set PendingReview  → Episode.Status = PendingReview (pipeline stops here for human)

    // RunStageAsync(int episodeId, PipelineStage stage) - runs a single stage (for regenerate)
    
    // Implementation note: Use Task.WhenAll for parallel image/audio generation
    // Implementation note: Retry each stage up to 3 times on failure before marking failed
}
```

### 7.2 Hangfire Jobs
```csharp
// Worker/Jobs/PipelineTriggerJob.cs
// Recurring job: every 30 minutes (configurable from Settings)
// Logic:
//   1. Check if pipeline is paused (read AppSetting "Pipeline:IsPaused")
//   2. Count active running episodes (Status NOT IN [PendingReview, Published, Failed, Rejected])
//   3. If activeCount < maxConcurrent (read AppSetting "Pipeline:MaxConcurrentEpisodes", default 1)
//   4. Pick next TopicSeed (IsUsed=false, ordered by Priority desc, then CreatedAt asc)
//   5. Create Episode record from topic, assign all active Characters
//   6. Mark TopicSeed as used
//   7. Enqueue Hangfire background job: BackgroundJob.Enqueue(() => orchestrator.RunFullPipelineAsync(episodeId))

// Worker/Jobs/AnalyticsSyncJob.cs
// Recurring job: every 6 hours
// Fetches YouTube Analytics for all published episodes and stores/updates in DB
```

---

## 8. Frontend — React + Bootstrap 5

### 8.1 `package.json`
```json
{
  "name": "kids-cartoon-pipeline-ui",
  "version": "1.0.0",
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "preview": "vite preview",
    "test": "vitest",
    "test:ui": "vitest --ui",
    "test:coverage": "vitest --coverage"
  },
  "dependencies": {
    "react": "^18.3.1",
    "react-dom": "^18.3.1",
    "react-router-dom": "^6.26.0",
    "axios": "^1.7.7",
    "bootstrap": "^5.3.3",
    "bootstrap-icons": "^1.11.3",
    "react-player": "^2.16.0",
    "react-toastify": "^10.0.5",
    "react-hook-form": "^7.53.0",
    "recharts": "^2.13.0"
  },
  "devDependencies": {
    "@vitejs/plugin-react": "^4.3.1",
    "vite": "^5.4.0",
    "vitest": "^2.1.0",
    "@vitest/ui": "^2.1.0",
    "@testing-library/react": "^16.0.0",
    "@testing-library/jest-dom": "^6.5.0",
    "@testing-library/user-event": "^14.5.0",
    "jsdom": "^25.0.0"
  }
}
```

### 8.2 `vite.config.js`
```js
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/setupTests.js',
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      }
    }
  }
})
```

### 8.3 `src/main.jsx`
```jsx
import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap-icons/font/bootstrap-icons.css'
import 'react-toastify/dist/ReactToastify.css'
import { ToastContainer } from 'react-toastify'
import App from './App'
import { AppProvider } from './context/AppContext'
import { SettingsProvider } from './context/SettingsContext'

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <BrowserRouter>
      <SettingsProvider>
        <AppProvider>
          <App />
          <ToastContainer position="top-right" autoClose={4000} />
        </AppProvider>
      </SettingsProvider>
    </BrowserRouter>
  </React.StrictMode>
)
```

### 8.4 `src/App.jsx`
```jsx
// Routes:
// /                  → <Dashboard />
// /episodes          → <Episodes />
// /episodes/:id      → <EpisodeDetail />
// /characters        → <Characters />
// /topics            → <Topics />
// /settings          → <Settings />
// /analytics         → <Analytics />
// All wrapped in <Layout> (Sidebar + TopBar)
```

### 8.5 Axios Instance
```js
// src/api/axiosInstance.js
import axios from 'axios'
import { toast } from 'react-toastify'

const api = axios.create({
  baseURL: '/api',
  timeout: 120000,  // 2 min for long AI operations
  headers: { 'Content-Type': 'application/json' }
})

// Response interceptor: show toast on error
api.interceptors.response.use(
  response => response,
  error => {
    const message = error.response?.data?.detail || error.message || 'Something went wrong'
    toast.error(message)
    return Promise.reject(error)
  }
)

export default api
```

### 8.6 API Modules (implement all)

```js
// src/api/episodesApi.js
export const episodesApi = {
  getAll: (params) => api.get('/episodes', { params }),            // { status, page, pageSize }
  getById: (id) => api.get(`/episodes/${id}`),
  updateMetadata: (id, data) => api.put(`/episodes/${id}/metadata`, data),
  approve: (id, data) => api.post(`/episodes/${id}/approve`, data),
  reject: (id, data) => api.post(`/episodes/${id}/reject`, data),
  regenerate: (id, stage) => api.post(`/episodes/${id}/regenerate/${stage}`),
  delete: (id) => api.delete(`/episodes/${id}`),
  getPipelineJobs: (id) => api.get(`/episodes/${id}/pipeline-jobs`),
}

// src/api/charactersApi.js
export const charactersApi = {
  getAll: () => api.get('/characters'),
  getById: (id) => api.get(`/characters/${id}`),
  create: (data) => api.post('/characters', data),
  update: (id, data) => api.put(`/characters/${id}`, data),
  delete: (id) => api.delete(`/characters/${id}`),
  getVoices: () => api.get('/characters/voices'),
  testVoice: (id) => api.post(`/characters/${id}/test-voice`),
}

// src/api/topicsApi.js
export const topicsApi = {
  getAll: () => api.get('/topics'),
  create: (data) => api.post('/topics', data),
  update: (id, data) => api.put(`/topics/${id}`, data),
  delete: (id) => api.delete(`/topics/${id}`),
  generateIdeas: () => api.post('/topics/generate-ideas'),
  trigger: (id) => api.post(`/topics/${id}/trigger`),
  reorder: (data) => api.put('/topics/reorder', data),
}

// src/api/settingsApi.js
export const settingsApi = {
  getAll: () => api.get('/settings'),
  update: (key, value) => api.put(`/settings/${key}`, { value }),
  updateBatch: (settings) => api.put('/settings/batch', settings),
  testConnection: (service) => api.post(`/settings/test/${service}`),
  getStatus: () => api.get('/settings/status'),
}

// src/api/pipelineApi.js
export const pipelineApi = {
  getStatus: () => api.get('/pipeline/status'),
  trigger: () => api.post('/pipeline/trigger'),
  pause: () => api.post('/pipeline/pause'),
  resume: () => api.post('/pipeline/resume'),
  getLogs: (episodeId) => api.get(`/pipeline/logs/${episodeId}`),
  getSchedule: () => api.get('/pipeline/schedule'),
  updateSchedule: (data) => api.put('/pipeline/schedule', data),
}
```

---

## 9. UI/UX Guidelines & Screen Specs

> **AGENT:** Implement every page and component below exactly as described. Use Bootstrap 5.3 utility classes throughout. Every page must be fully responsive (mobile-first). Use Bootstrap Icons for all icons.

### 9.1 Layout

#### Sidebar (`components/layout/Sidebar.jsx`)
- Fixed left sidebar, width 250px on desktop
- Collapses to icon-only (60px) on tablet, off-canvas on mobile
- Background: `bg-dark` (Bootstrap dark)
- App logo/name at top: "🎬 CartoonAI" in white bold text
- Navigation items (with Bootstrap Icons):
  - `bi-speedometer2` Dashboard
  - `bi-collection-play` Episodes
  - `bi-people` Characters
  - `bi-lightbulb` Topics
  - `bi-gear` Settings
  - `bi-bar-chart` Analytics
- Active item: `bg-primary` highlight
- Pipeline status widget at bottom of sidebar:
  - Shows "● Pipeline Active" (green dot) or "● Paused" (gray dot)
  - Shows count of queued topics
  - "Run Now" button (calls POST /api/pipeline/trigger)

#### TopBar (`components/layout/TopBar.jsx`)
- Full width header, height 60px, `bg-white border-bottom shadow-sm`
- Left: current page title (dynamic)
- Right: 
  - `bi-bell` notification icon (badge with count of episodes in PendingReview)
  - Quick "▶ Trigger Pipeline" button (primary, small)
  - Setup status: show orange warning badge "⚠ Setup Required" if any required API key is missing — clicking it navigates to Settings

### 9.2 Dashboard Page (`pages/Dashboard.jsx`)

**Purpose:** At-a-glance overview of pipeline health and recent activity.

**Layout:** 3 stat cards row + 2-column main content

**Stat Cards Row (top):**
- Card 1: "📥 Queued Topics" — count of unused TopicSeeds, `bg-info text-white`
- Card 2: "⏳ Pending Review" — count of episodes in PendingReview status, `bg-warning text-dark` — clicking navigates to filtered Episodes list
- Card 3: "✅ Published" — count of Published episodes, `bg-success text-white`
- Card 4: "❌ Failed" — count of Failed episodes, `bg-danger text-white`

**Main Content (2 columns):**
- Left (col-8): "Recent Episodes" list — last 10 episodes with status badges, thumbnail if available, pipeline progress bar if currently processing
- Right (col-4): "Pipeline Activity" — live-updating list of recent PipelineJobs (auto-refreshes every 10 seconds with polling), shows stage + status + time elapsed

**Setup Banner:**
- If any required API keys are missing, show a prominent `alert-warning` banner at top:
  "⚠️ Setup required: Missing API keys for [service names]. Go to Settings to configure."
  With a "Configure Now →" button linking to /settings

**UX Notes:**
- Auto-refresh the dashboard stats every 30 seconds
- Show a pulsing animation on the stat card while a pipeline is actively running
- Clicking any stat card filters the Episodes page by that status

### 9.3 Episodes Page (`pages/Episodes.jsx`)

**Purpose:** Browse all episodes, filter by status, take bulk actions.

**Layout:**
- Top: filter bar (status dropdown, search box, "Create Episode" button)
- Main: responsive grid of EpisodeCards (3 columns desktop, 2 tablet, 1 mobile)

**EpisodeCard Component (`components/episodes/EpisodeCard.jsx`):**
- Shows: thumbnail (or placeholder image if not yet generated), episode title, status badge, creation date
- Status badge colors:
  - TopicQueued → `badge bg-secondary`
  - Generating* → `badge bg-primary` with spinner animation
  - PendingReview → `badge bg-warning text-dark` (most prominent)
  - Approved → `badge bg-info`
  - Published → `badge bg-success`
  - Failed → `badge bg-danger`
  - Rejected → `badge bg-dark`
- Pipeline progress bar: thin colored bar at bottom of card showing stage completion (0–7 stages)
- Action buttons (visible on hover):
  - `bi-eye` View Details
  - `bi-check-circle` Approve (only if PendingReview)
  - `bi-x-circle` Reject (only if PendingReview)
  - `bi-arrow-repeat` Retry (only if Failed)
  - `bi-trash` Delete

**Filter Bar:**
- Status filter: `<select>` All | Queued | Processing | Pending Review | Published | Failed
- Search: searches title
- Sort: Newest | Oldest | Status

### 9.4 Episode Detail Page (`pages/EpisodeDetail.jsx`)

**Purpose:** Full view of one episode — review, edit metadata, approve/reject.

**Layout:** Two-column: left (col-7) video + pipeline logs, right (col-5) metadata panel

**Left Column:**
- Video preview player (react-player, shows MP4 if rendered, otherwise shows placeholder with current stage progress)
- Pipeline Stage Timeline: vertical stepper showing all 7 stages
  - Completed stages: green checkmark `bi-check-circle-fill text-success`
  - Current stage: blue spinner `spinner-border spinner-border-sm text-primary`
  - Failed stage: red X `bi-x-circle-fill text-danger`
  - Pending stages: gray circle `bi-circle text-muted`
  - Each stage shows: stage name, duration (when completed), "Regenerate" button
- Pipeline Logs accordion: expandable log output per stage (raw text in `<pre>` block)

**Right Column (metadata panel):**
- Episode title (editable `<input>`)
- SEO Title (editable, character count max 100)
- SEO Description (editable `<textarea>`, character count max 5000)
- Tags (editable tag input — comma-separated chips)
- Scheduled Publish At (datetime picker)
- Characters used (read-only list with avatars)
- Original topic (read-only)
- **Action buttons (full width, prominent):**
  - "✅ Approve & Schedule Upload" — `btn-success btn-lg` — shown only if PendingReview
  - "✏️ Save Metadata" — `btn-primary`
  - "❌ Reject" — `btn-outline-danger` — opens a modal asking for rejection reason
  - "🔁 Regenerate Entire Episode" — `btn-outline-warning`

**UX Notes:**
- Auto-poll pipeline logs every 5 seconds when episode is in a "Generating" status
- Show "Last updated X seconds ago" below the pipeline timeline
- When user clicks Approve, show a confirmation modal: "This will upload to YouTube. Schedule time: [datetime]. Confirm?"
- Unsaved metadata changes show a yellow "Unsaved changes" banner at top of metadata panel

### 9.5 Characters Page (`pages/Characters.jsx`)

**Purpose:** Manage the recurring cast of characters used in episodes.

**Layout:**
- Top: "Add Character" button (opens modal form)
- Grid of CharacterCards (4 per row desktop)

**CharacterCard Component (`components/characters/CharacterCard.jsx`):**
- Shows: character avatar image (or silhouette placeholder), name, voice name
- Shows episode count ("Used in 12 episodes")
- Edit and Delete buttons
- "▶ Test Voice" button: calls POST /api/characters/{id}/test-voice and plays the returned audio clip in-browser

**CharacterForm Component (in Modal):**
- Fields:
  - Name (required, text input)
  - Description (required, textarea — "Describe appearance for image generation. Be very specific. Example: A small round orange fox cub with big green eyes, wearing a blue striped scarf, 2D flat cartoon style")
  - Image Prompt Style (textarea — "Additional art style keywords to lock visual consistency. Example: soft shadows, vibrant colors, Pixar-inspired, child-safe")
  - Voice Assignment: dropdown populated from GET /api/characters/voices. Shows voice name + "Preview" button per option
  - Avatar URL (optional, text input — paste URL to a reference image)
- Validation: Name and Description are required
- "Test Voice" after selecting: plays a 5-second demo

**UX Notes:**
- Include helper text below the Description field: "💡 Tip: The more specific this description, the more consistent your character will look across episodes. Include colors, clothing, facial features."
- Include helper text below Image Prompt Style: "💡 Tip: These keywords are appended to every image generation prompt for this character."

### 9.6 Topics Page (`pages/Topics.jsx`)

**Purpose:** Manage the queue of episode ideas.

**Layout:**
- Top: "Add Topic" button + "🤖 Generate 10 AI Ideas" button
- Sortable list (drag-to-reorder to set priority)
- Two sections: "⏳ Queued" and "✅ Used"

**TopicCard Component:**
- Shows: title, moral/lesson if set, priority badge, "Produce Now" button
- "Produce Now" calls POST /api/topics/{id}/trigger immediately

**TopicForm (in Modal):**
- Fields: Title (required), Description, Target Moral/Lesson
- Helper text: "💡 Tip: Good topic titles include a character name and an action. Example: 'Luna the Bunny learns to share her carrots'"

**AI Generate Ideas button behavior:**
- Shows loading spinner
- Opens a modal showing 10 AI-generated topic suggestions
- Each suggestion has a "Add to Queue" button
- Suggestions are generated by Claude based on existing characters and their descriptions

**UX Notes:**
- Drag-and-drop reordering of queued topics (use HTML5 drag events or react-beautiful-dnd)
- Show "Next to be produced →" label on the highest-priority unused topic

### 9.7 Settings Page (`pages/Settings.jsx`)

**Purpose:** Central control for ALL API keys and pipeline behavior. After entering keys here, the pipeline works automatically. Zero `.env` editing needed.

**⚠️ CRITICAL REQUIREMENT:** This page is the most important page for first-time setup. It must be clear, well-organized, and forgiving of errors.

**Layout:** Tab-based sections (Bootstrap Tabs)

#### Tab 1: "🔑 API Keys"

Show a setup checklist at the top:
```
Setup Progress: [3/5 services configured] ████████░░ 60%

✅ Claude AI (Anthropic)         [Connected]
✅ DALL-E 3 (OpenAI)             [Connected]
✅ ElevenLabs (Voice)            [Connected]
❌ Suno (Music)                  [Not configured]
❌ YouTube                       [Not connected]
```

For each service, show a card with:
- Service name + logo/icon
- Link to where to get an API key (external link)
- Password input field for the API key
- "Test Connection" button → calls POST /api/settings/test/{service}
  - On success: green badge "✅ Connected"
  - On failure: red badge "❌ Failed: [error message]"
- Short description of what the service is used for

Services and their fields:
```
Anthropic (Claude AI)
  Key: Anthropic:ApiKey
  Get key at: https://console.anthropic.com
  Used for: Script generation, SEO metadata, topic ideas

OpenAI (DALL-E 3)
  Key: OpenAI:ApiKey
  Get key at: https://platform.openai.com/api-keys
  Used for: Scene image and thumbnail generation

ElevenLabs (Voice)
  Key: ElevenLabs:ApiKey
  Get key at: https://elevenlabs.io/app/settings/api-keys
  Used for: Character voice synthesis (TTS)

Suno (Music)
  Key: Suno:ApiKey
  Get key at: https://suno.ai
  Used for: Background music generation

YouTube
  Key: YouTube:RefreshToken (+ YouTube:ClientId + YouTube:ClientSecret)
  Special: Show "Connect YouTube Account" OAuth button (not just a text field)
  Used for: Auto-uploading videos to your channel
```

#### Tab 2: "⚙️ Pipeline Settings"

```
Auto-run pipeline:          [Toggle ON/OFF]
Pipeline schedule:          [Cron input: "0 */6 * * *"] + human-readable preview
Max concurrent episodes:    [Number input: 1] (recommended: 1 to avoid API rate limits)
Episodes per week target:   [Number input: 4]
Default publish time:       [Time input: 09:00] (local time)
Publish days:               [Checkboxes: Mon Tue Wed Thu Fri Sat Sun] (Sat+Sun checked by default)
```

#### Tab 3: "🎨 Image Style Settings"

```
Global art style:           [Textarea]
  Default: "2D flat cartoon, bright saturated colors, Pixar-inspired, child-friendly, soft shadows, no text, no watermarks, 16:9 aspect ratio"
  Helper: "This is appended to every image generation prompt. Change this to change the visual style of your entire channel."

Image quality:              [Select: standard | hd]
  Default: hd
  Note: "hd costs 2x more but produces significantly better results"

Background image size:      [Select: 1792x1024 | 1024x1024]
  Default: 1792x1024

Generate motion (RunwayML): [Toggle OFF]
  Note: "Adds subtle motion to static images using RunwayML API (requires additional API key). Off by default."
```

#### Tab 4: "✍️ Prompt Templates"

Editable text areas for all Claude prompts:
```
Script Generation Prompt:   [Large textarea - shows default prompt, fully editable]
SEO Title Prompt:           [Textarea]
SEO Description Prompt:     [Textarea]
SEO Tags Prompt:            [Textarea]
Topic Ideas Prompt:         [Textarea]
Image Prompt Builder:       [Textarea - the meta-prompt that converts scene descriptions to image prompts]
```
Each field has a "Reset to Default" button.

#### Tab 5: "📺 YouTube Settings"

```
Channel Name:               [Text input - for display only]
Default category:           [Select - Education (27) is pre-selected]
Default privacy:            [Select: Public | Unlisted | Private] (default: Public)
Made for kids:              [Toggle ON] (required for kids content, ON by default)
Auto-publish:               [Toggle ON] (if OFF, uploads as Private and notifies you)
Default video description suffix: [Textarea - appended to all video descriptions]
  Default: "\n\n#KidsCartoon #AnimatedCartoon #ChildrenEducation #KidsTV"
```

**Save Behavior:**
- Individual field saves: each field has its own "Save" icon button (checkmark)
- "Save All" button at bottom of each tab saves all fields in that tab at once
- Show a green "✅ Saved" toast notification on successful save
- Unsaved changes indicator: tab title shows "⚠" if there are unsaved changes

### 9.8 Analytics Page (`pages/Analytics.jsx`)

**Purpose:** View channel performance without leaving the app.

**Layout:**
- Top row: 4 stat cards — Total Views, Total Watch Hours, Est. Revenue (USD), Avg Views/Episode
- Main: Table of all published episodes with columns: Thumbnail | Title | Views | Watch Hours | Revenue | Published Date | YouTube Link
- Chart: Bar chart of views per episode over time (recharts)

**UX Notes:**
- "Sync from YouTube" button top right — calls GET /api/analytics/sync
- Show "Last synced X minutes ago" timestamp
- If YouTube not connected, show empty state with link to Settings

### 9.9 Global UX Rules

1. **Loading states:** Every async operation shows a `spinner-border` in the button and disables it while loading
2. **Empty states:** Every list page has an `<EmptyState>` component when the list is empty. Include an icon, a title, a description, and a primary CTA button. Examples:
   - Episodes empty: "No episodes yet. Add some topics and trigger the pipeline!"
   - Characters empty: "Add your first character to get started"
   - Topics empty: "Add topic ideas or let AI generate some for you"
3. **Error states:** Every data fetch has an error state showing `<ErrorAlert>` with a "Retry" button
4. **Confirmation modals:** All destructive actions (Delete, Reject) open a `<ConfirmModal>` before executing
5. **Responsive:** All pages work on mobile. Sidebar collapses to hamburger menu on mobile.
6. **Color theme:** Default Bootstrap theme with `data-bs-theme="light"`. Primary color is Bootstrap's default blue.
7. **Navigation feedback:** Active nav item in sidebar is highlighted. Page title in TopBar matches current route.
8. **Toasts:** Success = green. Error = red. Info = blue. Always auto-dismiss in 4 seconds.
9. **Form validation:** Use react-hook-form. Show inline validation errors in red below fields. Disable Submit button if form is invalid.
10. **API key masking:** Password inputs for API keys. Show/hide toggle (eye icon).

---

## 10. Settings & Configuration System

### How Settings Work (important — read carefully)

All configuration is stored in the `AppSettings` MySQL table. On app first run, the database is seeded with all setting definitions (key, displayName, description, isSecret, isRequired, defaultValue). The UI reads settings from `GET /api/settings` and writes them back with `PUT /api/settings/{key}`.

### SettingsService
```csharp
// Infrastructure/Services/Settings/SettingsService.cs
public class SettingsService : ISettingsService
{
    // Uses IMemoryCache to cache settings with 5-minute TTL
    // GetAsync(string key) → returns Value or null
    // GetApiKeyAsync(string service) → returns the API key for a service, throws if missing
    // SetAsync(string key, string value) → saves to DB, invalidates cache
    // GetAllByCategory(string category) → returns grouped settings
    // GetConfigurationStatus() → { configured: [], missing: [] }
    
    // Cache key pattern: "settings:{key}"
    // On any Set(), invalidate "settings:{key}" and "settings:all"
}
```

### Seed Data for Settings
```csharp
// Infrastructure/Data/SeedData.cs
// On startup, upsert all these settings (insert if not exists, never overwrite existing value)
public static readonly List<AppSetting> DefaultSettings = new()
{
    // API Keys
    new() { Key="Anthropic:ApiKey", Category="ApiKeys", DisplayName="Anthropic API Key", IsSecret=true, IsRequired=true, Description="Claude AI for scripts and SEO" },
    new() { Key="OpenAI:ApiKey", Category="ApiKeys", DisplayName="OpenAI API Key", IsSecret=true, IsRequired=true, Description="DALL-E 3 for image generation" },
    new() { Key="ElevenLabs:ApiKey", Category="ApiKeys", DisplayName="ElevenLabs API Key", IsSecret=true, IsRequired=true, Description="Text-to-speech for character voices" },
    new() { Key="Suno:ApiKey", Category="ApiKeys", DisplayName="Suno API Key", IsSecret=true, IsRequired=false, Description="Background music generation" },
    new() { Key="YouTube:ClientId", Category="ApiKeys", DisplayName="YouTube Client ID", IsSecret=false, IsRequired=true, Description="Google OAuth Client ID" },
    new() { Key="YouTube:ClientSecret", Category="ApiKeys", DisplayName="YouTube Client Secret", IsSecret=true, IsRequired=true, Description="Google OAuth Client Secret" },
    new() { Key="YouTube:RefreshToken", Category="ApiKeys", DisplayName="YouTube Refresh Token", IsSecret=true, IsRequired=true, Description="OAuth refresh token for your channel" },
    
    // Pipeline settings
    new() { Key="Pipeline:IsActive", Category="Pipeline", DisplayName="Auto-run Pipeline", Value="true", IsSecret=false },
    new() { Key="Pipeline:CronSchedule", Category="Pipeline", DisplayName="Schedule (Cron)", Value="0 */6 * * *", IsSecret=false },
    new() { Key="Pipeline:MaxConcurrent", Category="Pipeline", DisplayName="Max Concurrent Episodes", Value="1", IsSecret=false },
    new() { Key="Pipeline:DefaultPublishTime", Category="Pipeline", DisplayName="Default Publish Time", Value="09:00", IsSecret=false },
    new() { Key="Pipeline:PublishDays", Category="Pipeline", DisplayName="Publish Days", Value="Saturday,Sunday", IsSecret=false },
    
    // Image settings
    new() { Key="Images:GlobalStyle", Category="Images", DisplayName="Global Art Style", Value="2D flat cartoon, bright saturated colors, Pixar-inspired, child-friendly, soft shadows, no text, no watermarks, 16:9 aspect ratio", IsSecret=false },
    new() { Key="Images:Quality", Category="Images", DisplayName="Image Quality", Value="hd", IsSecret=false },
    new() { Key="Images:Size", Category="Images", DisplayName="Image Size", Value="1792x1024", IsSecret=false },
    
    // Prompt templates
    new() { Key="Prompts:Script", Category="Prompts", DisplayName="Script Generation Prompt", Value="[DEFAULT_SCRIPT_PROMPT]", IsSecret=false },
    new() { Key="Prompts:SeoTitle", Category="Prompts", DisplayName="SEO Title Prompt", Value="[DEFAULT_SEO_TITLE_PROMPT]", IsSecret=false },
    new() { Key="Prompts:SeoDescription", Category="Prompts", DisplayName="SEO Description Prompt", Value="[DEFAULT_SEO_DESCRIPTION_PROMPT]", IsSecret=false },
    new() { Key="Prompts:ImageBuilder", Category="Prompts", DisplayName="Image Prompt Builder", Value="[DEFAULT_IMAGE_PROMPT]", IsSecret=false },
    
    // YouTube settings
    new() { Key="YouTube:CategoryId", Category="YouTube", DisplayName="Video Category", Value="27", IsSecret=false },
    new() { Key="YouTube:Privacy", Category="YouTube", DisplayName="Default Privacy", Value="public", IsSecret=false },
    new() { Key="YouTube:MadeForKids", Category="YouTube", DisplayName="Made For Kids", Value="true", IsSecret=false },
    new() { Key="YouTube:AutoPublish", Category="YouTube", DisplayName="Auto-Publish", Value="true", IsSecret=false },
    new() { Key="YouTube:DescriptionSuffix", Category="YouTube", DisplayName="Description Suffix", Value="\n\n#KidsCartoon #AnimatedCartoon #ChildrensEducation #KidsTV", IsSecret=false },
};
```

---

## 11. AI Service Integrations

### 11.1 Claude Script Service
```csharp
// Infrastructure/Services/AI/ClaudeScriptService.cs
// Endpoint: POST https://api.anthropic.com/v1/messages
// Headers: x-api-key: {ApiKey}, anthropic-version: 2023-06-01
// Model: claude-sonnet-4-5
// Reads prompt template from: AppSettings["Prompts:Script"]

// Request body:
{
  "model": "claude-sonnet-4-5",
  "max_tokens": 4000,
  "system": "You are a children's cartoon scriptwriter. Output ONLY valid JSON with no markdown, no code fences, no preamble.",
  "messages": [{ "role": "user", "content": "{prompt}" }]
}

// Prompt template (stored in AppSettings, editable from UI):
// "Write a 3-minute animated episode for kids aged 3-7.
// Topic: {topic}
// Characters: {characters_json}
// Target moral: {moral}
// Return ONLY this JSON structure (no other text):
// {
//   \"title\": \"Episode title\",
//   \"summary\": \"2-sentence parent-friendly summary\",
//   \"moral\": \"One sentence lesson learned\",
//   \"scenes\": [
//     {
//       \"scene_number\": 1,
//       \"duration_seconds\": 20,
//       \"background\": \"Detailed visual scene description for image generation\",
//       \"action\": \"What physically happens in this scene\",
//       \"characters_present\": [\"CharacterName\"],
//       \"character_emotions\": {\"CharacterName\": \"happy\"},
//       \"dialogue\": [
//         { \"character\": \"CharacterName\", \"line\": \"Spoken text\", \"tone\": \"excited\" }
//       ]
//     }
//   ]
// }"
```

### 11.2 DALL-E 3 Image Service
```csharp
// Infrastructure/Services/AI/DalleImageService.cs
// Endpoint: POST https://api.openai.com/v1/images/generations
// Headers: Authorization: Bearer {ApiKey}

// For each scene, first call Claude to convert scene description to an optimized image prompt:
// "Convert this scene description to an image generation prompt.
// Scene: {scene.background} — {scene.action}
// Characters present: {characters with their descriptions}
// Prepend with global art style: {AppSettings["Images:GlobalStyle"]}
// Output ONLY the prompt text, nothing else."

// Then call DALL-E 3:
{
  "model": "dall-e-3",
  "prompt": "{generated_image_prompt}",
  "n": 1,
  "size": "{AppSettings[Images:Size]}",
  "quality": "{AppSettings[Images:Quality]}",
  "response_format": "b64_json"
}

// Save image to: storage/images/ep{episodeId}/scene{sceneNumber}.png
// Update Scene.ImagePath and Scene.ImagePromptUsed in DB
```

### 11.3 ElevenLabs Voice Service
```csharp
// Infrastructure/Services/AI/ElevenLabsVoiceService.cs
// Endpoint: POST https://api.elevenlabs.io/v1/text-to-speech/{voice_id}
// Headers: xi-api-key: {ApiKey}

// For each DialogueLine:
{
  "text": "{dialogueLine.Text}",
  "model_id": "eleven_turbo_v2",
  "voice_settings": {
    "stability": 0.75,
    "similarity_boost": 0.85,
    "style": 0.3,
    "use_speaker_boost": true
  }
}

// voice_id comes from: Characters table (character.VoiceId)
// If character has no VoiceId, use AppSettings["ElevenLabs:DefaultVoiceId"]
// Save audio to: storage/audio/ep{episodeId}/scene{sceneId}_line{lineOrder}.mp3
// Update DialogueLine.AudioPath in DB

// List voices (for Settings UI):
// GET https://api.elevenlabs.io/v1/voices
// Returns list of {voice_id, name, preview_url, category}
```

### 11.4 Suno Music Service
```csharp
// Infrastructure/Services/AI/SunoMusicService.cs
// Note: Suno's API is unofficial/v3. Use: POST https://studio-api.suno.ai/api/generate/v2/
// If Suno API key is not configured, fall back to a royalty-free music from Freesound API
// Alternatively use: Mubert API (https://api.mubert.com/v2/RecordTrack) as fallback

// Music prompt (generated by Claude from episode summary):
// "Children's cartoon background music. {moodKeywords}. 
// Upbeat, playful, warm orchestral, ukulele, xylophone. 
// No lyrics. Loops cleanly. Safe for kids aged 3-7. Duration: 3 minutes."

// Save music to: storage/music/ep{episodeId}/background.mp3
```

### 11.5 SEO Generation Service
```csharp
// Infrastructure/Services/AI/SeoGenerationService.cs
// Uses ClaudeScriptService's HTTP client (same Anthropic endpoint)
// Reads prompt templates from AppSettings

// Generates in one API call:
// {
//   "seo_title": "...",  // max 100 chars, includes character name + topic + "Kids Cartoon"
//   "seo_description": "...",  // 200-500 words, parent-friendly, keyword-rich
//   "tags": ["tag1", ...],  // 15-20 tags
//   "thumbnail_prompt": "..."  // image prompt for DALL-E thumbnail
// }

// Also generates thumbnail image via DalleImageService
// Save thumbnail to: storage/thumbnails/ep{episodeId}/thumbnail.png
```

### 11.6 FFmpeg Video Service
```csharp
// Infrastructure/Services/Video/FfmpegVideoService.cs

// STEP 1: For each scene, create a video clip:
// ffmpeg -loop 1 -i {scene.ImagePath} -i {combinedSceneAudio} 
//   -vf "zoompan=z='min(zoom+0.0008,1.2)':x='iw/2-(iw/zoom/2)':y='ih/2-(ih/zoom/2)':d=125:s=1920x1080,
//        subtitles={subtitleFile}:force_style='FontSize=24,PrimaryColour=&HFFFFFF,OutlineColour=&H000000,Outline=2'"
//   -c:v libx264 -preset fast -crf 20 -c:a aac -shortest
//   storage/videos/ep{episodeId}/scene{n}.mp4

// STEP 2: Combine scene dialogue audio files with timing:
// ffmpeg -i line1.mp3 -i line2.mp3 ... -filter_complex "concat=n={count}:v=0:a=1" scene_dialogue.mp3

// STEP 3: Mix dialogue + music:
// ffmpeg -i scene_dialogue.mp3 -i background.mp3
//   -filter_complex "[0:a]volume=1.0[dialogue];[1:a]volume=0.15[music];[dialogue][music]amix=inputs=2:duration=longest"
//   scene_audio_final.mp3

// STEP 4: Concatenate all scene clips:
// Write concat list file, then:
// ffmpeg -f concat -safe 0 -i scene_list.txt -c copy storage/videos/ep{episodeId}/episode_raw.mp4

// STEP 5: Add intro/outro bumpers (if configured in settings):
// ffmpeg -f concat -safe 0 -i full_list.txt -c copy storage/videos/ep{episodeId}/final.mp4

// Generate subtitle file (.srt format) from dialogue lines with timing
// Each line starts at cumulative audio offset time
// Update Episode.VideoPath in DB
```

---

## 12. YouTube Integration

```csharp
// Infrastructure/Services/YouTube/YouTubeUploadService.cs
// NuGet: Google.Apis.YouTube.v3, Google.Apis.Auth

// OAuth setup:
// ClientId and ClientSecret from AppSettings["YouTube:ClientId"] and ["YouTube:ClientSecret"]
// RefreshToken from AppSettings["YouTube:RefreshToken"]
// Use GoogleWebAuthorizationBroker or UserCredential with stored refresh token

// Upload video:
public async Task<string> UploadVideoAsync(Episode episode)
{
    var credential = await GetCredentialAsync();
    var youtubeService = new YouTubeService(new BaseClientService.Initializer
    {
        HttpClientInitializer = credential,
        ApplicationName = "KidsCartoonPipeline"
    });

    var video = new Video
    {
        Snippet = new VideoSnippet
        {
            Title = episode.SeoTitle ?? episode.Title,
            Description = episode.SeoDescription + await GetDescriptionSuffix(),
            Tags = JsonSerializer.Deserialize<List<string>>(episode.SeoTags ?? "[]"),
            CategoryId = await _settingsService.GetAsync("YouTube:CategoryId") ?? "27",
            DefaultLanguage = "en",
            DefaultAudioLanguage = "en"
        },
        Status = new VideoStatus
        {
            PrivacyStatus = "private",         // stays private until publish time
            PublishAt = episode.ScheduledPublishAt,
            MadeForKids = bool.Parse(await _settingsService.GetAsync("YouTube:MadeForKids") ?? "true"),
            SelfDeclaredMadeForKids = true
        }
    };

    // Upload video file
    using var videoStream = File.OpenRead(episode.VideoPath!);
    var insertRequest = youtubeService.Videos.Insert(video, "snippet,status", videoStream, "video/*");
    insertRequest.ProgressChanged += OnUploadProgressChanged;
    await insertRequest.UploadAsync();
    var videoId = insertRequest.ResponseBody.Id;

    // Upload thumbnail
    using var thumbStream = File.OpenRead(episode.ThumbnailPath!);
    await youtubeService.Thumbnails.Set(videoId, thumbStream, "image/png").UploadAsync();

    // Save videoId and URL to DB
    episode.YouTubeVideoId = videoId;
    episode.YouTubeUrl = $"https://youtu.be/{videoId}";
    episode.Status = EpisodeStatus.Scheduled;

    return videoId;
}

// YouTube OAuth callback endpoint:
// GET /api/settings/youtube/auth-url  → returns OAuth authorization URL
// GET /api/settings/youtube/callback?code=...  → exchanges code for tokens, saves to DB
```

---

## 13. In-Memory Cache Strategy

```csharp
// Infrastructure/Services/Cache/MemoryCacheService.cs
// Wraps IMemoryCache with typed methods and explicit TTL management

// Cache entries and TTLs:
// "settings:all"                → 5 minutes (all AppSettings)
// "settings:{key}"              → 5 minutes (single setting)
// "characters:all"              → 10 minutes (character list)
// "elevenlabs:voices"           → 60 minutes (ElevenLabs voice list)
// "analytics:summary"           → 30 minutes (YouTube analytics summary)
// "pipeline:status"             → 30 seconds (active job counts)
// "topics:queued:count"         → 1 minute (count of queued topics)

// Invalidation:
// On any setting save → invalidate "settings:all" and "settings:{key}"
// On character create/update/delete → invalidate "characters:all"
// On episode status change → invalidate "pipeline:status"

// ICacheService interface:
public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? ttl = null);
    void Remove(string key);
    void RemoveByPrefix(string prefix);  // removes all keys starting with prefix
    bool TryGet<T>(string key, out T? value);
}
```

---

## 14. Docker & Environment Setup

### `docker-compose.yml`
```yaml
version: '3.9'

services:
  mysql:
    image: mysql:8.0
    container_name: kcp_mysql
    restart: unless-stopped
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD:-rootpassword}
      MYSQL_DATABASE: KidsCartoonPipeline
      MYSQL_USER: ${MYSQL_USER:-kcp_user}
      MYSQL_PASSWORD: ${MYSQL_PASSWORD:-kcp_password}
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      interval: 10s
      timeout: 5s
      retries: 5

  backend:
    build:
      context: ./backend
      dockerfile: KidsCartoonPipeline.API/Dockerfile
    container_name: kcp_backend
    restart: unless-stopped
    depends_on:
      mysql:
        condition: service_healthy
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:5000
      ConnectionStrings__DefaultConnection: "Server=mysql;Port=3306;Database=KidsCartoonPipeline;User=kcp_user;Password=kcp_password;"
      Frontend__Url: http://localhost:3000
    ports:
      - "5000:5000"
    volumes:
      - ./storage:/app/storage
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    container_name: kcp_frontend
    restart: unless-stopped
    depends_on:
      - backend
    ports:
      - "3000:80"
    environment:
      VITE_API_URL: http://localhost:5000

  worker:
    build:
      context: ./backend
      dockerfile: KidsCartoonPipeline.Worker/Dockerfile
    container_name: kcp_worker
    restart: unless-stopped
    depends_on:
      mysql:
        condition: service_healthy
      backend:
        condition: service_healthy
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "Server=mysql;Port=3306;Database=KidsCartoonPipeline;User=kcp_user;Password=kcp_password;"
    volumes:
      - ./storage:/app/storage

volumes:
  mysql_data:
```

### Backend `Dockerfile`
```dockerfile
# backend/KidsCartoonPipeline.API/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
RUN apt-get update && apt-get install -y ffmpeg curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["KidsCartoonPipeline.API/KidsCartoonPipeline.API.csproj", "KidsCartoonPipeline.API/"]
COPY ["KidsCartoonPipeline.Core/KidsCartoonPipeline.Core.csproj", "KidsCartoonPipeline.Core/"]
COPY ["KidsCartoonPipeline.Infrastructure/KidsCartoonPipeline.Infrastructure.csproj", "KidsCartoonPipeline.Infrastructure/"]
RUN dotnet restore "KidsCartoonPipeline.API/KidsCartoonPipeline.API.csproj"
COPY . .
RUN dotnet build "KidsCartoonPipeline.API/KidsCartoonPipeline.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KidsCartoonPipeline.API/KidsCartoonPipeline.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/storage/images /app/storage/audio /app/storage/music /app/storage/videos /app/storage/thumbnails
ENTRYPOINT ["dotnet", "KidsCartoonPipeline.API.dll"]
```

### Frontend `Dockerfile`
```dockerfile
# frontend/Dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### Frontend `nginx.conf`
```nginx
server {
    listen 80;
    root /usr/share/nginx/html;
    index index.html;
    
    location / {
        try_files $uri $uri/ /index.html;
    }
    
    location /api/ {
        proxy_pass http://backend:5000/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        client_max_body_size 500M;
        proxy_read_timeout 300s;
    }
}
```

### `.env.example`
```env
MYSQL_ROOT_PASSWORD=rootpassword
MYSQL_USER=kcp_user
MYSQL_PASSWORD=kcp_password
```
> **Note:** No API keys in `.env`. All API keys are stored in the database and managed via the Settings page in the UI.

### `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=KidsCartoonPipeline;User=kcp_user;Password=kcp_password;"
  },
  "Frontend": {
    "Url": "http://localhost:5173"
  },
  "Storage": {
    "BasePath": "./storage"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

---

## 15. Seed Data & Initial Setup

```csharp
// Infrastructure/Data/SeedData.cs
public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        
        // 1. Seed AppSettings (all defaults above — never overwrite existing values)
        await SeedSettingsAsync(db);
        
        // 2. Seed sample Characters (only if Characters table is empty)
        if (!await db.Characters.AnyAsync())
        {
            db.Characters.AddRange(new[]
            {
                new Character
                {
                    Name = "Leo",
                    Description = "A small friendly orange lion cub with a fluffy mane, big brown eyes, and a warm smile. Wears a red bandana.",
                    ImagePromptStyle = "soft rounded features, warm lighting, bright and cheerful expression",
                    VoiceName = "Choose in Settings",
                    IsActive = true
                },
                new Character
                {
                    Name = "Luna",
                    Description = "A small curious purple bunny with long floppy ears, big blue eyes, and a tiny pink nose. Wears a yellow sunflower hairclip.",
                    ImagePromptStyle = "soft pastel tones, gentle expression, whimsical feel",
                    VoiceName = "Choose in Settings",
                    IsActive = true
                }
            });
        }

        // 3. Seed sample Topics (only if Topics table is empty)
        if (!await db.TopicSeeds.AnyAsync())
        {
            db.TopicSeeds.AddRange(new[]
            {
                new TopicSeed { Title = "Leo learns to count to 10", TargetMoral = "Learning is fun", Priority = 10 },
                new TopicSeed { Title = "Luna shares her carrots with friends", TargetMoral = "Sharing is caring", Priority = 9 },
                new TopicSeed { Title = "Leo and Luna discover the colors of the rainbow", TargetMoral = "The world is beautiful", Priority = 8 },
                new TopicSeed { Title = "Luna learns why we wash our hands", TargetMoral = "Staying clean keeps us healthy", Priority = 7 },
                new TopicSeed { Title = "Leo helps a lost bird find its way home", TargetMoral = "Kindness matters", Priority = 6 },
            });
        }

        await db.SaveChangesAsync();
    }
}
```

---

## 16. Full Implementation Order

> **AGENT: Follow this order exactly. Complete each step fully (including tests passing) before moving to the next.**

### Phase 1: Foundation (Backend)
- [ ] **Step 1:** Create solution and all project files. Add all NuGet packages. Verify `dotnet build` passes.
- [ ] **Step 2:** Write all Core entities, enums, DTOs, and interface definitions (no implementations yet).
- [ ] **Step 3:** Write EF DbContext and all entity configurations. Write migration.
- [ ] **Step 4:** Write `docker-compose.yml`. Verify MySQL container starts. Verify EF migration runs and creates tables.
- [ ] **Step 5:** Write tests for all Repositories (TDD). Then implement all Repositories. Run `dotnet test` — all pass.
- [ ] **Step 6:** Write tests for SettingsService (TDD). Implement SettingsService + MemoryCacheService. All tests pass.
- [ ] **Step 7:** Implement SeedData. Verify seed runs and default settings + sample data appears in DB.

### Phase 2: API Layer
- [ ] **Step 8:** Write integration tests for SettingsController (TDD). Implement SettingsController. Tests pass.
- [ ] **Step 9:** Write integration tests for CharactersController (TDD). Implement CharactersController. Tests pass.
- [ ] **Step 10:** Write integration tests for TopicsController (TDD). Implement TopicsController. Tests pass.
- [ ] **Step 11:** Write integration tests for EpisodesController (TDD). Implement EpisodesController. Tests pass.
- [ ] **Step 12:** Implement PipelineController, AnalyticsController, AssetsController. Write tests. Tests pass.
- [ ] **Step 13:** Implement ExceptionHandlingMiddleware. Verify all error responses are structured.
- [ ] **Step 14:** Verify Swagger UI at http://localhost:5000/swagger shows all endpoints.

### Phase 3: AI Services
- [ ] **Step 15:** Write unit tests for ClaudeScriptService (mock HTTP). Implement ClaudeScriptService. Tests pass.
- [ ] **Step 16:** Write unit tests for DalleImageService (mock HTTP). Implement DalleImageService. Tests pass.
- [ ] **Step 17:** Write unit tests for ElevenLabsVoiceService (mock HTTP). Implement ElevenLabsVoiceService. Tests pass.
- [ ] **Step 18:** Write unit tests for SunoMusicService (mock HTTP). Implement SunoMusicService. Tests pass.
- [ ] **Step 19:** Write unit tests for SeoGenerationService (mock HTTP). Implement SeoGenerationService. Tests pass.
- [ ] **Step 20:** Write unit tests for FfmpegVideoService (mock Process). Implement FfmpegVideoService. Tests pass.

### Phase 4: Pipeline Orchestration
- [ ] **Step 21:** Write unit tests for PipelineOrchestrator (mock all services). Implement PipelineOrchestrator. Tests pass.
- [ ] **Step 22:** Implement PipelineTriggerJob (Hangfire). Implement AnalyticsSyncJob. Write tests.
- [ ] **Step 23:** Write full integration test: FullPipelineIntegrationTests — mocks all external APIs, runs full pipeline, verifies Episode reaches PendingReview status.
- [ ] **Step 24:** Implement YouTubeUploadService. Write tests.

### Phase 5: Frontend Foundation
- [ ] **Step 25:** Scaffold Vite + React project. Install all packages. Verify `npm run dev` works.
- [ ] **Step 26:** Implement Layout (Sidebar + TopBar). Write component tests. Tests pass.
- [ ] **Step 27:** Implement all API modules (episodesApi, charactersApi, etc.) with Axios.
- [ ] **Step 28:** Implement all Context providers (AppContext, SettingsContext, PipelineContext).
- [ ] **Step 29:** Implement all shared components (LoadingSpinner, ConfirmModal, EmptyState, ErrorAlert). Write tests.

### Phase 6: Frontend Pages
- [ ] **Step 30:** Implement Settings page with all 5 tabs. Write tests. Verify API key save + test connection works end-to-end.
- [ ] **Step 31:** Implement Characters page + CharacterForm + CharacterCard. Write tests.
- [ ] **Step 32:** Implement Topics page + TopicForm + AI topic generation. Write tests.
- [ ] **Step 33:** Implement Episodes page + EpisodeCard + filters. Write tests.
- [ ] **Step 34:** Implement EpisodeDetail page with video player, pipeline timeline, metadata panel, approve/reject flow. Write tests.
- [ ] **Step 35:** Implement Dashboard page with stat cards, recent episodes, pipeline activity. Write tests.
- [ ] **Step 36:** Implement Analytics page with charts. Write tests.

### Phase 7: Integration & Polish
- [ ] **Step 37:** Run full end-to-end test: add API keys in Settings → add a topic → trigger pipeline → watch progress → approve → verify YouTube upload flow is called.
- [ ] **Step 38:** Verify all responsive breakpoints (mobile, tablet, desktop) on all pages.
- [ ] **Step 39:** Verify Docker Compose build works: `docker compose up --build` → app accessible at http://localhost:3000.
- [ ] **Step 40:** Run `dotnet test` — all backend tests pass. Run `npm test` — all frontend tests pass.
- [ ] **Step 41:** Update README.md with: prerequisites, quick start (`docker compose up`), first-time setup steps (go to Settings, add API keys), how to add characters and topics.

---

## Appendix A: Default Prompt Templates

### Script Generation Prompt (stored in AppSettings["Prompts:Script"])
```
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
{
  "title": "Catchy episode title including character name",
  "summary": "2-sentence parent-friendly description",
  "moral": "One-sentence lesson",
  "scenes": [
    {
      "scene_number": 1,
      "duration_seconds": 20,
      "background": "Detailed visual description of the setting for image generation. Include colors, time of day, objects present.",
      "action": "What physically happens. What characters are doing.",
      "characters_present": ["CharacterName"],
      "character_emotions": {"CharacterName": "happy"},
      "dialogue": [
        {
          "character": "CharacterName",
          "line": "What the character says",
          "tone": "excited"
        }
      ]
    }
  ]
}
```

### Image Prompt Builder (stored in AppSettings["Prompts:ImageBuilder"])
```
Convert the following scene into a DALL-E 3 image generation prompt.

Scene background: {background}
Action: {action}
Characters present and their emotions: {character_emotions_json}
Character visual descriptions: {character_descriptions_json}
Global art style: {global_style}

Rules:
- Lead with the art style
- Describe the scene setting vividly
- Describe each character's appearance exactly as provided
- Include the character's current emotion in their expression
- End with technical specs: "16:9 aspect ratio, no text, no watermarks"
- Output ONLY the image prompt text, nothing else
```

### SEO Metadata Prompt (stored in AppSettings["Prompts:SeoTitle"] etc.)
```
Generate YouTube SEO metadata for a children's cartoon episode.

Episode title: {title}
Summary: {summary}
Characters: {characters}
Moral: {moral}
Target audience: Parents searching for educational cartoons for kids aged 3-7

Return ONLY valid JSON:
{
  "seo_title": "Max 70 chars. Include character name + topic + 'Kids Cartoon' or 'for Kids'",
  "seo_description": "300-500 words. Start with episode summary. Include educational value. Parent-friendly. End with channel subscribe CTA. Use keywords naturally.",
  "tags": ["15-20 tags. Mix: character names, topics, 'kids cartoon', 'children animation', 'educational', age groups, moral topic"],
  "thumbnail_prompt": "Exciting scene from the episode for DALL-E thumbnail. Show character looking happy/surprised. Bright colors. Same art style as episode."
}
```

---

## Appendix B: API Response Formats

### Paged Response
```json
{
  "items": [...],
  "totalCount": 42,
  "page": 1,
  "pageSize": 12,
  "totalPages": 4
}
```

### Episode Response (full)
```json
{
  "id": 1,
  "title": "Leo Counts to 10",
  "summary": "...",
  "moral": "...",
  "status": "PendingReview",
  "statusLabel": "Pending Review",
  "videoUrl": "/api/assets/video/1",
  "thumbnailUrl": "/api/assets/thumbnail/1",
  "seoTitle": "...",
  "seoDescription": "...",
  "seoTags": ["tag1", "tag2"],
  "scheduledPublishAt": "2026-03-15T09:00:00Z",
  "youtubeVideoId": null,
  "youtubeUrl": null,
  "scenes": [...],
  "characters": [{ "id": 1, "name": "Leo", "avatarUrl": "..." }],
  "currentStageError": null,
  "createdAt": "2026-03-08T10:00:00Z",
  "updatedAt": "2026-03-08T12:30:00Z"
}
```

### Settings Status Response
```json
{
  "configured": ["Anthropic", "OpenAI", "ElevenLabs"],
  "missing": ["Suno", "YouTube"],
  "isFullyConfigured": false,
  "requiredMissing": ["YouTube"]
}
```

### Pipeline Status Response
```json
{
  "isActive": true,
  "activeEpisodes": 1,
  "queuedTopics": 4,
  "pendingReview": 2,
  "nextScheduledRun": "2026-03-08T16:00:00Z",
  "recentJobs": [
    {
      "id": 5,
      "episodeId": 3,
      "episodeTitle": "Luna Shares Her Carrots",
      "stage": "ImageGeneration",
      "status": "Running",
      "startedAt": "2026-03-08T14:22:00Z",
      "durationSeconds": 45
    }
  ]
}
```

---

*End of E2E Build Plan. Total estimated build time with AI agent: 4–8 hours. All decisions are final. Build everything.*
