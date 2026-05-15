using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaNovel.Models;

namespace AvaloniaNovel.Services;

public class StoryService : IStoryService
{
    private readonly ILLMService _llmService;

    public StoryService(ILLMService llmService)
    {
        _llmService = llmService;
    }

    // ── 默认内置 Prompt（兼容无模板场景）──────────────────────────────────
    private const string DefaultOutlinePrompt = @"## 任务
根据以下设定，生成网络小说大纲。

## 输入
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 生成 10-15 章的章节列表
2. 每章需要有 50-100 字的简要描述
3. 确保整体故事有起承转合，高潮迭起
4. 章节标题要吸引人，有网感

## 输出格式
JSON 格式，字段：title（章节标题），summary（章节概要）";

    private const string DefaultChapterPrompt = @"## 任务
根据以下大纲，写出章节正文。

## 输入
- 章节标题：{{chapterTitle}}
- 章节概要：{{chapterSummary}}
- 前文剧情：{{previousSummary}}
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 字数：2000-5000 字
2. 情节紧凑，避免水文
3. 适当加入对话和心理描写
4. 章节结尾留下悬念，吸引读者
5. 注意起承转合，情节要完整

## 输出
直接输出章节正文，不需要额外格式。";

    private const string DefaultRewritePrompt = @"## 任务
根据用户的新指令，修改当前章节内容。

## 输入
- 当前章节标题：{{chapterTitle}}
- 当前章节内容：{{currentContent}}
- 用户指令：{{instruction}}
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 严格按照用户指令调整剧情
2. 保持原有章节标题和整体结构
3. 调整后的内容应合理、流畅
4. 字数：2000-5000 字

## 输出
直接输出修改后的章节正文，不需要额外格式。";

    // ── 模板变量替换 ──────────────────────────────────────────────────
    private static string RenderTemplate(string template, Dictionary<string, string> variables)
    {
        var result = template;
        foreach (var (key, value) in variables)
        {
            result = result.Replace($"{{{{{key}}}}}", value);
        }
        return result;
    }

    // ── 大纲生成（支持自定义模板）──────────────────────────────────────
    public async Task<List<Chapter>> GenerateOutlineAsync(
        string genre, string worldSetting,
        string? outlineTemplate = null, string? systemPrompt = null)
    {
        var template = string.IsNullOrWhiteSpace(outlineTemplate)
            ? DefaultOutlinePrompt
            : outlineTemplate;

        var prompt = RenderTemplate(template, new Dictionary<string, string>
        {
            ["genre"] = genre,
            ["worldSetting"] = worldSetting
        });

        var response = await _llmService.InvokeAsync(prompt, systemPrompt);
        var chapters = ParseChaptersFromJson(response);

        for (int i = 0; i < chapters.Count; i++)
        {
            chapters[i].Order = i + 1;
            chapters[i].Status = ChapterStatus.Outline;
        }

        return chapters;
    }

    // ── 章节写作（普通）──────────────────────────────────────────────
    public async Task<string> WriteChapterAsync(
        string chapterTitle, string chapterSummary,
        string genre, string worldSetting, string previousSummary,
        string? chapterTemplate = null, string? systemPrompt = null)
    {
        var prompt = BuildChapterPrompt(chapterTitle, chapterSummary,
            genre, worldSetting, previousSummary, chapterTemplate);
        return await _llmService.InvokeAsync(prompt, systemPrompt);
    }

    // ── 章节重写 ──────────────────────────────────────────────────────
    public async Task<string> RewriteChapterAsync(
        string currentContent,
        string instruction,
        string chapterTitle,
        string genre, string worldSetting,
        string previousSummary = "",
        string? rewriteTemplate = null, string? systemPrompt = null)
    {
        var template = string.IsNullOrWhiteSpace(rewriteTemplate)
            ? DefaultRewritePrompt
            : rewriteTemplate;

        var prompt = RenderTemplate(template, new Dictionary<string, string>
        {
            ["chapterTitle"] = chapterTitle,
            ["currentContent"] = currentContent,
            ["instruction"] = instruction,
            ["previousSummary"] = previousSummary,
            ["genre"] = genre,
            ["worldSetting"] = worldSetting
        });

        return await _llmService.InvokeAsync(prompt, systemPrompt);
    }

    // ── 章节写作（流式）──────────────────────────────────────────────
    /// <summary>
    /// 流式写章节：每收到一个 token 片段就调用 <paramref name="onChunk"/>，
    /// 最终返回完整正文。可通过 <paramref name="cancellationToken"/> 取消。
    /// </summary>
    public async Task<string> WriteChapterStreamAsync(
        string chapterTitle, string chapterSummary,
        string genre, string worldSetting, string previousSummary,
        Action<string> onChunk,
        CancellationToken cancellationToken = default,
        string? chapterTemplate = null, string? systemPrompt = null)
    {
        var prompt = BuildChapterPrompt(chapterTitle, chapterSummary,
            genre, worldSetting, previousSummary, chapterTemplate);
        var fullContent = new System.Text.StringBuilder();

        await foreach (var chunk in _llmService.InvokeStreamAsync(prompt, systemPrompt, cancellationToken))
        {
            fullContent.Append(chunk);
            onChunk(chunk);
        }

        return fullContent.ToString();
    }

    // ── 共用 Prompt 构造 ──────────────────────────────────────────────
    private static string BuildChapterPrompt(
        string chapterTitle, string chapterSummary,
        string genre, string worldSetting, string previousSummary,
        string? chapterTemplate = null)
    {
        var template = string.IsNullOrWhiteSpace(chapterTemplate)
            ? DefaultChapterPrompt
            : chapterTemplate;

        return RenderTemplate(template, new Dictionary<string, string>
        {
            ["chapterTitle"] = chapterTitle,
            ["chapterSummary"] = chapterSummary,
            ["previousSummary"] = previousSummary,
            ["genre"] = genre,
            ["worldSetting"] = worldSetting
        });
    }

    private List<Chapter> ParseChaptersFromJson(string json)
    {
        var chapters = new List<Chapter>();

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("chapters", out var chaptersArray))
            {
                foreach (var item in chaptersArray.EnumerateArray())
                {
                    var chapter = new Chapter
                    {
                        Title = item.GetProperty("title").GetString() ?? string.Empty,
                        Summary = item.GetProperty("summary").GetString() ?? string.Empty
                    };
                    chapters.Add(chapter);
                }
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    var chapter = new Chapter
                    {
                        Title = item.GetProperty("title").GetString() ?? string.Empty,
                        Summary = item.GetProperty("summary").GetString() ?? string.Empty
                    };
                    chapters.Add(chapter);
                }
            }
        }
        catch
        {
            var lines = json.Split('\n');
            Chapter? current = null;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Contains("title") || trimmed.Contains("章节"))
                {
                    if (current != null)
                        chapters.Add(current);
                    current = new Chapter();
                }
                if (current != null && trimmed.Contains("title"))
                {
                    var parts = trimmed.Split(':');
                    if (parts.Length > 1)
                        current.Title = parts[1].Trim().Trim('"', ',');
                }
                if (current != null && trimmed.Contains("summary"))
                {
                    var parts = trimmed.Split(':');
                    if (parts.Length > 1)
                        current.Summary = parts[1].Trim().Trim('"', ',');
                }
            }
            if (current != null)
                chapters.Add(current);
        }

        return chapters;
    }
}
