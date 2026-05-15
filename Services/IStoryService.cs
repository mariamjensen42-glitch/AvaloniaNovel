using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaNovel.Models;

namespace AvaloniaNovel.Services;

public interface IStoryService
{
    Task<List<Chapter>> GenerateOutlineAsync(
        string genre, string worldSetting,
        string? outlineTemplate = null, string? systemPrompt = null);

    Task<string> WriteChapterAsync(
        string chapterTitle, string chapterSummary,
        string genre, string worldSetting, string previousSummary,
        string? chapterTemplate = null, string? systemPrompt = null);

    Task<string> RewriteChapterAsync(
        string currentContent, string instruction,
        string chapterTitle, string genre, string worldSetting,
        string previousSummary,
        string? rewriteTemplate = null, string? systemPrompt = null);

    Task<string> WriteChapterStreamAsync(
        string chapterTitle, string chapterSummary,
        string genre, string worldSetting, string previousSummary,
        System.Action<string> onChunk,
        CancellationToken cancellationToken = default,
        string? chapterTemplate = null, string? systemPrompt = null);
}
