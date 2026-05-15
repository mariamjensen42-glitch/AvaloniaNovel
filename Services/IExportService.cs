using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace AvaloniaNovel.Services;

public interface IExportService
{
    Task ExportToTxtAsync(TopLevel topLevel, string title, string content);
    string GenerateTxtContent(string title, IEnumerable<(string ChapterTitle, string Content)> chapters);
}
