using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AvaloniaNovel.Services;

public class LLMService
{
    private readonly DatabaseService _dbService;
    private const string DefaultSystemPrompt = @"你是一个经验丰富的网络小说作家，精通各种网文套路和风格。
你有10年以上的网文创作经验，写过都市、玄幻、悬疑、科幻等多种类型的小说。
你的写作风格：
- 情节紧凑，不拖沓
- 人物刻画鲜明，对话自然
- 善于设置悬念和爽点
- 章节结尾留有钩子，吸引读者继续阅读";

    private const string DeepSeekEndpoint = "https://api.deepseek.com";

    public LLMService()
    {
        _dbService = new DatabaseService();
    }

    // ── 构建共用的 ChatService ──────────────────────────────────────────
    private async Task<IChatCompletionService> BuildChatServiceAsync()
    {
        var settings = await _dbService.GetAppSettingsAsync();
        var apiKey = settings?.DeepSeekApiKey ?? string.Empty;

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("请先在设置中配置 DeepSeek API Key");

        var httpClient = new HttpClient(new HttpClientHandler())
        {
            Timeout = TimeSpan.FromSeconds(180)
        };

        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "deepseek-chat",
                apiKey: apiKey,
                httpClient: httpClient,
                endpoint: new Uri(DeepSeekEndpoint))
            .Build();

        return kernel.GetRequiredService<IChatCompletionService>();
    }

    private static OpenAIPromptExecutionSettings DefaultExecutionSettings => new()
    {
        MaxTokens = 4096,
        Temperature = 0.8
    };

    // ── 普通调用（一次性返回完整结果）──────────────────────────────────
    public async Task<string> InvokeAsync(string userPrompt, string? systemPrompt = null)
    {
        var chatService = await BuildChatServiceAsync();
        var chatHistory = new ChatHistory(systemPrompt ?? DefaultSystemPrompt);
        chatHistory.AddUserMessage(userPrompt);

        var result = await chatService.GetChatMessageContentAsync(
            chatHistory, DefaultExecutionSettings);

        return result.Content ?? string.Empty;
    }

    // ── 流式调用：通过 IAsyncEnumerable 逐 token 推送 ──────────────────
    /// <summary>
    /// 以流式方式调用 LLM，通过 IAsyncEnumerable 逐个推送 token 片段。
    /// 调用方可用 <c>await foreach</c> 实时消费，并通过 <paramref name="cancellationToken"/> 取消。
    /// </summary>
    public async IAsyncEnumerable<string> InvokeStreamAsync(
        string userPrompt,
        string? systemPrompt = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatService = await BuildChatServiceAsync();
        var chatHistory = new ChatHistory(systemPrompt ?? DefaultSystemPrompt);
        chatHistory.AddUserMessage(userPrompt);

        await foreach (var chunk in chatService
            .GetStreamingChatMessageContentsAsync(chatHistory, DefaultExecutionSettings)
            .WithCancellation(cancellationToken))
        {
            var text = chunk.Content;
            if (!string.IsNullOrEmpty(text))
                yield return text;
        }
    }
}
