using ReleaseNotesScribe.Configuration;
using ReleaseNotesScribe.Helpers;
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

// Detect non-interactive mode: iteration path passed as CLI arg
var interactive = args.Length == 0 && !Console.IsInputRedirected;

// 2. Get iteration path(s) — comma-separated
string iterationInput;
if (args.Length > 0)
{
    iterationInput = args[0];
    AnsiConsole.MarkupLine($"Iteration path(s): [green]{iterationInput.EscapeMarkup()}[/]");
}
else
{
    iterationInput = ConsoleUI.PromptIterationPath();
}

// Split on commas and normalize each path: auto-prepend development\ and project prefix
var iterationPaths = IterationPathHelper.NormalizePaths(iterationInput, settings.AdoProject);

// 3. Query Azure DevOps
var devOps = new DevOpsService(settings);
List<ReleaseNotesScribe.Models.WorkItemInfo> workItems;
var completedOnly = false;
try
{
    if (interactive)
    {
        completedOnly = ConsoleUI.PromptCompletedOnly();
    }

    var pathsDisplay = string.Join(", ", iterationPaths);
    workItems = await ConsoleUI.WithSpinner(
        $"Querying Azure DevOps for closed items under [green]{pathsDisplay.EscapeMarkup()}[/]...",
        () => devOps.QueryClosedWorkItemsAsync(iterationPaths, completedOnly));
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

List<ReleaseNotesScribe.Models.WorkItemInfo> selected;
if (interactive)
{
    selected = ConsoleUI.PromptSelectWorkItems(workItems);
    if (selected.Count == 0)
    {
        ConsoleUI.WriteWarning("No items selected. Exiting.");
        return 0;
    }
}
else
{
    selected = workItems;
    AnsiConsole.MarkupLine("[dim]Non-interactive mode: using all items[/]");
}

AnsiConsole.MarkupLine($"\n[dim]{selected.Count} item(s) selected[/]");

// 5. Get version label
string versionLabel;
if (interactive)
{
    versionLabel = ConsoleUI.PromptVersionLabel(iterationInput);
}
else
{
    versionLabel = iterationInput.Contains('\\')
        ? iterationInput[(iterationInput.LastIndexOf('\\') + 1)..]
        : iterationInput;
    AnsiConsole.MarkupLine($"Version label: [green]{versionLabel.EscapeMarkup()}[/]");
}

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

if (interactive)
{
    await ConsoleUI.PromptSaveToFile(releaseNotes);
}
else
{
    var filename = $"release-notes-{versionLabel.Replace(" ", "-").Replace("\\", "-")}.md";
    await File.WriteAllTextAsync(filename, releaseNotes);
    AnsiConsole.MarkupLine($"[green]Saved to {filename.EscapeMarkup()}[/]");
}

return 0;
