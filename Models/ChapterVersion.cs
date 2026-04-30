using System;

namespace AvaloniaNovel.Models;

public class ChapterVersion
{
    public int Id { get; set; }
    public int ChapterId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public string Trigger { get; set; } = "auto-save"; // auto-save / manual-save / rewrite
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Chapter? Chapter { get; set; }
}