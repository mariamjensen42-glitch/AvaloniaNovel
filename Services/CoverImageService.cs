using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AvaloniaNovel.Services;

public class CoverImageService : ICoverImageService
{
    private static readonly FilePickerFileType ImageFileType = new("鍥剧墖鏂囦欢")
    {
        Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.webp", "*.bmp" },
        MimeTypes = new[] { "image/png", "image/jpeg", "image/webp", "image/bmp" }
    };

    public string CoversDirectoryPath => Path.Combine(GetAppDataDirectory(), "Covers");

    public async Task<string?> PickCoverImageAsync(TopLevel topLevel)
    {
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "閫夋嫨灏忚灏侀潰",
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
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(basePath))
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(basePath, "AINovelFlow");
    }
}


