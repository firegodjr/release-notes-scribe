namespace ReleaseNotesScribe.Configuration;

public class AppSettings
{
    public string AdoOrg { get; }
    public string AdoProject { get; }
    public string AdoPat { get; }
    public string AzAiEndpoint { get; }
    public string AzAiDeployment { get; }
    public string AzAiKey { get; }

    private AppSettings(
        string adoOrg, string adoProject, string adoPat,
        string azAiEndpoint, string azAiDeployment, string azAiKey)
    {
        AdoOrg = adoOrg;
        AdoProject = adoProject;
        AdoPat = adoPat;
        AzAiEndpoint = azAiEndpoint;
        AzAiDeployment = azAiDeployment;
        AzAiKey = azAiKey;
    }

    public static readonly string[] RequiredVariableNames =
        ["ADO_ORG", "ADO_PROJECT", "ADO_PAT", "AZ_AI_ENDPOINT", "AZ_AI_DEPLOYMENT", "AZ_AI_KEY"];

    public static readonly string[] SecretVariableNames = ["ADO_PAT", "AZ_AI_KEY"];

    public static AppSettings Load()
    {
        // Load .env from project root (two levels up from bin/)
        var envPath = FindEnvFile();
        if (envPath is not null)
            DotNetEnv.Env.Load(envPath);

        var adoOrg = GetRequired("ADO_ORG");
        var adoProject = GetRequired("ADO_PROJECT");
        var adoPat = GetRequired("ADO_PAT");
        var azAiEndpoint = GetRequired("AZ_AI_ENDPOINT");
        var azAiDeployment = GetRequired("AZ_AI_DEPLOYMENT");
        var azAiKey = GetRequired("AZ_AI_KEY");

        return new AppSettings(adoOrg, adoProject, adoPat, azAiEndpoint, azAiDeployment, azAiKey);
    }

    /// <summary>
    /// Attempts to load configuration without throwing on missing variables.
    /// Returns the settings (if all present) and a list of missing variable names.
    /// </summary>
    public static (AppSettings? Settings, string[] MissingVariables) TryLoad()
    {
        var envPath = FindEnvFile();
        if (envPath is not null)
            DotNetEnv.Env.Load(envPath);

        var missing = RequiredVariableNames
            .Where(name => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)))
            .ToArray();

        if (missing.Length > 0)
            return (null, missing);

        var settings = new AppSettings(
            Environment.GetEnvironmentVariable("ADO_ORG")!,
            Environment.GetEnvironmentVariable("ADO_PROJECT")!,
            Environment.GetEnvironmentVariable("ADO_PAT")!,
            Environment.GetEnvironmentVariable("AZ_AI_ENDPOINT")!,
            Environment.GetEnvironmentVariable("AZ_AI_DEPLOYMENT")!,
            Environment.GetEnvironmentVariable("AZ_AI_KEY")!);

        return (settings, []);
    }

    private static string GetRequired(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Missing required environment variable: {name}");
        return value;
    }

    private static string? FindEnvFile()
    {
        // Walk up from current directory looking for .env
        var dir = Directory.GetCurrentDirectory();
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, ".env");
            if (File.Exists(candidate))
                return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        return null;
    }

    /// <summary>
    /// Returns the base URI for Azure OpenAI (strips any path after the host).
    /// The SDK appends /openai/deployments/... automatically.
    /// </summary>
    public Uri GetAzureOpenAiBaseUri()
    {
        var uri = new Uri(AzAiEndpoint);
        return new Uri($"{uri.Scheme}://{uri.Host}");
    }
}
