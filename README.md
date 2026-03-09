# Kids Cartoon Pipeline

An automated end-to-end pipeline that writes, illustrates, voices, and publishes AI-generated kids cartoon episodes to YouTube — all from a single web dashboard.

## How It Works

```
Topic → Script (Claude) → Scene Images (DALL·E 3) → Voices (ElevenLabs)
      → Music (Suno) → Video Assembly → SEO Metadata → YouTube Upload
```

You provide topics and characters. The pipeline handles everything else automatically on a configurable schedule.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend API | ASP.NET Core 10 (C#) |
| Background Worker | .NET Worker Service |
| Database | MySQL 8+ via Entity Framework Core (Pomelo) |
| Job Scheduling | Hangfire |
| Frontend | React 18 + Vite + Bootstrap 5 |
| Package Manager | pnpm |
| Logging | Serilog |

**External AI/API Services:**

| Service | Purpose |
|---------|---------|
| Anthropic Claude | Script writing & image prompt generation |
| OpenAI DALL·E 3 | Scene image generation |
| ElevenLabs | Character text-to-speech (TTS) |
| Suno *(optional)* | Background music generation |
| YouTube Data API v3 | Uploading & scheduling videos |

---

## Project Structure

```
YT-Automation/
├── backend/
│   ├── KidsCartoonPipeline.API/          # ASP.NET Core Web API (port 5018)
│   ├── KidsCartoonPipeline.Worker/       # Background job processor
│   ├── KidsCartoonPipeline.Core/         # Domain models & interfaces
│   ├── KidsCartoonPipeline.Infrastructure/ # EF Core, external service clients
│   ├── KidsCartoonPipeline.Tests/        # Unit & integration tests
│   └── KidsCartoonPipeline.slnx          # Solution file
├── frontend/                             # React + Vite dashboard (port 5173)
├── .env.example                          # Environment variable template
└── README.md
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/) and [pnpm](https://pnpm.io/installation)
- [MySQL 8+](https://dev.mysql.com/downloads/mysql/) running locally (or a remote instance)
- API keys for the services listed above

---

## Setup

### 1. Clone the repository

```bash
git clone https://github.com/your-username/yt-automation.git
cd yt-automation
```

### 2. Configure the database connection

Open `backend/KidsCartoonPipeline.API/appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=KidsCartoonPipeline;Uid=root;Pwd=YOUR_MYSQL_ROOT_PASSWORD;Allow User Variables=true;"
  }
}
```

Update the same connection string in the Worker project:
`backend/KidsCartoonPipeline.Worker/appsettings.json`

> The application runs EF Core migrations automatically on first start — no manual schema setup needed.

### 3. Run the backend

**Option A — Visual Studio:**
1. Open `backend/KidsCartoonPipeline.slnx`
2. Right-click the solution → **Set Startup Projects** → select **Multiple startup projects**
3. Set both `KidsCartoonPipeline.API` and `KidsCartoonPipeline.Worker` to **Start**
4. Press **F5**

**Option B — CLI:**

```bash
# Terminal 1 — API
cd backend/KidsCartoonPipeline.API
dotnet run

# Terminal 2 — Worker
cd backend/KidsCartoonPipeline.Worker
dotnet run
```

The API starts at `http://localhost:5018`. The database is migrated and seeded automatically.

### 4. Run the frontend

```bash
cd frontend
pnpm install
pnpm run dev
```

Open `http://localhost:5173` in your browser.

---

## Configuring API Keys (in the UI)

API keys are stored in the database, **not in config files**. After the app is running:

1. Go to **Settings → API Keys** in the sidebar
2. Enter and save each key:

| Service | Where to get it |
|---------|----------------|
| Anthropic | [console.anthropic.com/account/keys](https://console.anthropic.com/account/keys) |
| OpenAI | [platform.openai.com/api-keys](https://platform.openai.com/api-keys) |
| ElevenLabs | [elevenlabs.io/subscription](https://elevenlabs.io/subscription) — Settings → API Key |
| Suno | *(optional)* |

3. Click **Test Connection** next to each key to verify it works.

---

## Connecting YouTube

1. Go to [Google Cloud Console](https://console.cloud.google.com/apis/credentials)
2. Create a project and enable **YouTube Data API v3**
3. Create an **OAuth 2.0 Client ID** (Web Application type)
4. Add `http://localhost:5018/api/youtube/callback` as an **Authorised Redirect URI**
5. In the dashboard go to **Settings → API Keys**, enter your **Client ID** and **Client Secret**
6. Click **"Authorize with YouTube"** and complete the Google OAuth flow
7. You should see a green **Connected** status when done

---

## Usage

### Setting Up Characters

Go to **Characters → New Character** and fill in:

- **Name** — e.g. `Lily the Fox`
- **Description** — personality traits used in script prompts
- **Voice ID** — the ElevenLabs voice ID from your ElevenLabs dashboard
- **Voice Settings** — JSON, e.g. `{"stability": 0.5, "similarity_boost": 0.75}`

Create at least 2–3 characters before generating episodes.

### Adding Topics

Go to **Topics** and either:
- Click **+ Add Topic** and fill in the title, description, and moral of the episode
- Click **Generate Ideas** to have Claude suggest topics for you

### Running the Pipeline

**Automatic mode:** Enable **Auto-run Pipeline** in **Settings → Pipeline**. The Worker picks the next queued topic on the configured cron schedule and runs the full pipeline.

**Manual mode:** On any topic card, click **Produce Now** to immediately kick off a pipeline run for that topic. Monitor progress on the **Episode Detail** page.

### Pipeline Stages

```
Topic Queued → Script Writing → Image Generation → Voice Generation
             → Music Generation → Video Assembly → SEO Optimization
             → Ready for Review → Approved → Uploading → Published ✅
```

Each stage's logs are visible in real time on the Episode Detail page.

### Reviewing & Publishing

After the pipeline finishes, the episode enters **Ready for Review**. Open the episode and:
- Watch the video preview
- Click **Approve** to upload to YouTube
- Click **Reject** to discard

---

## Pipeline Settings Reference

In **Settings → Pipeline**:

| Setting | Default | Description |
|---------|---------|-------------|
| Auto-run Pipeline | Off | Worker automatically picks queued topics |
| Schedule (cron) | `*/30 * * * *` | How often the Worker checks for new work |
| Max Concurrent Episodes | 1 | Episodes processed simultaneously |
| Default Publish Time | 10:00 | Scheduled upload time on YouTube |
| Publish Days | Saturday, Sunday | Days on which episodes are published |

In **Settings → Image Style**:
- **Global Art Style Prompt** — e.g. `"2D flat cartoon, bright saturated colors, child-friendly, rounded shapes"`
- **Quality** — `standard` or `hd`
- **Size** — `1792×1024` (landscape recommended)

In **Settings → Prompts**: Edit the system prompts used for script generation, image prompt building, and SEO metadata to match your channel's tone.

---

## Monitoring

| Page | What it shows |
|------|--------------|
| Dashboard | Pipeline status, recent episodes, quick stats |
| Episodes | All episodes filterable by status |
| Analytics | YouTube views and watch-time data after publishing |
| `/hangfire` | Hangfire job dashboard (background job queue & history) |
| `/health` | API health check endpoint |
| `/swagger` | Swagger/OpenAPI explorer for all API endpoints |

---

## Running Tests

```bash
cd backend
dotnet test
```

---

## Environment Variables / Configuration Summary

All sensitive keys are managed through the Settings UI. The only values you need to set before first launch are:

**`backend/KidsCartoonPipeline.API/appsettings.json`**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=KidsCartoonPipeline;Uid=<user>;Pwd=<password>;Allow User Variables=true;"
  },
  "Frontend": {
    "Url": "http://localhost:5173"
  },
  "Storage": {
    "BasePath": "./storage"
  }
}
```

> The `Storage.BasePath` folder is where generated audio, images, and assembled videos are stored locally before being uploaded to YouTube.

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| API key test fails | Re-check the key and make sure you clicked the save (✓) button |
| YouTube auth fails | Verify the redirect URI in Google Cloud Console matches exactly |
| Pipeline stuck on a stage | Open the Episode Detail page and check the pipeline logs for error messages |
| Worker not processing topics | Make sure the Worker project is also running alongside the API |
| Frontend cannot reach the API | Verify the API is running on port 5018; the Vite proxy forwards `/api` calls there |
| EF Core migration error | Confirm MySQL is running and the connection string credentials are correct |

---

## Contributing

Pull requests are welcome. For major changes, open an issue first to discuss what you'd like to change.

## License

MIT
