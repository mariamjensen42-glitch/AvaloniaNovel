using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AvaloniaNovel.Services;

public class LLMService
{
    private readonly DatabaseService _dbService;
    private const string SystemPrompt = @"你是一个经验丰富的网络小说作家，精通各种网文套路和风格。
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

    public async Task<string> InvokeAsync(string userPrompt)
    {
        var settings = await _dbService.GetAppSettingsAsync();
        var apiKey = settings?.DeepSeekApiKey ?? string.Empty;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("请先在设置中配置 DeepSeek API Key");
        }

        var httpHandler = new HttpClientHandler();
        var httpClient = new HttpClient(httpHandler)
        {
            Timeout = TimeSpan.FromSeconds(120)
        };

        var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "deepseek-chat",
                apiKey: apiKey,
                httpClient: httpClient,
                endpoint: new Uri(DeepSeekEndpoint));

        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory(SystemPrompt);
        chatHistory.AddUserMessage(userPrompt);

        var result = await chatService.GetChatMessageContentAsync(chatHistory,
            new OpenAIPromptExecutionSettings
            {
                MaxTokens = 4096,
                Temperature = 0.8
            });

        return result.Content?.ToString() ?? string.Empty;
    }
}
