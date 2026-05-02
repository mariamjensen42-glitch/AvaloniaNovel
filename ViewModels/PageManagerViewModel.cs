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

public partial class PageManagerViewModel : ViewModelBase
{
    private readonly PdfService _pdfService = new();
    private string? _sourceFilePath;

    [ObservableProperty]
    private PdfFile? _currentFile;

    [ObservableProperty]
    private ObservableCollection<PdfPage> _selectedPages = new();

    [ObservableProperty]
    private bool _isProcessing;

    [RelayCommand]
    private async Task OpenFile()
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
                _sourceFilePath = files[0].Path.LocalPath;
            }
            catch (Exception ex)
            {
                // TODO: 显示错误提示
            }
        }
    }

    [RelayCommand]
    private void RotateLeft()
    {
        foreach (var page in SelectedPages)
        {
            page.Rotation = (page.Rotation - 90 + 360) % 360;
        }
    }

    [RelayCommand]
    private void RotateRight()
    {
        foreach (var page in SelectedPages)
        {
            page.Rotation = (page.Rotation + 90) % 360;
        }
    }

    [RelayCommand]
    private void Delete()
    {
        if (CurrentFile == null)
            return;

        foreach (var page in SelectedPages.ToList())
        {
            CurrentFile.Pages.Remove(page);
        }
        SelectedPages.Clear();

        // 重新编号
        for (int i = 0; i < CurrentFile.Pages.Count; i++)
        {
            CurrentFile.Pages[i].PageNumber = i + 1;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (CurrentFile == null || _sourceFilePath == null)
            return;

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        var storage = desktop.MainWindow?.StorageProvider;
        if (storage == null) return;

        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "保存PDF文件",
            SuggestedFileName = Path.GetFileNameWithoutExtension(CurrentFile.FileName) + "_modified.pdf",
            FileTypeChoices = new[] { new FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } } }
        });

        if (file == null)
            return;

        IsProcessing = true;

        try
        {
            var newOrder = CurrentFile.Pages.Select(p => p.PageNumber).ToList();
            await _pdfService.ReorderPagesAsync(_sourceFilePath, newOrder, file.Path.LocalPath);
            
            // TODO: 显示成功提示
        }
        catch (Exception ex)
        {
            // TODO: 显示错误提示
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
