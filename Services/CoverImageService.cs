using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AvaloniaNovel.Services;

public class CoverImageService
{
    private static readonly FilePickerFileType ImageFileType = new("图片文件")
    {
        Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.webp", "*.bmp" },
        MimeTypes = new[] { "image/png", "image/jpeg", "image/webp", "image/bmp" }
    };

    public string CoversDirectoryPath => Path.Combine(GetAppDataDirectory(), "Covers");

    public async Task<string?> PickCoverImageAsync(TopLevel topLevel)
    {
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择小说封面",
            AllowMultiple = false,
            FileTypeFilter = new[] { ImageFileType }
        });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    public async Task<string> SaveCoverImageAsync(string sourceFilePath)
    {
        Directory.CreateDirectory(CoversDirectoryPath);

        var extension = Path.GetExtension(sourceFilePath);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".png";
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var targetPath = Path.Combine(CoversDirectoryPath, fileName);

        await using var sourceStream = File.OpenRead(sourceFilePath);
        await using var targetStream = File.Create(targetPath);
        await sourceStream.CopyToAsync(targetStream);

        return targetPath;
    }

    public void DeleteCoverImage(string? coverImagePath)
    {
        if (string.IsNullOrWhiteSpace(coverImagePath) || !File.Exists(coverImagePath))
        {
            return;
        }

        File.Delete(coverImagePath);
    }

    private static string GetAppDataDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AINovelFlow");
    }
}
