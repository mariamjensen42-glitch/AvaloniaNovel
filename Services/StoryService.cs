using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AvaloniaNovel.Models;

namespace AvaloniaNovel.Services;

public class StoryService
{
    private readonly LLMService _llmService;

    public StoryService()
    {
        _llmService = new LLMService();
    }

    public async Task<List<Chapter>> GenerateOutlineAsync(string genre, string worldSetting)
    {
        var prompt = $@"## 任务
根据以下设定，生成网络小说大纲。

## 输入
- 题材：{genre}
- 世界观：{worldSetting}

## 要求
1. 生成 10-15 章的章节列表
2. 每章需要有 50-100 字的简要描述
3. 确保整体故事有起承转合，高潮迭起
4. 章节标题要吸引人，有网感

## 输出格式
JSON 格式，字段：title（章节标题），summary（章节概要）";

        var response = await _llmService.InvokeAsync(prompt);

        var chapters = ParseChaptersFromJson(response);

        for (int i = 0; i < chapters.Count; i++)
        {
            chapters[i].Order = i + 1;
            chapters[i].Status = ChapterStatus.Outline;
        }

        return chapters;
    }

    public async Task<string> WriteChapterAsync(
        string chapterTitle,
        string chapterSummary,
        string genre,
        string worldSetting,
        string previousSummary)
    {
        var prompt = $@"## 任务
根据以下大纲，写出章节正文。

## 输入
- 章节标题：{chapterTitle}
- 章节概要：{chapterSummary}
- 前文剧情：{previousSummary}
- 题材：{genre}
- 世界观：{worldSetting}

## 要求
1. 字数：2000-5000 字
2. 情节紧凑，避免水文
3. 适当加入对话和心理描写
4. 章节结尾留下悬念，吸引读者
5. 注意起承转合，情节要完整

## 输出
直接输出章节正文，不需要额外格式。";

        return await _llmService.InvokeAsync(prompt);
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
