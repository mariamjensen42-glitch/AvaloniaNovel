using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaNovel.Models;
using AvaloniaNovel.Services;

namespace AvaloniaNovel.ViewModels;

public partial class CreateViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly ExportService _exportService;

    [ObservableProperty]
    private Novel? _currentNovel;

    [ObservableProperty]
    private ObservableCollection<Chapter> _chapters = new();

    [ObservableProperty]
    private Chapter? _selectedChapter;

    [ObservableProperty]
    private bool _isWriting;

    [ObservableProperty]
    private bool _hasOutline;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public CreateViewModel()
    {
        _dbService = new DatabaseService();
        _exportService = new ExportService();
    }

    public async Task LoadNovelAsync(Novel novel)
    {
        CurrentNovel = novel;
        var freshNovel = await _dbService.GetNovelByIdAsync(novel.Id);
        if (freshNovel != null)
        {
            Chapters = new ObservableCollection<Chapter>(freshNovel.Chapters.OrderBy(c => c.Order));
            HasOutline = Chapters.Count > 0;
            SelectedChapter = Chapters.FirstOrDefault();
        }
    }

    [RelayCommand]
    private async Task GenerateOutline()
    {
        if (CurrentNovel == null)
            return;

        IsWriting = true;
        StatusMessage = "正在生成大纲...";

        try
        {
            var storyService = new StoryService();
            var chapters = await storyService.GenerateOutlineAsync(
                CurrentNovel.Genre,
                CurrentNovel.WorldSetting);

            foreach (var chapter in chapters)
            {
                chapter.NovelId = CurrentNovel.Id;
                chapter.CreatedAt = DateTime.Now;
                var savedChapter = await _dbService.AddChapterAsync(chapter);
                Chapters.Add(savedChapter);
            }

            HasOutline = true;
            StatusMessage = $"大纲生成完成，共 {chapters.Count} 章";
        }
        catch (Exception ex)
        {
            StatusMessage = $"生成失败: {ex.Message}";
        }
        finally
        {
            IsWriting = false;
        }
    }

    [RelayCommand]
    private async Task StartWriting()
    {
        if (CurrentNovel == null || Chapters.Count == 0)
            return;

        IsWriting = true;
        StatusMessage = "开始写作...";

        try
        {
            var storyService = new StoryService();
            var firstIncomplete = Chapters.FirstOrDefault(c => c.Status == ChapterStatus.Outline);

            if (firstIncomplete == null)
            {
                StatusMessage = "所有章节已完成";
                return;
            }

            SelectedChapter = firstIncomplete;
            await WriteChapterAsync(firstIncomplete, storyService);

            StatusMessage = "写作完成";
        }
        catch (Exception ex)
        {
            StatusMessage = $"写作失败: {ex.Message}";
        }
        finally
        {
            IsWriting = false;
        }
    }

    private async Task WriteChapterAsync(Chapter chapter, StoryService storyService)
    {
        chapter.Status = ChapterStatus.Writing;
        await _dbService.UpdateChapterAsync(chapter);
        var index = Chapters.IndexOf(chapter);
        if (index >= 0)
        {
            Chapters[index] = chapter;
        }

        StatusMessage = $"正在写作: {chapter.Title}";

        var previousSummary = Chapters.Take(index).LastOrDefault()?.Summary ?? string.Empty;
        var content = await storyService.WriteChapterAsync(
            chapter.Title,
            chapter.Summary,
            CurrentNovel!.Genre,
            CurrentNovel.WorldSetting,
            previousSummary);

        chapter.Content = content;
        chapter.Status = ChapterStatus.Completed;
        await _dbService.UpdateChapterAsync(chapter);

        if (index >= 0)
        {
            Chapters[index] = chapter;
        }
        await _dbService.UpdateNovelTimestampAsync(CurrentNovel.Id);
    }

    [RelayCommand]
    private void StopWriting()
    {
        IsWriting = false;
        StatusMessage = "已停止";
    }

    [RelayCommand]
    private async Task Export()
    {
        if (CurrentNovel == null || Chapters.Count == 0)
        {
            StatusMessage = "没有可导出的内容";
            return;
        }

        try
        {
            var completedChapters = Chapters
                .Where(c => !string.IsNullOrEmpty(c.Content))
                .Select(c => (c.Title, c.Content))
                .ToList();

            if (completedChapters.Count == 0)
            {
                StatusMessage = "没有已完成的章节可导出";
                return;
            }

            var content = _exportService.GenerateTxtContent(CurrentNovel.Title, completedChapters);

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var topLevel = desktop.MainWindow;
                if (topLevel != null)
                {
                    await _exportService.ExportToTxtAsync(topLevel, CurrentNovel.Title, content);
                    StatusMessage = "导出成功";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"导出失败: {ex.Message}";
        }
    }
}
