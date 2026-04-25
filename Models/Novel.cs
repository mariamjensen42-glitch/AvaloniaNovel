using System;
using System.Collections.Generic;

namespace AvaloniaNovel.Models;

public class Novel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string WorldSetting { get; set; } = string.Empty;
    public string CoverImagePath { get; set; } = string.Empty;
    public bool HasCoverImage => !string.IsNullOrWhiteSpace(CoverImagePath);
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Chapter> Chapters { get; set; } = new();
}
