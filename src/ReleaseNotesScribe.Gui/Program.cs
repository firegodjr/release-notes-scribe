using ElectronNET.API;
using ElectronNET.API.Entities;
using ReleaseNotesScribe.Configuration;
using ReleaseNotesScribe.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseElectron(args);

// Load configuration using TryLoad (non-throwing)
var (settings, missingVars) = AppSettings.TryLoad();

builder.Services.AddSingleton(new ConfigStatus(settings, missingVars));

if (settings is not null)
{
    builder.Services.AddSingleton(settings);
    builder.Services.AddSingleton<DevOpsService>();
    builder.Services.AddSingleton<AiService>();
}

builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("index.html");

if (HybridSupport.IsElectronActive)
{
    await app.StartAsync();

    var window = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
    {
        Width = 1200,
        Height = 800,
        AutoHideMenuBar = true,
        Title = "Release Notes Scribe"
    });

    window.OnClosed += () => Electron.App.Quit();

    await app.WaitForShutdownAsync();
}
else
{
    app.Run();
}

/// <summary>
/// Holds the result of configuration loading for injection into controllers.
/// </summary>
public record ConfigStatus(AppSettings? Settings, string[] MissingVariables)
{
    public bool IsConfigured => Settings is not null;
}
