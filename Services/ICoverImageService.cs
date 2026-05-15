using System.Threading.Tasks;
using Avalonia.Controls;

namespace AvaloniaNovel.Services;

public interface ICoverImageService
{
    string CoversDirectoryPath { get; }
    Task<string?> PickCoverImageAsync(TopLevel topLevel);
    Task<string> SaveCoverImageAsync(string sourceFilePath);
    void DeleteCoverImage(string? coverImagePath);
}
