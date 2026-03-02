using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using ReleaseNotesScribe.Configuration;
using ReleaseNotesScribe.Models;

namespace ReleaseNotesScribe.Services;

public class AiService
{
    private readonly AppSettings _settings;

    public AiService(AppSettings settings)
    {
        _settings = settings;
    }

    public async Task<string> GenerateReleaseNotesAsync(
        string versionLabel,
        IReadOnlyList<WorkItemInfo> items)
    {
        var credential = new ApiKeyCredential(_settings.AzAiKey);
        var client = new AzureOpenAIClient(_settings.GetAzureOpenAiBaseUri(), credential);
        var chatClient = client.GetChatClient(_settings.AzAiDeployment);

        var messages = new ChatMessage[]
        {
            new SystemChatMessage(PromptTemplate.SystemMessage),
            new UserChatMessage(PromptTemplate.BuildUserMessage(versionLabel, items))
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0.4f,
            MaxOutputTokenCount = 4096
        };

        var completion = await chatClient.CompleteChatAsync(messages, options);
        return completion.Value.Content[0].Text;
    }
}
