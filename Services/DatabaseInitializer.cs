using System;
using System.Linq;
using AvaloniaNovel.Data;
using AvaloniaNovel.Services;
using Microsoft.EntityFrameworkCore;

namespace AvaloniaNovel.Services;

public static class DatabaseInitializer
{
    public static void Initialize()
    {
        using var db = new NovelDbContext();
        db.Database.Migrate();

        // 确保默认模板存在（同步执行，避免 UI 线程死锁）
        EnsureDefaultTemplatesSync(db);
    }

    private static void EnsureDefaultTemplatesSync(NovelDbContext db)
    {
        var existingNames = db.PromptTemplates
            .Where(t => t.IsBuiltIn)
            .Select(t => t.Name)
            .ToHashSet();

        var now = DateTime.Now;
        var defaults = DatabaseService.GetBuiltInTemplates(now);
        var newTemplates = defaults.Where(d => !existingNames.Contains(d.Name)).ToList();

        if (newTemplates.Count == 0)
            return;

        db.PromptTemplates.AddRange(newTemplates);
        db.SaveChanges();
    }
}
