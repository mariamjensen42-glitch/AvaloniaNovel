using System.Collections.Generic;
using System.Threading.Tasks;
using AvaloniaNovel.Models;

namespace AvaloniaNovel.Services;

public interface IDatabaseService
{
    Task<List<Novel>> GetAllNovelsAsync();
    Task<Novel?> GetNovelByIdAsync(int id);
    Task<Novel> CreateNovelAsync(string title, string genre, string worldSetting, string? coverImageSourcePath = null);
    Task DeleteNovelAsync(int id);
    Task<Chapter> AddChapterAsync(Chapter chapter);
    Task UpdateChapterAsync(Chapter chapter);
    Task<Chapter?> GetChapterByIdAsync(int id);
    Task<AppSettings?> GetAppSettingsAsync();
    Task SaveAppSettingsAsync(string apiKey);
    Task UpdateNovelTimestampAsync(int novelId);
    Task<List<PromptTemplate>> GetAllPromptTemplatesAsync();
    Task<List<PromptTemplate>> GetPromptTemplatesByTypeAsync(PromptTemplateType type);
    Task<PromptTemplate?> GetPromptTemplateByIdAsync(int id);
    Task<PromptTemplate> CreatePromptTemplateAsync(PromptTemplate template);
    Task UpdatePromptTemplateAsync(PromptTemplate template);
    Task DeletePromptTemplateAsync(int id);
    Task<ChapterVersion> AddVersionAsync(ChapterVersion version);
    Task<List<ChapterVersion>> GetVersionsAsync(int chapterId);
    Task DeleteOldVersionsAsync(int chapterId);
}
