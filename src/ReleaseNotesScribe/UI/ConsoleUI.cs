using ReleaseNotesScribe.Models;
using Spectre.Console;

namespace ReleaseNotesScribe.UI;

public static class ConsoleUI
{
    private static readonly Dictionary<string, string> TypeColors = new()
    {
        ["Epic"] = "magenta",
        ["Feature"] = "blue",
        ["User Story"] = "green",
        ["Defect"] = "red",
        ["Design Debt"] = "yellow"
    };

    public static string PromptIterationPath()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the [green]iteration path[/] (e.g. [dim]development\\v4.2[/]):")
                .ValidationErrorMessage("[red]Iteration path cannot be empty[/]")
                .Validate(input => !string.IsNullOrWhiteSpace(input)
                    ? ValidationResult.Success()
                    : ValidationResult.Error()));
    }

    public static void DisplayWorkItemTable(IReadOnlyList<WorkItemInfo> items)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("ID")
            .AddColumn("Type")
            .AddColumn("Title")
            .AddColumn("State")
            .AddColumn("Iteration");

        foreach (var item in items)
        {
            var color = TypeColors.GetValueOrDefault(item.WorkItemType, "white");
            table.AddRow(
                $"[dim]{item.Id}[/]",
                $"[{color}]{item.WorkItemType.EscapeMarkup()}[/]",
                item.Title.EscapeMarkup(),
                item.State.EscapeMarkup(),
                $"[dim]{item.IterationPath.EscapeMarkup()}[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[dim]Found {items.Count} work item(s)[/]");
    }

    public static List<WorkItemInfo> PromptSelectWorkItems(IReadOnlyList<WorkItemInfo> items)
    {
        var prompt = new MultiSelectionPrompt<WorkItemInfo>()
            .Title("Select work items to include in release notes:")
            .PageSize(20)
            .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to accept)[/]")
            .UseConverter(item =>
            {
                var color = TypeColors.GetValueOrDefault(item.WorkItemType, "white");
                return $"[{color}]{item.WorkItemType.EscapeMarkup()}[/] #{item.Id}: {item.Title.EscapeMarkup()}";
            })
            .AddChoices(items);

        // Pre-select all items
        foreach (var item in items)
            prompt.Select(item);

        return AnsiConsole.Prompt(prompt);
    }

    public static string PromptVersionLabel(string iterationPath)
    {
        var defaultLabel = iterationPath.Contains('\\')
            ? iterationPath[(iterationPath.LastIndexOf('\\') + 1)..]
            : iterationPath;

        return AnsiConsole.Prompt(
            new TextPrompt<string>($"Version label for the release notes [dim](default: {defaultLabel.EscapeMarkup()})[/]:")
                .DefaultValue(defaultLabel)
                .AllowEmpty());
    }

    public static void DisplayReleaseNotes(string markdown)
    {
        AnsiConsole.WriteLine();
        var panel = new Panel(markdown.EscapeMarkup())
            .Header("[green]Generated Release Notes[/]")
            .Border(BoxBorder.Double)
            .Expand();
        AnsiConsole.Write(panel);
    }

    public static async Task<string?> PromptSaveToFile(string markdown)
    {
        var save = AnsiConsole.Confirm("Save release notes to a file?", defaultValue: true);
        if (!save)
            return null;

        var filename = AnsiConsole.Prompt(
            new TextPrompt<string>("Filename:")
                .DefaultValue("release-notes.md"));

        await File.WriteAllTextAsync(filename, markdown);
        AnsiConsole.MarkupLine($"[green]Saved to {filename.EscapeMarkup()}[/]");
        return filename;
    }

    public static async Task<T> WithSpinner<T>(string message, Func<Task<T>> action)
    {
        return await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync(message, async _ => await action());
    }

    public static void WriteWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]{message.EscapeMarkup()}[/]");
    }

    public static void WriteError(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message.EscapeMarkup()}[/]");
    }
}
