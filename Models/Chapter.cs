using System;

namespace AvaloniaNovel.Models;

public class Chapter
{
    public int Id { get; set; }
    public int NovelId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public ChapterStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}