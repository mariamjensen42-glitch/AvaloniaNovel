using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AvaloniaNovel.Services;

public class LLMService : ILLMService
{
    private readonly IDatabaseService _dbService;
    private const string DefaultSystemPrompt = @"你是一个经验丰富的网络小说作家，精通各种网文套路和风格。
你有10年以上的网文创作经验，写过都市、玄幻、悬疑、科幻等多种类型的小说。
你的写作风格：
- 情节紧凑，不拖沓
- 人物刻画鲜明，对话自然
- 善于设置悬念和爽点
- 章节结尾留有钩子，吸引读者继续阅读";

    private const string DeepSeekEndpoint = "https://api.deepseek.com";

    public LLMService(IDatabaseService dbService)
    {
        _dbService = dbService;
    }

    private async Task<IChatClient> BuildChatClientAsync()
    {
        var settings = await _dbService.GetAppSettingsAsync();
        var apiKey = settings?.DeepSeekApiKey ?? string.Empty;

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("请先在设置中配置 DeepSeek API Key");

        var openAIClient = new OpenAIClient(
            new System.ClientModel.ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(DeepSeekEndpoint) });

        return openAIClient.GetChatClient("deepseek-chat").AsIChatClient();
    }

    private static ChatOptions DefaultChatOptions => new()
    {
        MaxOutputTokens = 4096,
        Temperature = 0.8f
    };

    public async Task<string> InvokeAsync(string userPrompt, string? systemPrompt = null)
    {
        var chatClient = await BuildChatClientAsync();
        var chatHistory = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt ?? DefaultSystemPrompt),
            new(ChatRole.User, userPrompt)
        };

        var result = await chatClient.GetResponseAsync(chatHistory, DefaultChatOptions);
        return result.Text ?? string.Empty;
    }

    public async IAsyncEnumerable<string> InvokeStreamAsync(
        string userPrompt,
        string? systemPrompt = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatClient = await BuildChatClientAsync();
        var chatHistory = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt ?? DefaultSystemPrompt),
            new(ChatRole.User, userPrompt)
        };

        await foreach (var update in chatClient.GetStreamingResponseAsync(chatHistory, DefaultChatOptions, cancellationToken))
        {
            var text = update.Text;
            if (!string.IsNullOrEmpty(text))
                yield return text;
        }
    }
}
