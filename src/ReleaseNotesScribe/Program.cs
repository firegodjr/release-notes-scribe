using ReleaseNotesScribe.Configuration;
using ReleaseNotesScribe.Services;
using ReleaseNotesScribe.UI;
using Spectre.Console;

AnsiConsole.Write(new FigletText("Release Notes Scribe")
    .Color(Color.Blue));

// 1. Load configuration
AppSettings settings;
try
{
    settings = AppSettings.Load();
}
catch (InvalidOperationException ex)
{
    ConsoleUI.WriteError(ex.Message);
    return 1;
}

// 2. Prompt for iteration path
var iterationPath = ConsoleUI.PromptIterationPath();
var fullIterationPath = $"{settings.AdoProject}\\{iterationPath}";

// 3. Query Azure DevOps
var devOps = new DevOpsService(settings);
List<ReleaseNotesScribe.Models.WorkItemInfo> workItems;
try
{
    workItems = await ConsoleUI.WithSpinner(
        $"Querying Azure DevOps for closed items under [green]{fullIterationPath}[/]...",
        () => devOps.QueryClosedWorkItemsAsync(fullIterationPath));
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
    return 1;
}

if (workItems.Count == 0)
{
    ConsoleUI.WriteWarning("No closed work items found for that iteration path.");
    return 0;
}

// 4. Display table and prompt selection
ConsoleUI.DisplayWorkItemTable(workItems);
var selected = ConsoleUI.PromptSelectWorkItems(workItems);

if (selected.Count == 0)
{
    ConsoleUI.WriteWarning("No items selected. Exiting.");
    return 0;
}

AnsiConsole.MarkupLine($"\n[dim]{selected.Count} item(s) selected[/]");

// 5. Prompt for version label
var versionLabel = ConsoleUI.PromptVersionLabel(iterationPath);

// 6. Generate release notes via Azure OpenAI
var aiService = new AiService(settings);
string releaseNotes;
try
{
    releaseNotes = await ConsoleUI.WithSpinner(
        "Generating release notes with AI...",
        () => aiService.GenerateReleaseNotesAsync(versionLabel, selected));
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
    return 1;
}

// 7. Display and optionally save
ConsoleUI.DisplayReleaseNotes(releaseNotes);
await ConsoleUI.PromptSaveToFile(releaseNotes);

return 0;
