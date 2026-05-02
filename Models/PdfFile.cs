using System;
using System.Collections.ObjectModel;

namespace AgentNovel.Models;

public class PdfFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public long FileSize { get; set; }
    public ObservableCollection<PdfPage> Pages { get; set; } = new();
}
