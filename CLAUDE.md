# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build                                          # build the solution
dotnet run --project src/ReleaseNotesScribe           # interactive mode (requires a TTY)
dotnet run --project src/ReleaseNotesScribe -- 'development\4.0'  # non-interactive, includes all items
```

No tests exist yet. No linter is configured.

## Configuration

The app loads a `.env` file (searched upward from cwd) via DotNetEnv. Required variables are in `.env.example`. All six vars (`ADO_ORG`, `ADO_PROJECT`, `ADO_PAT`, `AZ_AI_ENDPOINT`, `AZ_AI_DEPLOYMENT`, `AZ_AI_KEY`) must be set or the app fails fast.

`AZ_AI_ENDPOINT` may contain a full path (e.g. `.../openai/v1/chat/completions`); `AppSettings.GetAzureOpenAiBaseUri()` strips it to scheme+host because the Azure OpenAI SDK appends its own path.

## Architecture

Single-project .NET 8 console app (`src/ReleaseNotesScribe/`) with top-level `Program.cs` orchestration. Two modes: **interactive** (Spectre.Console prompts for iteration path, multi-select, version label, save) and **non-interactive** (iteration path via CLI arg, all items included, auto-save).

**Flow:** Load config → get iteration path → WIQL query ADO for closed work items → display table → select items → send to Azure OpenAI → display/save markdown.

Key design decisions:
- **DevOpsService**: Two-step ADO fetch — WIQL query returns IDs only, then `GetWorkItemsAsync` batch-fetches in chunks of 200. The WIQL uses `UNDER` to match the iteration path and all sub-paths.
- **AiService**: Uses `Azure.AI.OpenAI` (`AzureOpenAIClient` + `ApiKeyCredential`). Temperature 0.4, max 4096 output tokens.
- **PromptTemplate**: Static class with system prompt (role + output format + type-to-category mapping) and user message builder that groups items by type, strips HTML, and truncates descriptions (500 chars) and acceptance criteria (300 chars).
- **ConsoleUI**: All Spectre.Console interactions isolated here. Work item types are color-coded (Epic=magenta, Feature=blue, User Story=green, Defect=red, Design Debt=yellow).
- **Iteration path handling**: Program.cs auto-prepends `AdoProject\` if the user-provided path doesn't already start with it.
