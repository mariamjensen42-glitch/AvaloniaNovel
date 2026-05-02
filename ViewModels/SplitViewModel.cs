using System;
using System.IO;
using System.Threading.Tasks;
using AgentNovel.Models;
using AgentNovel.Services;
using Avalonia;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgentNovel.ViewModels;

public partial class SplitViewModel : ViewModelBase
{
    private readonly PdfService _pdfService = new();

    [ObservableProperty]
    private PdfFile? _currentFile;

    [ObservableProperty]
    private string _pageRange = string.Empty;

    [ObservableProperty]
    private bool _extractMode = true;

    [ObservableProperty]
    private int _progress;

    [ObservableProperty]
    private bool _isProcessing;

    [RelayCommand]
    private async Task SelectFile()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        var storage = desktop.MainWindow?.StorageProvider;
        if (storage == null) return;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择PDF文件",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } } }
        });

        if (files.Count > 0)
        {
            try
            {
                CurrentFile = await _pdfService.LoadPdfAsync(files[0].Path.LocalPath);
                PageRange = $"1-{CurrentFile.PageCount}";
            }
            catch (Exception ex)
            {
                // TODO: 显示错误提示
            }
        }
    }

    [RelayCommand]
    private async Task Split()
    {
        if (CurrentFile == null || string.IsNullOrWhiteSpace(PageRange))
            return;

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        var storage = desktop.MainWindow?.StorageProvider;
        if (storage == null) return;

        var folder = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "选择保存位置",
            AllowMultiple = false
        });

        if (folder.Count == 0)
            return;

        IsProcessing = true;
        Progress = 0;

        try
        {
            var pageNumbers = Helpers.PageRangeParser.Parse(PageRange, CurrentFile.PageCount);
            if (pageNumbers.Count == 0)
            {
                // TODO: 显示错误提示
                return;
            }

            var outputPath = Path.Combine(folder[0].Path.LocalPath, $"{Path.GetFileNameWithoutExtension(CurrentFile.FileName)}_split.pdf");
            await _pdfService.SplitPdfAsync(CurrentFile.FilePath, pageNumbers, outputPath);
            
            // TODO: 显示成功提示
        }
        catch (Exception ex)
        {
            // TODO: 显示错误提示
        }
        finally
        {
            IsProcessing = false;
            Progress = 0;
        }
    }
}
