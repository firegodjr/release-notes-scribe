using Microsoft.AspNetCore.Mvc;
using ReleaseNotesScribe.Gui.Models;
using ReleaseNotesScribe.Models;
using ReleaseNotesScribe.Services;

namespace ReleaseNotesScribe.Gui.Controllers;

[ApiController]
[Route("api/generate")]
public class GenerateController(ConfigStatus configStatus, IServiceProvider services) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<GenerateResponse>> Generate([FromBody] GenerateRequest request)
    {
        if (!configStatus.IsConfigured)
            return BadRequest("Configuration is incomplete. Please set all required environment variables.");

        var aiService = services.GetRequiredService<AiService>();

        var workItems = request.Items.Select(dto => new WorkItemInfo(
            dto.Id, dto.Title, dto.WorkItemType, dto.State,
            dto.Description, dto.AcceptanceCriteria, dto.IterationPath
        )).ToList();

        var markdown = await aiService.GenerateReleaseNotesAsync(request.VersionLabel, workItems);

        return Ok(new GenerateResponse(markdown));
    }
}
