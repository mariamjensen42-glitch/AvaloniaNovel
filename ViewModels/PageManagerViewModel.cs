using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AgentNovel.Messages;
using AgentNovel.Models;
using AgentNovel.Services;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AgentNovel.ViewModels;

public partial class PageManagerViewModel : ViewModelBase
{
    private readonly PdfService _pdfService = new();
    private readonly ThumbnailService _thumbnailService = new();
    private readonly SettingsService _settingsService = new();
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

        var settings = await _settingsService.LoadAsync();
        var options = new FilePickerOpenOptions
        {
            Title = "选择PDF文件",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } } }
        };
        if (!string.IsNullOrEmpty(settings.LastProjectPath))
        {
            options.SuggestedStartLocation = await StorageProviderHelper.TryGetFolderFromPathAsync(
                storage, settings.LastProjectPath);
        }

        var files = await storage.OpenFilePickerAsync(options);

        if (files.Count > 0)
        {
            try
            {
                var filePath = files[0].Path.LocalPath;
                CurrentFile = await _pdfService.LoadPdfAsync(filePath);
                _sourceFilePath = filePath;

                settings.LastProjectPath = Path.GetDirectoryName(filePath);
                await _settingsService.SaveAsync(settings);

                await GenerateThumbnailsAsync(filePath);
            }
            catch (Exception)
            {
                WeakReferenceMessenger.Default.Send(
                    new NotificationMessage(new NotificationInfo { Message = "文件加载失败", Type = NotificationType.Error }));
            }
        }
    }

    private async Task GenerateThumbnailsAsync(string filePath)
    {
        if (CurrentFile == null) return;

        for (int i = 0; i < CurrentFile.Pages.Count; i++)
        {
            var page = CurrentFile.Pages[i];
            var thumbnail = await _thumbnailService.GenerateThumbnailAsync(filePath, page.PageNumber);
            page.Thumbnail = thumbnail;
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
        if (CurrentFile == null) return;

        foreach (var page in SelectedPages.ToList())
        {
            CurrentFile.Pages.Remove(page);
        }
        SelectedPages.Clear();

        for (int i = 0; i < CurrentFile.Pages.Count; i++)
        {
            CurrentFile.Pages[i].PageNumber = i + 1;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (CurrentFile == null || _sourceFilePath == null) return;

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

        if (file == null) return;

        IsProcessing = true;

        try
        {
            await _pdfService.SavePagesAsync(_sourceFilePath, CurrentFile.Pages.ToList(), file.Path.LocalPath);

            WeakReferenceMessenger.Default.Send(
                new NotificationMessage(new NotificationInfo { Message = "PDF保存成功", Type = NotificationType.Success }));
        }
        catch (Exception)
        {
            WeakReferenceMessenger.Default.Send(
                new NotificationMessage(new NotificationInfo { Message = "保存失败", Type = NotificationType.Error }));
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
