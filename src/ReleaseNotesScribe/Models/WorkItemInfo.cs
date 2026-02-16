namespace ReleaseNotesScribe.Models;

public record WorkItemInfo(
    int Id,
    string Title,
    string WorkItemType,
    string State,
    string? Description,
    string? AcceptanceCriteria,
    string IterationPath)
{
    public override string ToString() => $"[{WorkItemType}] #{Id}: {Title}";
}
