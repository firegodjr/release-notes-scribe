using System.Text;
using System.Text.RegularExpressions;
using ReleaseNotesScribe.Models;

namespace ReleaseNotesScribe.Services;

public static partial class PromptTemplate
{
    public const string SystemMessage = """
        You are a technical writer creating customer-facing release notes for a software product.

        Output format: Markdown with the following sections (only include sections that have relevant items):
        - ## New Features
        - ## Improvements
        - ## Bug Fixes
        - ## Technical Debt

        Mapping rules:
        - Epic, Feature → New Features
        - User Story → Improvements (unless it clearly describes a new capability, then New Features)
        - Defect → Bug Fixes
        - Design Debt → Technical Debt

        Guidelines:
        - Write in a professional, customer-friendly tone
        - Each item should be a concise bullet point (1-2 sentences)
        - Focus on the *user impact* — what changed and why it matters
        - Do not include internal IDs, iteration paths, or implementation details
        - Group related items together when possible
        - Start with a brief introductory paragraph summarizing the release
        """;

    public static string BuildUserMessage(string versionLabel, IReadOnlyList<WorkItemInfo> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Generate release notes for **{versionLabel}**.");
        sb.AppendLine();
        sb.AppendLine("Here are the completed work items:");
        sb.AppendLine();

        var grouped = items.GroupBy(i => i.WorkItemType).OrderBy(g => g.Key);
        foreach (var group in grouped)
        {
            sb.AppendLine($"### {group.Key}");
            foreach (var item in group)
            {
                sb.AppendLine($"- **{item.Title}**");
                if (!string.IsNullOrWhiteSpace(item.Description))
                    sb.AppendLine($"  Description: {Truncate(StripHtml(item.Description), 500)}");
                if (!string.IsNullOrWhiteSpace(item.AcceptanceCriteria))
                    sb.AppendLine($"  Acceptance Criteria: {Truncate(StripHtml(item.AcceptanceCriteria), 300)}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string StripHtml(string html)
    {
        var text = HtmlTagRegex().Replace(html, " ");
        text = HtmlEntityRegex().Replace(text, " ");
        return WhitespaceRegex().Replace(text, " ").Trim();
    }

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : string.Concat(text.AsSpan(0, maxLength), "...");

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex("&[^;]+;")]
    private static partial Regex HtmlEntityRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
