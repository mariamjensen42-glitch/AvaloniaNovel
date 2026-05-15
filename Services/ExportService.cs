using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AvaloniaNovel.Services;

public class ExportService : IExportService
{
    public async Task ExportToTxtAsync(TopLevel topLevel, string title, string content)
    {
        if (topLevel == null)
            return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "瀵煎嚭灏忚",
            SuggestedFileName = $"{title}.txt",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("鏂囨湰鏂囦欢")
                {
                    Patterns = new[] { "*.txt" }
                }
            }
        });

        if (file != null)
        {
            await File.WriteAllTextAsync(file.Path.LocalPath, content);
        }
    }

    public string GenerateTxtContent(string title, IEnumerable<(string ChapterTitle, string Content)> chapters)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(title);
        sb.AppendLine(new string('=', 50));
        sb.AppendLine();

        foreach (var (chapterTitle, content) in chapters)
        {
            sb.AppendLine(chapterTitle);
            sb.AppendLine(new string('-', 30));
            sb.AppendLine();
            sb.AppendLine(content);
            sb.AppendLine();
            sb.AppendLine(new string('=', 50));
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

