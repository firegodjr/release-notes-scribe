namespace ReleaseNotesScribe.Helpers;

public static class IterationPathHelper
{
    /// <summary>
    /// Splits a comma-separated iteration input and normalizes each path:
    /// - If no backslash, prepends "development\"
    /// - If project prefix is missing, prepends it
    /// </summary>
    public static string[] NormalizePaths(string iterationInput, string adoProject)
    {
        return iterationInput
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => NormalizeSinglePath(p, adoProject))
            .ToArray();
    }

    private static string NormalizeSinglePath(string path, string adoProject)
    {
        // If no backslash, assume it's under development\
        if (!path.Contains('\\'))
            path = $"development\\{path}";

        // Prepend project prefix if missing
        if (!path.StartsWith(adoProject + "\\", StringComparison.OrdinalIgnoreCase))
            path = $"{adoProject}\\{path}";

        return path;
    }
}
