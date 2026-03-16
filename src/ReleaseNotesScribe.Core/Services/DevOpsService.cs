using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using ReleaseNotesScribe.Configuration;
using ReleaseNotesScribe.Models;

namespace ReleaseNotesScribe.Services;

public class DevOpsService
{
    private readonly AppSettings _settings;
    private static readonly string[] FieldNames =
    [
        "System.Id",
        "System.Title",
        "System.WorkItemType",
        "System.State",
        "System.Description",
        "Microsoft.VSTS.Common.AcceptanceCriteria",
        "System.IterationPath"
    ];

    public DevOpsService(AppSettings settings)
    {
        _settings = settings;
    }

    public async Task<List<WorkItemInfo>> QueryClosedWorkItemsAsync(IEnumerable<string> iterationPaths, bool onlyClosed)
    {
        var orgUrl = new Uri($"https://dev.azure.com/{_settings.AdoOrg}");
        var credentials = new VssBasicCredential(string.Empty, _settings.AdoPat);
        using var connection = new VssConnection(orgUrl, credentials);
        var witClient = connection.GetClient<WorkItemTrackingHttpClient>();

        var pathClauses = iterationPaths
            .Select(p => $"[System.IterationPath] UNDER '{p}'");
        var iterationFilter = string.Join(" OR ", pathClauses);

        // Step 1: Query IDs via WIQL
        var wiql = new Wiql
        {
            Query = $"""
                SELECT [System.Id]
                FROM WorkItems
                WHERE [System.TeamProject] = '{_settings.AdoProject}'
                  AND ({iterationFilter})
                  AND [System.WorkItemType] IN ('Epic','Feature','User Story','Defect','Design Debt')
                  {(onlyClosed ? "AND [System.State] IN ('Closed', 'Done')" : "AND [System.State] NOT IN ('Removed')")}
                ORDER BY [System.WorkItemType] ASC, [System.Id] ASC
                """
        };

        var queryResult = await witClient.QueryByWiqlAsync(wiql);
        var ids = queryResult.WorkItems.Select(wi => wi.Id).ToList();

        if (ids.Count == 0)
            return [];

        // Step 2: Batch fetch in chunks of 200
        var results = new List<WorkItemInfo>();
        foreach (var chunk in ids.Chunk(200))
        {
            var workItems = await witClient.GetWorkItemsAsync(
                chunk,
                fields: FieldNames,
                expand: WorkItemExpand.None);

            results.AddRange(workItems.Select(MapToWorkItemInfo));
        }

        return results;
    }

    private static WorkItemInfo MapToWorkItemInfo(WorkItem wi)
    {
        return new WorkItemInfo(
            Id: wi.Id ?? 0,
            Title: GetField<string>(wi, "System.Title") ?? "(no title)",
            WorkItemType: GetField<string>(wi, "System.WorkItemType") ?? "Unknown",
            State: GetField<string>(wi, "System.State") ?? "Unknown",
            Description: GetField<string>(wi, "System.Description"),
            AcceptanceCriteria: GetField<string>(wi, "Microsoft.VSTS.Common.AcceptanceCriteria"),
            IterationPath: GetField<string>(wi, "System.IterationPath") ?? ""
        );
    }

    private static T? GetField<T>(WorkItem wi, string fieldName)
    {
        return wi.Fields.TryGetValue(fieldName, out var value) && value is T typed
            ? typed
            : default;
    }
}
