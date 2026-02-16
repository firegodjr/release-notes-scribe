# Plan: release-notes-scribe

## Context

Create a .NET 8 console application that generates customer-facing release notes by querying Azure DevOps for closed work items under a given iteration path, letting the user select relevant items via an interactive terminal UI, and then using Azure OpenAI (gpt-4.1) to write a formatted release article.

## Technology Stack

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.TeamFoundationServer.Client` | 19.225.1 | ADO work item queries via WIQL |
| `Microsoft.VisualStudio.Services.Client` | 19.225.1 | PAT authentication (`VssBasicCredential`) |
| `Azure.AI.OpenAI` | 2.1.0 | Azure OpenAI chat completions |
| `Spectre.Console` | 0.49.1 | Terminal UI: multi-select, tables, spinners |
| `DotNetEnv` | 3.1.1 | Load `.env` file |

## Project Structure

```
release-notes-scribe/
├── ReleaseNotesScribe.sln
├── .gitignore
├── .env
├── .env.example
└── src/ReleaseNotesScribe/
    ├── ReleaseNotesScribe.csproj
    ├── Program.cs
    ├── Configuration/
    │   └── AppSettings.cs
    ├── Models/
    │   └── WorkItemInfo.cs
    ├── Services/
    │   ├── DevOpsService.cs
    │   ├── AiService.cs
    │   └── PromptTemplate.cs
    └── UI/
        └── ConsoleUI.cs
```
