using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaNovel.Models;
using AvaloniaNovel.Services;
using LiveMarkdown.Avalonia;

namespace AvaloniaNovel.ViewModels;

public partial class CreateViewModel : ViewModelBase
{
    private readonly IDatabaseService _dbService;
    private readonly IExportService _exportService;
    private readonly IStoryService _storyService = null!;

    // 用于取消正在进行的流式写作
    private CancellationTokenSource? _writingCts;

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

    /// <summary>当前章节已生成的字数（流式写作时实时更新）</summary>
    [ObservableProperty]
    private int _currentWordCount;

    /// <summary>是否处于流式输出状态（用于切换 View 显示）</summary>
    [ObservableProperty]
    private bool _isStreaming;

    /// <summary>
    /// 流式输出的 ObservableStringBuilder，绑定到 MarkdownRenderer.MarkdownBuilder。
    /// Append 时 MarkdownRenderer 自动重新解析并渲染，实现实时 Markdown 渲染。
    /// </summary>
    public ObservableStringBuilder StreamingBuilder { get; } = new();

    /// <summary>
    /// 非流式查看时，直接设置 Markdown 字符串内容。
    /// </summary>
    [ObservableProperty]
    private string _chapterMarkdown = string.Empty;

    /// <summary>
    /// 流式内容更新时触发，View 监听此事件实现自动滚动。
    /// </summary>
    public event Action? StreamingContentUpdated;

    // ── 模板选择 ─────────────────────────────────────────────────────
    [ObservableProperty]
    private ObservableCollection<PromptTemplate> _systemTemplates = new();

    [ObservableProperty]
    private ObservableCollection<PromptTemplate> _outlineTemplates = new();

    [ObservableProperty]
    private ObservableCollection<PromptTemplate> _chapterTemplates = new();

    [ObservableProperty]
    private PromptTemplate? _selectedSystemTemplate;

    [ObservableProperty]
    private PromptTemplate? _selectedOutlineTemplate;

    [ObservableProperty]
    private PromptTemplate? _selectedChapterTemplate;

    /// <summary>是否显示模板选择面板</summary>
    [ObservableProperty]
    private bool _showTemplatePanel;

    // ── 版本历史 ─────────────────────────────────────────────────────
    [ObservableProperty]
    private ObservableCollection<ChapterVersion> _versions = new();

    // ── 自动保存 ─────────────────────────────────────────────────────
    private DispatcherTimer? _autoSaveTimer;
    private int _wordsSinceLastSave = 0;
    private const int AutoSaveWordThreshold = 500; // 每 500 字自动保存
    private const int AutoSaveIntervalMs = 60000;   // 每 60 秒自动保存

    [ObservableProperty]
    private bool _showVersionPanel;

    [ObservableProperty]
    private string _instructionText = string.Empty;

    public CreateViewModel(IDatabaseService dbService, IExportService exportService, IStoryService storyService)
    {
        _dbService = dbService;
        _exportService = exportService;
        _storyService = storyService;
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

        // 加载模板
        await LoadTemplatesAsync();
    }

    private async Task LoadTemplatesAsync()
    {
        var systemList = await _dbService.GetPromptTemplatesByTypeAsync(PromptTemplateType.System);
        SystemTemplates = new ObservableCollection<PromptTemplate>(systemList);
        SelectedSystemTemplate = systemList.FirstOrDefault(t => t.IsBuiltIn) ?? systemList.FirstOrDefault();

        var outlineList = await _dbService.GetPromptTemplatesByTypeAsync(PromptTemplateType.Outline);
        OutlineTemplates = new ObservableCollection<PromptTemplate>(outlineList);
        SelectedOutlineTemplate = outlineList.FirstOrDefault(t => t.IsBuiltIn) ?? outlineList.FirstOrDefault();

        var chapterList = await _dbService.GetPromptTemplatesByTypeAsync(PromptTemplateType.Chapter);
        ChapterTemplates = new ObservableCollection<PromptTemplate>(chapterList);
        SelectedChapterTemplate = chapterList.FirstOrDefault(t => t.IsBuiltIn) ?? chapterList.FirstOrDefault();
    }

    // 当 SelectedChapter 变化时，更新非流式的 Markdown 显示内容
    partial void OnSelectedChapterChanged(Chapter? value)
    {
        ChapterMarkdown = value?.Content ?? string.Empty;
        if (value != null)
            _ = LoadVersionsAsync(value.Id);
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
            var storyService = _storyService;
            var chapters = await storyService.GenerateOutlineAsync(
                CurrentNovel.Genre,
                CurrentNovel.WorldSetting,
                outlineTemplate: SelectedOutlineTemplate?.Content,
                systemPrompt: SelectedSystemTemplate?.Content);

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

        // 创建新的取消令牌
        _writingCts?.Cancel();
        _writingCts = new CancellationTokenSource();

        IsWriting = true;
        StatusMessage = "开始写作...";

        try
        {
            var storyService = _storyService;
            var firstIncomplete = Chapters.FirstOrDefault(c => c.Status == ChapterStatus.Outline);

            if (firstIncomplete == null)
            {
                StatusMessage = "所有章节已完成";
                return;
            }

            SelectedChapter = firstIncomplete;
            await WriteChapterStreamingAsync(firstIncomplete, storyService, _writingCts.Token);

            StatusMessage = "写作完成";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "已停止写作";
        }
        catch (Exception ex)
        {
            StatusMessage = $"写作失败: {ex.Message}";
        }
        finally
        {
            IsWriting = false;
            IsStreaming = false;
        }
    }

    /// <summary>
    /// 使用流式 API 写作一章：每收到 token 即刷新 MarkdownRenderer，完成后保存数据库。
    /// </summary>
    private async Task WriteChapterStreamingAsync(
        Chapter chapter, IStoryService storyService, CancellationToken ct)
    {
        var index = Chapters.IndexOf(chapter);

        // 更新状态为写作中
        chapter.Status = ChapterStatus.Writing;
        await _dbService.UpdateChapterAsync(chapter);
        RefreshChapterInList(chapter, index);

        StatusMessage = $"正在写作: {chapter.Title}";

        // 重置流式缓冲
        StreamingBuilder.Clear();
        CurrentWordCount = 0;
        IsStreaming = true;
        _wordsSinceLastSave = 0;
        StartAutoSaveTimer();

        var previousSummary = index > 0 ? Chapters[index - 1].Summary : string.Empty;

        // ── 流式回调：在 UI 线程追加到 ObservableStringBuilder ──────────
        void OnChunk(string chunk)
        {
            Dispatcher.UIThread.Post(() =>
            {
                StreamingBuilder.Append(chunk);
                CurrentWordCount += chunk.Length;
                System.Threading.Interlocked.Add(ref _wordsSinceLastSave, chunk.Length);
                if (System.Threading.Volatile.Read(ref _wordsSinceLastSave) >= AutoSaveWordThreshold)
                {
                    _ = SaveVersionAsync("auto-save");
                    _wordsSinceLastSave = 0;
                }
                StatusMessage = $"正在写作: {chapter.Title}（{CurrentWordCount} 字）";
                StreamingContentUpdated?.Invoke();
            });
        }

        var fullContent = await storyService.WriteChapterStreamAsync(
            chapter.Title,
            chapter.Summary,
            CurrentNovel!.Genre,
            CurrentNovel.WorldSetting,
            previousSummary,
            OnChunk,
            ct,
            chapterTemplate: SelectedChapterTemplate?.Content,
            systemPrompt: SelectedSystemTemplate?.Content);

        // ── 写作完成：持久化并更新列表 ────────────────────────────────
        chapter.Content = fullContent;
        chapter.Status = ChapterStatus.Completed;
        await _dbService.UpdateChapterAsync(chapter);
        await _dbService.UpdateNovelTimestampAsync(CurrentNovel.Id);

        // 同步到 ObservableCollection，触发 UI 刷新
        Dispatcher.UIThread.Post(() =>
        {
            RefreshChapterInList(chapter, index);

            // 强制刷新 SelectedChapter 绑定：
            // Chapter 是 POCO，Content 变更不会触发 INPC 通知。
            // 先置 null 再赋值，强制 Avalonia 重新读取 Content 属性。
            SelectedChapter = null;
            SelectedChapter = chapter;

            // 切回非流式显示
            IsStreaming = false;
            ChapterMarkdown = fullContent;
            CurrentWordCount = fullContent.Length;
            StatusMessage = $"{chapter.Title} 写作完成（共 {CurrentWordCount} 字）";
            StopAutoSaveTimer();
        });
    }

    [RelayCommand]
    private void StopWriting()
    {
        _writingCts?.Cancel();
        // 状态在 StartWriting 的 finally 中统一更新
    }

    [RelayCommand]
    private void ToggleTemplatePanel()
    {
        ShowTemplatePanel = !ShowTemplatePanel;
    }

    [RelayCommand]
    private void ToggleVersionPanel()
    {
        ShowVersionPanel = !ShowVersionPanel;
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

    // ── 工具方法 ──────────────────────────────────────────────────────
    private void RefreshChapterInList(Chapter chapter, int index)
    {
        if (index >= 0 && index < Chapters.Count)
        {
            Chapters.RemoveAt(index);
            Chapters.Insert(index, chapter);
        }
    }

    private void StartAutoSaveTimer()
    {
        StopAutoSaveTimer();
        _autoSaveTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(AutoSaveIntervalMs),
            DispatcherPriority.Background,
            async (s, e) =>
            {
                var words = System.Threading.Interlocked.Exchange(ref _wordsSinceLastSave, 0);
                if (words > 0)
                {
                    await SaveVersionAsync("auto-save");
                }
            });
        _autoSaveTimer.Start();
    }

    private void StopAutoSaveTimer()
    {
        _autoSaveTimer?.Stop();
        _autoSaveTimer?.Stop();
        _autoSaveTimer = null;
    }

    private async Task LoadVersionsAsync(int chapterId)
    {
        var versionList = await _dbService.GetVersionsAsync(chapterId);
        Versions = new ObservableCollection<ChapterVersion>(versionList);
    }

    private async Task SaveVersionAsync(string trigger)
    {
        if (SelectedChapter == null || string.IsNullOrEmpty(SelectedChapter.Content))
            return;

        var version = new ChapterVersion
        {
            ChapterId = SelectedChapter.Id,
            Content = SelectedChapter.Content,
            WordCount = SelectedChapter.Content.Length,
            Trigger = trigger
        };

        await _dbService.AddVersionAsync(version);

        // 清理旧自动保存版本
        if (trigger == "auto-save")
            await _dbService.DeleteOldVersionsAsync(SelectedChapter.Id);

        await LoadVersionsAsync(SelectedChapter.Id);
    }

    
    private string BuildPreviousSummary()
    {
        if (SelectedChapter == null || Chapters.Count == 0)
            return string.Empty;

        var idx = Chapters.IndexOf(SelectedChapter);
        if (idx <= 0)
            return string.Empty;

        return Chapters[idx - 1].Summary;
    }

    [RelayCommand]
    private async Task SendInstruction()
    {
        if (string.IsNullOrWhiteSpace(InstructionText) || SelectedChapter == null)
            return;

        _writingCts?.Cancel();
        await SaveVersionAsync("rewrite");

        IsWriting = true;
        StatusMessage = "正在根据指令重写章节...";

        try
        {
            var storyService = _storyService;
            var newContent = await storyService.RewriteChapterAsync(
                SelectedChapter.Content,
                InstructionText,
                SelectedChapter.Title,
                CurrentNovel!.Genre,
                CurrentNovel.WorldSetting,
                previousSummary: BuildPreviousSummary(),
                rewriteTemplate: SelectedChapterTemplate?.Content,
                systemPrompt: SelectedSystemTemplate?.Content);

            SelectedChapter.Content = newContent;
            await _dbService.UpdateChapterAsync(SelectedChapter);
            ChapterMarkdown = newContent;

            StatusMessage = "章节已根据指令更新";
            InstructionText = string.Empty;
            await SaveVersionAsync("manual-save");
        }
        catch (Exception ex)
        {
            StatusMessage = $"重写失败: {ex.Message}";
        }
        finally
        {
            IsWriting = false;
        }
    }

    [RelayCommand]
    private async Task RewriteChapter()
    {
        InstructionText = "重新写这一章，内容方向不变";
        await SendInstruction();
    }

    [RelayCommand]
    private async Task SaveSnapshot()
    {
        await SaveVersionAsync("manual-save");
        StatusMessage = "快照已保存";
    }

    [RelayCommand]
    private async Task RollbackVersion(ChapterVersion version)
    {
        if (SelectedChapter == null) return;

        SelectedChapter.Content = version.Content;
        ChapterMarkdown = version.Content;
        await _dbService.UpdateChapterAsync(SelectedChapter);
        await SaveVersionAsync("manual-save");
        StatusMessage = "已回滚到版本 " + version.Id;
    }

    [RelayCommand]
    private void ViewVersion(ChapterVersion version)
    {
        StatusMessage = $"查看版本 {version.Id}（{version.Trigger}，{version.WordCount} 字）";
    }
}
