using System;
using System.Linq;
using AvaloniaNovel.Data;
using AvaloniaNovel.Models;
using AvaloniaNovel.Services;
using Microsoft.EntityFrameworkCore;

namespace AvaloniaNovel.Services;

public static class DatabaseInitializer
{
    public static void Initialize()
    {
        using var db = new NovelDbContext();
        db.Database.Migrate();

        DeduplicateBuiltInTemplates(db);
        EnsureDefaultTemplatesSync(db);
    }

    private static void DeduplicateBuiltInTemplates(NovelDbContext db)
    {
        var builtIn = db.PromptTemplates.Where(t => t.IsBuiltIn).ToList();
        if (builtIn.Count == 0)
            return;

        var toRemove = builtIn
            .GroupBy(t => new { t.Name, t.Type })
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.OrderBy(x => x.Id).Skip(1))
            .ToList();

        if (toRemove.Count == 0)
            return;

        db.PromptTemplates.RemoveRange(toRemove);
        db.SaveChanges();
    }

    private static void EnsureDefaultTemplatesSync(NovelDbContext db)
    {
        var existing = db.PromptTemplates
            .Where(t => t.IsBuiltIn)
            .Select(t => new { t.Name, t.Type })
            .ToHashSet();

        var now = DateTime.Now;
        var defaults = DatabaseService.GetBuiltInTemplates(now);
        var newTemplates = defaults
            .Where(d => !existing.Contains(new { d.Name, d.Type }))
            .ToList();

        if (newTemplates.Count == 0)
            return;

        db.PromptTemplates.AddRange(newTemplates);
        db.SaveChanges();
    }
}
