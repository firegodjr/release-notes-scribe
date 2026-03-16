# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build                                          # build entire solution (3 projects)
dotnet run --project src/ReleaseNotesScribe           # console app, interactive mode (requires a TTY)
dotnet run --project src/ReleaseNotesScribe -- 'development\4.0'  # console app, non-interactive
dotnet run --project src/ReleaseNotesScribe.Gui       # GUI backend (web mode, http://localhost:5000)
```

**GUI dev workflow** (two terminals):
```bash
dotnet run --project src/ReleaseNotesScribe.Gui                    # Terminal 1: ASP.NET Core backend
cd src/ReleaseNotesScribe.Gui/ClientApp && npm run dev             # Terminal 2: Vite dev server (http://localhost:5173)
```

**Electron desktop mode:**
```bash
dotnet tool install ElectronNET.CLI -g                             # one-time CLI install
cd src/ReleaseNotesScribe.Gui && electronize start                 # launch desktop window
```

No tests exist yet. No linter is configured.

## Configuration

The app loads a `.env` file (searched upward from cwd) via DotNetEnv. Required variables are in `.env.example`. All six vars (`ADO_ORG`, `ADO_PROJECT`, `ADO_PAT`, `AZ_AI_ENDPOINT`, `AZ_AI_DEPLOYMENT`, `AZ_AI_KEY`) must be set or the console app fails fast. The GUI app degrades gracefully — it redirects to a Missing Configuration page showing which variables are missing.

`AZ_AI_ENDPOINT` may contain a full path (e.g. `.../openai/v1/chat/completions`); `AppSettings.GetAzureOpenAiBaseUri()` strips it to scheme+host because the Azure OpenAI SDK appends its own path.

## Project Structure

```
src/
├── ReleaseNotesScribe.Core/         # Shared class library (net8.0)
│   ├── Configuration/AppSettings.cs # Load() throws, TryLoad() returns tuple
│   ├── Models/WorkItemInfo.cs       # Immutable record
│   ├── Services/DevOpsService.cs    # Azure DevOps WIQL + batch fetch
│   ├── Services/AiService.cs        # Azure OpenAI chat completion
│   ├── Services/PromptTemplate.cs   # System/user prompt builder, HTML stripping
│   └── Helpers/IterationPathHelper.cs # Path normalization (auto-prefix logic)
│
├── ReleaseNotesScribe/              # Console app (references Core)
│   ├── Program.cs                   # Top-level orchestration, two modes
│   └── UI/ConsoleUI.cs              # Spectre.Console interactions
│
└── ReleaseNotesScribe.Gui/          # Electron.NET + ASP.NET Core (references Core)
    ├── Program.cs                   # DI setup, Electron BrowserWindow, static files
    ├── electron.manifest.json       # Electron.NET config (singleInstance)
    ├── Controllers/
    │   ├── ConfigController.cs      # GET /api/config/status
    │   ├── WorkItemsController.cs   # POST /api/workitems/query
    │   └── GenerateController.cs    # POST /api/generate
    ├── Models/Dtos.cs               # Request/response DTOs for all endpoints
    └── ClientApp/                   # Vue 3 SPA (built to ../wwwroot by Vite)
        └── src/
            ├── router/index.ts      # 3 routes, nav guard checks config status
            ├── stores/config.ts     # Pinia store: config status
            ├── stores/workitems.ts  # Pinia store: items, selection, generation
            ├── api/client.ts        # Typed fetch wrapper for all endpoints
            ├── views/               # MissingConfigView, WorkItemsView, GenerationView
            └── components/          # MarkdownPreview (renders via `marked`)
```

## Architecture

**Core library** (`ReleaseNotesScribe.Core`): Uses `RootNamespace=ReleaseNotesScribe` so all namespaces are unchanged from the original single-project layout. Both the console app and GUI reference it.

**Console app flow:** Load config → get iteration path → WIQL query ADO for closed work items → display table → select items → send to Azure OpenAI → display/save markdown.

**GUI app flow:** ASP.NET Core serves the Vue SPA from `wwwroot/`. The SPA calls REST endpoints that delegate to the same Core services. Electron.NET wraps it in a desktop window (1200×800). Without Electron, it runs as a normal web app.

Key design decisions:
- **DevOpsService**: Two-step ADO fetch — WIQL query returns IDs only, then `GetWorkItemsAsync` batch-fetches in chunks of 200. The WIQL uses `UNDER` to match the iteration path and all sub-paths.
- **AiService**: Uses `Azure.AI.OpenAI` (`AzureOpenAIClient` + `ApiKeyCredential`). Temperature 0.4, max 4096 output tokens.
- **PromptTemplate**: Static class with system prompt (role + output format + type-to-category mapping) and user message builder that groups items by type, strips HTML, and truncates descriptions (500 chars) and acceptance criteria (300 chars).
- **AppSettings**: `Load()` throws on missing vars (used by console app). `TryLoad()` returns `(AppSettings?, string[])` without throwing (used by GUI).
- **IterationPathHelper**: Extracted path normalization — auto-prepends `development\` if no backslash, then project prefix if missing.
- **ConsoleUI**: All Spectre.Console interactions isolated here. Work item types are color-coded (Epic=magenta, Feature=blue, User Story=green, Defect=red, Design Debt=yellow).
- **GUI build integration**: MSBuild target in Gui `.csproj` runs `npm install && npm run build` before .NET build when `wwwroot/index.html` doesn't exist. Vite outputs to `../wwwroot`.
- **Vue SPA**: PrimeVue (Aura theme) for UI components, Pinia for state, vue-router with navigation guard that redirects to `/missing-config` when env vars are missing.
