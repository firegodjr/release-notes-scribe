using Microsoft.AspNetCore.Mvc;
using ReleaseNotesScribe.Configuration;
using ReleaseNotesScribe.Gui.Models;

namespace ReleaseNotesScribe.Gui.Controllers;

[ApiController]
[Route("api/config")]
public class ConfigController(ConfigStatus configStatus) : ControllerBase
{
    [HttpGet("status")]
    public ActionResult<ConfigStatusResponse> GetStatus()
    {
        var variables = AppSettings.RequiredVariableNames.Select(name =>
        {
            var value = Environment.GetEnvironmentVariable(name);
            var isSet = !string.IsNullOrWhiteSpace(value);
            var isSecret = AppSettings.SecretVariableNames.Contains(name);
            var displayValue = isSet
                ? (isSecret ? "********" : value)
                : null;

            return new ConfigVariable(name, isSet, isSecret, displayValue);
        }).ToList();

        return Ok(new ConfigStatusResponse(configStatus.IsConfigured, variables));
    }
}
