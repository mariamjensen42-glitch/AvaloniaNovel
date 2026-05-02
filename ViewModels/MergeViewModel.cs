using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AgentNovel.Models;
using AgentNovel.Services;
using Avalonia;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgentNovel.ViewModels;

public partial class MergeViewModel : ViewModelBase
{
    private readonly PdfService _pdfService = new();

    [ObservableProperty]
    private ObservableCollection<PdfFile> _pdfFiles = new();

    [ObservableProperty]
    private PdfFile? _selectedFile;

    [ObservableProperty]
    private int _progress;

    [ObservableProperty]
    private bool _isProcessing;

    [RelayCommand]
    private async Task AddFiles()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        var storage = desktop.MainWindow?.StorageProvider;
        if (storage == null) return;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择PDF文件",
            AllowMultiple = true,
            FileTypeFilter = new[] { new FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } } }
        });

        foreach (var file in files)
        {
            try
            {
                var pdfFile = await _pdfService.LoadPdfAsync(file.Path.LocalPath);
                PdfFiles.Add(pdfFile);
            }
            catch (Exception ex)
            {
                // TODO: 显示错误提示
            }
        }
    }

    [RelayCommand]
    private void RemoveFile()
    {
        if (SelectedFile != null)
        {
            PdfFiles.Remove(SelectedFile);
        }
    }

    [RelayCommand]
    private void ClearFiles()
    {
        PdfFiles.Clear();
    }

    [RelayCommand]
    private async Task Merge()
    {
        if (PdfFiles.Count < 2)
            return;

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        var storage = desktop.MainWindow?.StorageProvider;
        if (storage == null) return;

        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "保存合并后的PDF",
            SuggestedFileName = "merged.pdf",
            FileTypeChoices = new[] { new FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } } }
        });

        if (file == null)
            return;

        IsProcessing = true;
        Progress = 0;

        try
        {
            var filePaths = PdfFiles.Select(f => f.FilePath).ToList();
            var progress = new Progress<int>(p => Progress = p);
            await _pdfService.MergePdfsAsync(filePaths, file.Path.LocalPath, progress);
            
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
