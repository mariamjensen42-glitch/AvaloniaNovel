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
    private readonly DatabaseService _dbService;
    private readonly ExportService _exportService;

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
    private ObservableCollection<PromptTemplate> _rewriteTemplates = new();

    [ObservableProperty]
    private ObservableCollection<PromptTemplate> _summaryTemplates = new();

    [ObservableProperty]
    private PromptTemplate? _selectedSystemTemplate;

    [ObservableProperty]
    private PromptTemplate? _selectedOutlineTemplate;

    [ObservableProperty]
    private PromptTemplate? _selectedChapterTemplate;

    [ObservableProperty]
    private PromptTemplate? _selectedRewriteTemplate;

    [ObservableProperty]
    private PromptTemplate? _selectedSummaryTemplate;

    /// <summary>是否显示模板选择面板</summary>
    [ObservableProperty]
    private bool _showTemplatePanel;

    // ── 手动编辑 ──────────────────────────────────────────────────────
    /// <summary>是否处于手动编辑模式</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    private bool _isEditing;

    /// <summary>编辑框中的文本内容（编辑模式下使用）</summary>
    [ObservableProperty]
    private string _editingContent = string.Empty;

    /// <summary>能否进入编辑模式（非 AI 写作中、有选中章节）</summary>
    public bool CanEdit => !IsWriting && !IsEditing && SelectedChapter != null;

    // ── 剧情干预 ─────────────────────────────────────────────────────
    /// <summary>是否显示剧情干预对话框</summary>
    [ObservableProperty]
    private bool _showRewriteDialog;

    /// <summary>用户输入的干预指令</summary>
    [ObservableProperty]
    private string _rewriteInstruction = string.Empty;

    // ── 章节摘要 ─────────────────────────────────────────────────────

    /// <summary>是否正在生成摘要</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerateSummary))]
    private bool _isGeneratingSummary;

    /// <summary>能否手动生成摘要（非 AI 写作中、非生成中、有选中章节且有正文内容）</summary>
    public bool CanGenerateSummary => !IsWriting && !IsGeneratingSummary
        && SelectedChapter != null && !string.IsNullOrEmpty(SelectedChapter.Content);

    // ── 字数统计 / 进度可视化 ──────────────────────────────────────────

    /// <summary>是否显示统计面板</summary>
    [ObservableProperty]
    private bool _showStatsPanel;

    /// <summary>全书总字数</summary>
    [ObservableProperty]
    private int _totalWordCount;

    /// <summary>已完成章节数</summary>
    [ObservableProperty]
    private int _completedChapterCount;

    /// <summary>总章节数</summary>
    [ObservableProperty]
    private int _totalChapterCount;

    /// <summary>写作完成百分比（0-100）</summary>
    [ObservableProperty]
    private double _completionPercentage;

    /// <summary>当前选中章节的字数</summary>
    [ObservableProperty]
    private int _selectedChapterWordCount;

    // ── 写作历史 / 版本快照 ────────────────────────────────────────────

    /// <summary>是否显示历史版本面板</summary>
    [ObservableProperty]
    private bool _showHistoryPanel;

    /// <summary>当前章节的快照列表</summary>
    [ObservableProperty]
    private ObservableCollection<ChapterSnapshot> _snapshots = new();

    /// <summary>在历史面板中选中预览的快照</summary>
    [ObservableProperty]
    private ChapterSnapshot? _selectedSnapshot;

    /// <summary>快照预览内容（选中快照时显示）</summary>
    [ObservableProperty]
    private string _snapshotPreview = string.Empty;

    // 选中快照时加载预览
    partial void OnSelectedSnapshotChanged(ChapterSnapshot? value)
    {
        SnapshotPreview = value?.Content ?? string.Empty;
    }

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

        // 加载模板
        await LoadTemplatesAsync();

        // 初始统计
        RefreshStatistics();
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

        var rewriteList = await _dbService.GetPromptTemplatesByTypeAsync(PromptTemplateType.Rewrite);
        RewriteTemplates = new ObservableCollection<PromptTemplate>(rewriteList);
        SelectedRewriteTemplate = rewriteList.FirstOrDefault(t => t.IsBuiltIn) ?? rewriteList.FirstOrDefault();

        var summaryList = await _dbService.GetPromptTemplatesByTypeAsync(PromptTemplateType.Summary);
        SummaryTemplates = new ObservableCollection<PromptTemplate>(summaryList);
        SelectedSummaryTemplate = summaryList.FirstOrDefault(t => t.IsBuiltIn) ?? summaryList.FirstOrDefault();
    }

    // 当 IsWriting 变化时，通知 CanEdit 和 CanGenerateSummary 刷新
    partial void OnIsWritingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(CanGenerateSummary));
    }

    // 当 IsEditing 变化时，通知 CanEdit 刷新
    partial void OnIsEditingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanEdit));
    }

    // 当 IsGeneratingSummary 变化时，通知 CanGenerateSummary 刷新
    partial void OnIsGeneratingSummaryChanged(bool value)
    {
        OnPropertyChanged(nameof(CanGenerateSummary));
    }

    // 当 SelectedChapter 变化时，更新非流式的 Markdown 显示内容，并退出编辑模式
    partial void OnSelectedChapterChanged(Chapter? value)
    {
        // 切换章节时退出编辑模式，丢弃未保存更改
        if (IsEditing)
        {
            IsEditing = false;
            EditingContent = string.Empty;
        }
        ChapterMarkdown = value?.Content ?? string.Empty;
        SelectedChapterWordCount = value?.Content?.Length ?? 0;
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(CanGenerateSummary));
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
            RefreshStatistics();
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
            var storyService = new StoryService();
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
        Chapter chapter, StoryService storyService, CancellationToken ct)
    {
        var index = Chapters.IndexOf(chapter);

        // ── 写作前自动快照（章节已有内容时） ────────────────────────────
        if (!string.IsNullOrEmpty(chapter.Content))
            await _dbService.SaveSnapshotAsync(chapter.Id, chapter.Content, "AI写作前备份");

        // 更新状态为写作中
        chapter.Status = ChapterStatus.Writing;
        await _dbService.UpdateChapterAsync(chapter);
        RefreshChapterInList(chapter, index);

        StatusMessage = $"正在写作: {chapter.Title}";

        // 重置流式缓冲
        StreamingBuilder.Clear();
        CurrentWordCount = 0;
        IsStreaming = true;

        var previousSummary = GetPreviousContext(index);

        // ── 流式回调：在 UI 线程追加到 ObservableStringBuilder ──────────
        void OnChunk(string chunk)
        {
            Dispatcher.UIThread.Post(() =>
            {
                StreamingBuilder.Append(chunk);
                CurrentWordCount += chunk.Length;
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

        // ── 自动生成正文摘要（后台执行，不阻塞 UI 刷新） ──────────────
        _ = GenerateAndSaveContentSummaryAsync(chapter);

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
            RefreshStatistics();
            StatusMessage = $"{chapter.Title} 写作完成（共 {CurrentWordCount} 字）";
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
    private void ToggleStatsPanel()
    {
        ShowStatsPanel = !ShowStatsPanel;
    }

    // ── 手动编辑命令 ───────────────────────────────────────────────────

    /// <summary>进入编辑模式：将当前章节内容填入编辑框</summary>
    [RelayCommand]
    private void EnterEditMode()
    {
        if (SelectedChapter == null || IsWriting) return;
        EditingContent = SelectedChapter.Content ?? string.Empty;
        IsEditing = true;
    }

    /// <summary>保存编辑：写入数据库并刷新 Markdown 渲染</summary>
    [RelayCommand]
    private async Task SaveEdit()
    {
        if (SelectedChapter == null) return;

        var chapter = SelectedChapter;
        var newContent = EditingContent;

        try
        {
            // ── 手动编辑前自动快照 ──────────────────────────────────────
            if (!string.IsNullOrEmpty(chapter.Content))
                await _dbService.SaveSnapshotAsync(chapter.Id, chapter.Content, "手动编辑前备份");

            await _dbService.UpdateChapterContentAsync(chapter.Id, newContent);
            await _dbService.UpdateNovelTimestampAsync(CurrentNovel!.Id);

            chapter.Content = newContent;

            // ── 手动编辑后自动重新生成摘要 ──────────────────────────────
            _ = GenerateAndSaveContentSummaryAsync(chapter);

            var index = Chapters.IndexOf(chapter);
            RefreshChapterInList(chapter, index);

            // 退出编辑模式，刷新 Markdown 显示
            IsEditing = false;
            EditingContent = string.Empty;
            ChapterMarkdown = newContent;

            // 强制刷新 SelectedChapter 绑定
            SelectedChapter = null;
            SelectedChapter = chapter;

            RefreshStatistics();
            StatusMessage = $"{chapter.Title} 已保存（共 {newContent.Length} 字）";
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存失败: {ex.Message}";
        }
    }

    /// <summary>取消编辑：丢弃更改，切回 Markdown 渲染</summary>
    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        EditingContent = string.Empty;
        StatusMessage = "已取消编辑";
    }

    // ── 写作历史命令 ───────────────────────────────────────────────────

    /// <summary>打开历史版本面板，加载当前章节快照列表</summary>
    [RelayCommand]
    private async Task OpenHistory()
    {
        if (SelectedChapter == null) return;
        var list = await _dbService.GetSnapshotsAsync(SelectedChapter.Id);
        Snapshots = new ObservableCollection<ChapterSnapshot>(list);
        SelectedSnapshot = Snapshots.FirstOrDefault();
        ShowHistoryPanel = true;
    }

    /// <summary>关闭历史版本面板</summary>
    [RelayCommand]
    private void CloseHistory()
    {
        ShowHistoryPanel = false;
        SelectedSnapshot = null;
        SnapshotPreview = string.Empty;
    }

    /// <summary>一键回退：将选中快照恢复为当前章节内容</summary>
    [RelayCommand]
    private async Task RestoreSnapshot()
    {
        if (SelectedChapter == null || SelectedSnapshot == null) return;

        var chapter = SelectedChapter;
        var snapshot = SelectedSnapshot;

        try
        {
            await _dbService.RestoreSnapshotAsync(chapter.Id, snapshot);
            await _dbService.UpdateNovelTimestampAsync(CurrentNovel!.Id);

            chapter.Content = snapshot.Content;

            ShowHistoryPanel = false;
            SelectedSnapshot = null;

            var index = Chapters.IndexOf(chapter);
            RefreshChapterInList(chapter, index);

            ChapterMarkdown = snapshot.Content;
            SelectedChapter = null;
            SelectedChapter = chapter;

            RefreshStatistics();
            StatusMessage = $"已恢复到：{snapshot.Reason}（{snapshot.CreatedAt:MM-dd HH:mm}）";

            // 刷新快照列表（恢复前自动备份了一条）
            var list = await _dbService.GetSnapshotsAsync(chapter.Id);
            Snapshots = new ObservableCollection<ChapterSnapshot>(list);
        }
        catch (Exception ex)
        {
            StatusMessage = $"恢复失败: {ex.Message}";
        }
    }

    /// <summary>手动创建当前章节的快照备份</summary>
    [RelayCommand]
    private async Task CreateManualSnapshot()
    {
        if (SelectedChapter == null || string.IsNullOrEmpty(SelectedChapter.Content)) return;

        await _dbService.SaveSnapshotAsync(SelectedChapter.Id, SelectedChapter.Content, "手动备份");

        // 若历史面板已打开，刷新列表
        if (ShowHistoryPanel)
        {
            var list = await _dbService.GetSnapshotsAsync(SelectedChapter.Id);
            Snapshots = new ObservableCollection<ChapterSnapshot>(list);
        }

        StatusMessage = "已创建手动备份";
    }

    /// <summary>打开剧情干预对话框</summary>
    [RelayCommand]
    private void OpenRewriteDialog()
    {
        if (SelectedChapter == null || string.IsNullOrEmpty(SelectedChapter.Content))
        {
            StatusMessage = "请先选择一个已写完的章节";
            return;
        }
        RewriteInstruction = string.Empty;
        ShowRewriteDialog = true;
    }

    /// <summary>取消剧情干预</summary>
    [RelayCommand]
    private void CancelRewrite()
    {
        ShowRewriteDialog = false;
        RewriteInstruction = string.Empty;
    }

    /// <summary>确认剧情干预，启动流式重写</summary>
    [RelayCommand]
    private async Task ConfirmRewrite()
    {
        if (SelectedChapter == null || string.IsNullOrWhiteSpace(RewriteInstruction))
            return;

        ShowRewriteDialog = false;
        _writingCts?.Cancel();
        _writingCts = new CancellationTokenSource();

        IsWriting = true;
        IsStreaming = true;
        StatusMessage = "正在重写章节...";

        try
        {
            var storyService = new StoryService();
            var chapter = SelectedChapter;
            var index = Chapters.IndexOf(chapter);
            var previousSummary = GetPreviousContext(index);

            // ── 重写前自动快照 ──────────────────────────────────────────
            if (!string.IsNullOrEmpty(chapter.Content))
                await _dbService.SaveSnapshotAsync(chapter.Id, chapter.Content, "AI重写前备份");

            StreamingBuilder.Clear();
            CurrentWordCount = 0;

            void OnChunk(string chunk)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    StreamingBuilder.Append(chunk);
                    CurrentWordCount += chunk.Length;
                    StatusMessage = $"正在重写: {chapter.Title}（{CurrentWordCount} 字）";
                    StreamingContentUpdated?.Invoke();
                });
            }

            var fullContent = await storyService.RewriteChapterStreamAsync(
                chapter.Content,
                RewriteInstruction,
                chapter.Title,
                chapter.Summary,
                CurrentNovel!.Genre,
                CurrentNovel.WorldSetting,
                previousSummary,
                OnChunk,
                _writingCts.Token,
                rewriteTemplate: SelectedRewriteTemplate?.Content,
                systemPrompt: SelectedSystemTemplate?.Content);

            // 保存重写后的内容
            await _dbService.UpdateChapterContentAsync(chapter.Id, fullContent);
            await _dbService.UpdateNovelTimestampAsync(CurrentNovel.Id);

            chapter.Content = fullContent;

            // ── 重写完成后自动重新生成摘要 ───────────────────────────────
            _ = GenerateAndSaveContentSummaryAsync(chapter);

            Dispatcher.UIThread.Post(() =>
            {
                RefreshChapterInList(chapter, index);
                SelectedChapter = null;
                SelectedChapter = chapter;

                IsStreaming = false;
                ChapterMarkdown = fullContent;
                CurrentWordCount = fullContent.Length;
                RefreshStatistics();
                StatusMessage = $"{chapter.Title} 重写完成（共 {CurrentWordCount} 字）";
            });
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "已停止重写";
        }
        catch (Exception ex)
        {
            StatusMessage = $"重写失败: {ex.Message}";
        }
        finally
        {
            IsWriting = false;
            IsStreaming = false;
        }
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
            Chapters[index] = chapter;
    }

    /// <summary>
    /// 刷新全书统计数据：总字数、完成章节、完成百分比。
    /// 在章节列表变化、写作/编辑/重写完成后调用。
    /// </summary>
    private void RefreshStatistics()
    {
        TotalChapterCount = Chapters.Count;
        CompletedChapterCount = Chapters.Count(c => c.Status == ChapterStatus.Completed);
        TotalWordCount = Chapters.Sum(c => c.Content?.Length ?? 0);
        CompletionPercentage = TotalChapterCount > 0
            ? Math.Round((double)CompletedChapterCount / TotalChapterCount * 100, 1)
            : 0;

        // 更新当前选中章节字数
        SelectedChapterWordCount = SelectedChapter?.Content?.Length ?? 0;
    }

    /// <summary>
    /// 获取前文上下文：优先使用前一章的 ContentSummary（正文摘要），
    /// 回退到 Summary（大纲概要），解决长篇创作中 AI "失忆"问题。
    /// </summary>
    private string GetPreviousContext(int currentIndex)
    {
        if (currentIndex <= 0) return string.Empty;
        var prev = Chapters[currentIndex - 1];
        return !string.IsNullOrEmpty(prev.ContentSummary)
            ? prev.ContentSummary
            : prev.Summary;
    }

    // ── 章节摘要命令 ───────────────────────────────────────────────────

    /// <summary>
    /// 后台生成章节正文摘要并保存到数据库（自动调用，写作/重写/编辑完成后触发）。
    /// 不阻塞主流程，失败静默处理。
    /// </summary>
    private async Task GenerateAndSaveContentSummaryAsync(Chapter chapter)
    {
        try
        {
            var storyService = new StoryService();
            var summary = await storyService.GenerateContentSummaryAsync(
                chapter.Title,
                chapter.Summary,
                chapter.Content,
                summaryTemplate: SelectedSummaryTemplate?.Content,
                systemPrompt: SelectedSystemTemplate?.Content);

            if (!string.IsNullOrWhiteSpace(summary))
            {
                chapter.ContentSummary = summary.Trim();
                await _dbService.UpdateChapterContentSummaryAsync(chapter.Id, chapter.ContentSummary);

                // 刷新章节列表显示
                var index = Chapters.IndexOf(chapter);
                if (index >= 0)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        RefreshChapterInList(chapter, index);
                        // 如果当前正在查看该章节，强制刷新绑定
                        if (SelectedChapter?.Id == chapter.Id)
                        {
                            SelectedChapter = null;
                            SelectedChapter = chapter;
                        }
                    });
                }
            }
        }
        catch
        {
            // 摘要生成失败不影响主流程，静默忽略
        }
    }

    /// <summary>手动触发生成当前章节的摘要</summary>
    [RelayCommand]
    private async Task GenerateSummary()
    {
        if (SelectedChapter == null || string.IsNullOrEmpty(SelectedChapter.Content)) return;

        IsGeneratingSummary = true;
        StatusMessage = $"正在生成摘要: {SelectedChapter.Title}";

        try
        {
            var storyService = new StoryService();
            var chapter = SelectedChapter;

            var summary = await storyService.GenerateContentSummaryAsync(
                chapter.Title,
                chapter.Summary,
                chapter.Content,
                summaryTemplate: SelectedSummaryTemplate?.Content,
                systemPrompt: SelectedSystemTemplate?.Content);

            if (!string.IsNullOrWhiteSpace(summary))
            {
                chapter.ContentSummary = summary.Trim();
                await _dbService.UpdateChapterContentSummaryAsync(chapter.Id, chapter.ContentSummary);

                var index = Chapters.IndexOf(chapter);
                RefreshChapterInList(chapter, index);

                // 强制刷新 SelectedChapter 绑定
                SelectedChapter = null;
                SelectedChapter = chapter;

                StatusMessage = $"摘要生成完成（{chapter.ContentSummary.Length} 字）";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"摘要生成失败: {ex.Message}";
        }
        finally
        {
            IsGeneratingSummary = false;
        }
    }
}
