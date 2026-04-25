using AvaloniaNovel.Data;
using Microsoft.EntityFrameworkCore;

namespace AvaloniaNovel.Services;

public static class DatabaseInitializer
{
    public static void Initialize()
    {
        using var db = new NovelDbContext();
        db.Database.Migrate();
    }
}
