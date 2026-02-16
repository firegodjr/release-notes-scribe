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

// Detect non-interactive mode: iteration path passed as CLI arg
var interactive = args.Length == 0 && !Console.IsInputRedirected;

// 2. Get iteration path
string iterationPath;
if (args.Length > 0)
{
    iterationPath = args[0];
    AnsiConsole.MarkupLine($"Iteration path: [green]{iterationPath.EscapeMarkup()}[/]");
}
else
{
    iterationPath = ConsoleUI.PromptIterationPath();
}

// If the user already included the project prefix, use as-is; otherwise prepend it
var fullIterationPath = iterationPath.StartsWith(settings.AdoProject + "\\", StringComparison.OrdinalIgnoreCase)
    ? iterationPath
    : $"{settings.AdoProject}\\{iterationPath}";

// 3. Query Azure DevOps
var devOps = new DevOpsService(settings);
List<ReleaseNotesScribe.Models.WorkItemInfo> workItems;
try
{
    workItems = await ConsoleUI.WithSpinner(
        $"Querying Azure DevOps for closed items under [green]{fullIterationPath.EscapeMarkup()}[/]...",
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
    versionLabel = ConsoleUI.PromptVersionLabel(iterationPath);
}
else
{
    versionLabel = iterationPath.Contains('\\')
        ? iterationPath[(iterationPath.LastIndexOf('\\') + 1)..]
        : iterationPath;
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
