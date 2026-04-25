using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaNovel.Data;
using AvaloniaNovel.Models;
using Microsoft.EntityFrameworkCore;

namespace AvaloniaNovel.Services;

public class DatabaseService
{
    private readonly CoverImageService _coverImageService = new();

    public async Task<List<Novel>> GetAllNovelsAsync()
    {
        using var db = new NovelDbContext();
        return await db.Novels
            .Include(n => n.Chapters)
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Novel?> GetNovelByIdAsync(int id)
    {
        using var db = new NovelDbContext();
        return await db.Novels
            .Include(n => n.Chapters.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Novel> CreateNovelAsync(string title, string genre, string worldSetting, string? coverImageSourcePath = null)
    {
        using var db = new NovelDbContext();
        var coverImagePath = string.IsNullOrWhiteSpace(coverImageSourcePath)
            ? string.Empty
            : await _coverImageService.SaveCoverImageAsync(coverImageSourcePath);

        var novel = new Novel
        {
            Title = title,
            Genre = genre,
            WorldSetting = worldSetting,
            CoverImagePath = coverImagePath,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        db.Novels.Add(novel);
        await db.SaveChangesAsync();
        return novel;
    }

    public async Task DeleteNovelAsync(int id)
    {
        using var db = new NovelDbContext();
        var novel = await db.Novels.FindAsync(id);
        if (novel != null)
        {
            _coverImageService.DeleteCoverImage(novel.CoverImagePath);
            db.Novels.Remove(novel);
            await db.SaveChangesAsync();
        }
    }

    public async Task<Chapter> AddChapterAsync(Chapter chapter)
    {
        using var db = new NovelDbContext();
        db.Chapters.Add(chapter);
        await db.SaveChangesAsync();
        return chapter;
    }

    public async Task UpdateChapterAsync(Chapter chapter)
    {
        using var db = new NovelDbContext();
        var existing = await db.Chapters.FindAsync(chapter.Id);
        if (existing != null)
        {
            existing.Title = chapter.Title;
            existing.Summary = chapter.Summary;
            existing.Content = chapter.Content;
            existing.Status = chapter.Status;
            existing.Order = chapter.Order;
            await db.SaveChangesAsync();
        }
    }

    public async Task<Chapter?> GetChapterByIdAsync(int id)
    {
        using var db = new NovelDbContext();
        return await db.Chapters.FindAsync(id);
    }

    public async Task<AppSettings?> GetAppSettingsAsync()
    {
        using var db = new NovelDbContext();
        return await db.AppSettings.FirstOrDefaultAsync();
    }

    public async Task SaveAppSettingsAsync(string apiKey)
    {
        using var db = new NovelDbContext();
        var settings = await db.AppSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new AppSettings { DeepSeekApiKey = apiKey };
            db.AppSettings.Add(settings);
        }
        else
        {
            settings.DeepSeekApiKey = apiKey;
        }
        await db.SaveChangesAsync();
    }

    public async Task UpdateNovelTimestampAsync(int novelId)
    {
        using var db = new NovelDbContext();
        var novel = await db.Novels.FindAsync(novelId);
        if (novel != null)
        {
            novel.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
        }
    }
}
