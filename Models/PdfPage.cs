using System;
using Avalonia.Media.Imaging;

namespace AgentNovel.Models;

public class PdfPage
{
    public int PageNumber { get; set; }
    public int Rotation { get; set; } = 0;
    public Bitmap? Thumbnail { get; set; }
    public bool IsSelected { get; set; }
}
