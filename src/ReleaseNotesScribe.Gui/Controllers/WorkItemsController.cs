using Microsoft.AspNetCore.Mvc;
using ReleaseNotesScribe.Gui.Models;
using ReleaseNotesScribe.Helpers;
using ReleaseNotesScribe.Services;

namespace ReleaseNotesScribe.Gui.Controllers;

[ApiController]
[Route("api/workitems")]
public class WorkItemsController(ConfigStatus configStatus, IServiceProvider services) : ControllerBase
{
    [HttpPost("query")]
    public async Task<ActionResult<WorkItemQueryResponse>> Query([FromBody] WorkItemQueryRequest request)
    {
        if (!configStatus.IsConfigured)
            return BadRequest("Configuration is incomplete. Please set all required environment variables.");

        var devOps = services.GetRequiredService<DevOpsService>();
        var normalizedPaths = IterationPathHelper.NormalizePaths(
            request.IterationPath, configStatus.Settings!.AdoProject);

        var workItems = await devOps.QueryClosedWorkItemsAsync(normalizedPaths, request.OnlyClosed);

        var items = workItems.Select(wi => new WorkItemDto(
            wi.Id, wi.Title, wi.WorkItemType, wi.State,
            wi.Description, wi.AcceptanceCriteria, wi.IterationPath
        )).ToList();

        return Ok(new WorkItemQueryResponse(items, normalizedPaths));
    }
}
