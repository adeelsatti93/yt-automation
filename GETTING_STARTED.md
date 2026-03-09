# Kids Cartoon Pipeline — Getting Started

A step-by-step guide to get your automated YouTube kids cartoon pipeline running.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/) and [pnpm](https://pnpm.io/installation) (`npm install -g pnpm`)
- **MySQL 8+** running locally (or a remote instance you have credentials for)
- **Visual Studio 2022** *(optional)* — or use the .NET CLI

---

## Step 0: Configure the Database Connection

Before starting the app, open `backend/KidsCartoonPipeline.API/appsettings.json` and update the connection string with your MySQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=KidsCartoonPipeline;Uid=root;Pwd=YOUR_MYSQL_ROOT_PASSWORD;Allow User Variables=true;"
  }
}
```

> The database schema and seed data are created automatically on first launch — no manual SQL needed.

---

## Step 1: Start the Application

### Option A — Visual Studio 2022

1. Open `backend/KidsCartoonPipeline.slnx` in **Visual Studio 2022**
2. Right-click the solution → **Set Startup Projects** → Multiple startup projects
3. Set both **KidsCartoonPipeline.API** and **KidsCartoonPipeline.Worker** to **Start**
4. Press **F5** — the API runs on `http://localhost:5018`, the database gets auto-migrated and seeded

### Option B — CLI

```bash
# Terminal 1 — API
cd backend/KidsCartoonPipeline.API
dotnet run

# Terminal 2 — Worker
cd backend/KidsCartoonPipeline.Worker
dotnet run
```

Then in a third terminal:

```bash
cd frontend
pnpm install
pnpm run dev
```

Open **http://localhost:5173** in your browser.

---

## Step 2: Configure API Keys (Settings → API Keys tab)

Go to **Settings** in the sidebar. Enter your API keys in this order:

| Service | What It Does | Get Key |
|---------|-------------|---------|
| **Anthropic (Claude)** | Writes the cartoon script & builds image prompts | [console.anthropic.com](https://console.anthropic.com/account/keys) |
| **OpenAI (DALL·E 3)** | Generates scene images | [platform.openai.com](https://platform.openai.com/api-keys) |
| **ElevenLabs** | Generates character voices (TTS) | [elevenlabs.io](https://elevenlabs.io/subscription) |
| **Suno** *(optional)* | Generates background music | — |
| **Fal.ai** *(optional)* | Kling AI animation — character movement & lip sync | [fal.ai/dashboard](https://fal.ai/dashboard) |

Click the **✓** button after each key to save. Use **Test Connection** to verify each one.

---

## Step 3: Connect YouTube (Settings → API Keys tab)

1. Go to [Google Cloud Console](https://console.cloud.google.com/apis/credentials) → Create a project
2. Enable **YouTube Data API v3** in APIs & Services
3. Create an **OAuth 2.0 Client ID** (Web Application type)
4. Add `http://localhost:5018/api/youtube/callback` as an **Authorised Redirect URI**
5. Back in Settings, enter your **Client ID** and **Client Secret**
6. Click **"Authorize with YouTube"** → Sign in with Google → Grant access
7. You'll see **Connected** when done

---

## Step 4: Create Characters (Characters page)

1. Go to **Characters** in the sidebar
2. Click **"+ New Character"**
3. Fill in:
   - **Name** — e.g. `Lily the Fox`
   - **Description** — personality traits used in script generation
   - **Voice ID** — ElevenLabs voice ID (find in your ElevenLabs dashboard)
   - **Voice Settings** — JSON: `{"stability": 0.5, "similarity_boost": 0.75}`
4. Create at least **2–3 characters** for your cartoon

---

## Step 5: Add Topics (Topics page)

1. Go to **Topics** in the sidebar
2. **Option A:** Click **"+ Add Topic"** and fill in title, description, and target moral
3. **Option B:** Click **"Generate Ideas"** to let Claude suggest topic ideas
4. Add at least a few topics — these become queued episodes

---

## Step 6: Configure Pipeline Settings (Settings → Pipeline tab)

| Setting | Recommended | Description |
|---------|-------------|-------------|
| Auto-run Pipeline | Enabled | Worker picks topics automatically |
| Schedule | `*/30 * * * *` | How often to check for new work |
| Max Concurrent | 1 | Episodes processing at the same time |
| Video Engine | `FFmpeg` | `FFmpeg` = free 2D slideshow (no extra key needed). `Kling` = AI-animated with movement & lip sync (~$6/episode, requires Fal.ai key) |
| Default Publish Time | 10:00 | When to schedule YouTube uploads |
| Publish Days | Saturday, Sunday | Which days to publish |

---

## Step 7: Customize Image Style (Settings → Image Style tab)

- Set a **Global Art Style Prompt** — e.g. `"2D flat cartoon, bright saturated colors, child-friendly, rounded shapes"`
- Choose **Quality** (`standard` / `hd`) and **Size** (landscape recommended: `1792×1024`)

---

## Step 8: Review Prompts (Settings → Prompts tab)

Pre-configured prompts for:
- **Script Generation** — how Claude writes episode scripts
- **Image Prompt Builder** — how scene descriptions are crafted for DALL·E
- **SEO Title / Description** — how YouTube metadata is generated

Edit these to match your channel's style and tone.

---

## Step 9: Configure YouTube Upload (Settings → YouTube tab)

| Setting | Recommended |
|---------|-------------|
| Category ID | `27` (Education) |
| Privacy | Private (review first) or Public |
| Made for Kids | Yes |
| Auto-Publish | Disabled (review first) |
| Description Suffix | Your hashtags, links, etc. |

---

## Step 10: Produce Your First Episode!

### Automatic Mode
If Pipeline is **Enabled**, the Worker will automatically:
1. Pick the highest-priority unused topic
2. Create an episode
3. Run the full pipeline (script → images → voices → music → video → SEO)
4. Upload to YouTube (if auto-publish is on)

### Manual Mode
1. Go to **Topics** → find a topic → click **"Produce Now"**
2. Go to **Episodes** → click on the new episode → watch the pipeline progress
3. When complete, **review** the video preview
4. Click **Approve** to publish or **Reject** to discard

---

## Pipeline Stages

Each episode goes through these stages automatically:

```
Topic Queued → Script Writing → Image Generation → Voice Generation
             → Music Generation → Video Assembly → SEO Optimization
             → Ready for Review → Approved → Uploading → Published ✅
```

Track progress on the **Episode Detail** page with real-time pipeline logs.

---

## Monitoring

| Page | What it shows |
|------|--------------|
| **Dashboard** | Overview of pipeline status, recent episodes, quick stats |
| **Episodes** | Browse all episodes, filter by status |
| **Analytics** | YouTube performance data (views, watch time) after publishing |
| **/hangfire** | Background job queue & history |
| **/swagger** | API explorer for all endpoints |

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| API key test fails | Re-check the key, make sure it's saved (click ✓) |
| YouTube auth fails | Ensure redirect URI matches exactly in Google Cloud Console |
| Pipeline stuck | Check Episode Detail page for error logs |
| Worker not processing | Ensure the Worker project is also running alongside the API |
| Frontend 500 errors | Make sure the API project is running on port 5018 |
| EF Core migration error | Confirm MySQL is running and the connection string credentials are correct |
