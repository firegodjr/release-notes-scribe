namespace ReleaseNotesScribe.Gui.Models;

// --- Config ---

public record ConfigStatusResponse(
    bool IsConfigured,
    List<ConfigVariable> Variables);

public record ConfigVariable(
    string Name,
    bool IsSet,
    bool IsSecret,
    string? DisplayValue);

// --- Work Items ---

public record WorkItemQueryRequest(
    string IterationPath,
    bool OnlyClosed = true);

public record WorkItemDto(
    int Id,
    string Title,
    string WorkItemType,
    string State,
    string? Description,
    string? AcceptanceCriteria,
    string IterationPath);

public record WorkItemQueryResponse(
    List<WorkItemDto> Items,
    string[] NormalizedPaths);

// --- Generation ---

public record GenerateRequest(
    string VersionLabel,
    List<WorkItemDto> Items);

public record GenerateResponse(string Markdown);
