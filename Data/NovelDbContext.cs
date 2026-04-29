using Microsoft.EntityFrameworkCore;
using AvaloniaNovel.Models;
using System;
using System.IO;

namespace AvaloniaNovel.Data;

public class NovelDbContext : DbContext
{
    public DbSet<Novel> Novels => Set<Novel>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();
    public DbSet<PromptTemplate> PromptTemplates => Set<PromptTemplate>();
    public DbSet<ChapterVersion> ChapterVersions => Set<ChapterVersion>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AINovelFlow",
            "ainovelflow.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        options.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Novel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Genre).HasMaxLength(50);
            entity.Property(e => e.CoverImagePath).HasMaxLength(500);
            entity.HasMany(e => e.Chapters)
                  .WithOne()
                  .HasForeignKey(c => c.NovelId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeepSeekApiKey).HasMaxLength(500);
        });

        modelBuilder.Entity<PromptTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => e.Type);
        });

        modelBuilder.Entity<ChapterVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Chapter)
                  .WithMany(c => c.Versions)
                  .HasForeignKey(e => e.ChapterId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
