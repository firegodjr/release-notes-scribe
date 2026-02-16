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
