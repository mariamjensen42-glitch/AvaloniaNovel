# Story Intervention Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现剧情干预、章节重写、版本历史、自动保存、自定义 Prompt 模板管理功能

**Architecture:** 在现有 MVVM + EF Core + Semantic Kernel 架构基础上，新增 ChapterVersion 版本历史数据模型，扩展 StoryService 支持指令重写，在 CreateViewModel 中串联版本保存和干预逻辑。Template CRUD 通过已有 PromptTemplateViewModel/PromptTemplateView 实现界面。

**Tech Stack:** Avalonia 11.3.4, EF Core 8.0, Semantic Kernel 1.x, SukiUI 6.0.3

---

## File Structure

```
Models/
  ChapterVersion.cs           # 新增：版本历史实体

Data/
  NovelDbContext.cs           # 修改：新增 DbSet<ChapterVersion>

Migrations/
  YYYYMMDDHHMMSS_AddChapterVersion.cs  # 新增：EF Core 迁移

Services/
  DatabaseService.cs          # 修改：新增 GetVersions/AddVersion/GetLatestVersion/DeleteOldVersions 方法
  StoryService.cs             # 修改：新增 RewriteChapterAsync + 重写提示词

ViewModels/
  CreateViewModel.cs          # 修改：新增干预/重写/回滚/保存快照/自动保存逻辑
  PromptTemplateViewModel.cs  # 修改：新增 CreateTemplate/UpdateTemplate/DeleteTemplate 命令

Views/
  CreateView.axaml            # 修改：新增剧情干预面板、版本历史面板
  PromptTemplateView.axaml    # 修改：完善模板管理界面
```

---

## Task 1: ChapterVersion 数据模型

**Files:**
- Create: `Models/ChapterVersion.cs`
- Modify: `Models/Chapter.cs:1-50`（新增导航属性）

- [ ] **Step 1: 创建 ChapterVersion.cs**

```csharp
using System;

namespace AvaloniaNovel.Models;

public class ChapterVersion
{
    public int Id { get; set; }
    public int ChapterId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public string Trigger { get; set; } = "auto-save"; // auto-save / manual-save / rewrite
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Chapter? Chapter { get; set; }
}
```

- [ ] **Step 2: 修改 Chapter.cs，添加导航属性**

在 `Chapter` 类中添加：
```csharp
public List<ChapterVersion> Versions { get; set; } = new();
```

- [ ] **Step 3: 创建 EF Core 迁移**

```bash
dotnet ef migrations add AddChapterVersion --no-build
```

- [ ] **Step 4: 提交**

```bash
git add Models/ChapterVersion.cs Models/Chapter.cs Migrations/
git commit -m "feat: add ChapterVersion model for version history

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 2: DatabaseService 版本操作方法

**Files:**
- Modify: `Services/DatabaseService.cs`

- [ ] **Step 1: 读取 DatabaseService.cs 了解现有方法模式**

```bash
cat Services/DatabaseService.cs
```

- [ ] **Step 2: 添加版本相关方法到 DatabaseService**

在文件末尾添加：
```csharp
// 获取章节所有版本（按时间倒序）
public async Task<List<ChapterVersion>> GetVersionsAsync(int chapterId)
{
    return await _context.ChapterVersions
        .Where(v => v.ChapterId == chapterId)
        .OrderByDescending(v => v.CreatedAt)
        .ToListAsync();
}

// 保存新版本
public async Task<ChapterVersion> AddVersionAsync(ChapterVersion version)
{
    version.CreatedAt = DateTime.Now;
    _context.ChapterVersions.Add(version);
    await _context.SaveChangesAsync();
    return version;
}

// 获取章节最新版本
public async Task<ChapterVersion?> GetLatestVersionAsync(int chapterId)
{
    return await _context.ChapterVersions
        .Where(v => v.ChapterId == chapterId)
        .OrderByDescending(v => v.CreatedAt)
        .FirstOrDefaultAsync();
}

// 清理旧自动保存版本（每个章节保留最近 20 个 auto-save）
public async Task DeleteOldVersionsAsync(int chapterId)
{
    var oldVersions = await _context.ChapterVersions
        .Where(v => v.ChapterId == chapterId && v.Trigger == "auto-save")
        .OrderByDescending(v => v.CreatedAt)
        .Skip(20)
        .ToListAsync();

    if (oldVersions.Count > 0)
    {
        _context.ChapterVersions.RemoveRange(oldVersions);
        await _context.SaveChangesAsync();
    }
}

// 更新章节内容（用于回滚）
public async Task UpdateChapterContentAsync(int chapterId, string content)
{
    var chapter = await _context.Chapters.FindAsync(chapterId);
    if (chapter != null)
    {
        chapter.Content = content;
        await _context.SaveChangesAsync();
    }
}
```

- [ ] **Step 3: 提交**

```bash
git add Services/DatabaseService.cs
git commit -m "feat: add version management methods to DatabaseService

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 3: StoryService RewriteChapterAsync

**Files:**
- Modify: `Services/StoryService.cs`

- [ ] **Step 1: 读取 StoryService.cs 了解现有方法结构**

```bash
cat Services/StoryService.cs
```

- [ ] **Step 2: 添加重写提示词常量**

在 `DefaultChapterPrompt` 常量后添加：
```csharp
private const string DefaultRewritePrompt = @"## 任务
根据用户的新指令，修改当前章节内容。

## 输入
- 当前章节标题：{{chapterTitle}}
- 当前章节内容：{{currentContent}}
- 用户指令：{{instruction}}
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 严格按照用户指令调整剧情
2. 保持原有章节标题和整体结构
3. 调整后的内容应合理、流畅
4. 字数：2000-5000 字

## 输出
直接输出修改后的章节正文，不需要额外格式。";
```

- [ ] **Step 3: 添加 RewriteChapterAsync 方法**

在 `WriteChapterAsync` 方法后添加：
```csharp
public async Task<string> RewriteChapterAsync(
    string currentContent,
    string instruction,
    string chapterTitle,
    string genre, string worldSetting,
    string? rewriteTemplate = null, string? systemPrompt = null)
{
    var template = string.IsNullOrWhiteSpace(rewriteTemplate)
        ? DefaultRewritePrompt
        : rewriteTemplate;

    var prompt = RenderTemplate(template, new Dictionary<string, string>
    {
        ["chapterTitle"] = chapterTitle,
        ["currentContent"] = currentContent,
        ["instruction"] = instruction,
        ["genre"] = genre,
        ["worldSetting"] = worldSetting
    });

    return await _llmService.InvokeAsync(prompt, systemPrompt);
}
```

- [ ] **Step 4: 提交**

```bash
git add Services/StoryService.cs
git commit -m "feat: add RewriteChapterAsync to StoryService

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 4: CreateViewModel 扩展（版本历史 + 干预 + 自动保存）

**Files:**
- Modify: `ViewModels/CreateViewModel.cs`

- [ ] **Step 1: 读取 CreateViewModel.cs 了解现有结构**

```bash
cat ViewModels/CreateViewModel.cs
```

- [ ] **Step 2: 新增 ObservableCollection 和字段**

在类开头字段区域添加：
```csharp
// 版本历史
[ObservableProperty]
private ObservableCollection<ChapterVersion> _versions = new();

// 自动保存定时器
private System.Timers.Timer? _autoSaveTimer;
private int _wordsSinceLastSave = 0;
private const int AutoSaveWordThreshold = 500; // 每 500 字自动保存
private const int AutoSaveIntervalMs = 60000;   // 每 60 秒自动保存
```

- [ ] **Step 3: 新增 LoadVersionsAsync 方法**

```csharp
private async Task LoadVersionsAsync(int chapterId)
{
    var versionList = await _dbService.GetVersionsAsync(chapterId);
    Versions = new ObservableCollection<ChapterVersion>(versionList);
}
```

- [ ] **Step 4: 新增 SaveVersionAsync 方法**

```csharp
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
```

- [ ] **Step 5: 新增自动保存定时器逻辑**

在 `WriteChapterStreamingAsync` 方法中，流式回调里添加：
```csharp
_wordsSinceLastSave += chunk.Length;
if (_wordsSinceLastSave >= AutoSaveWordThreshold)
{
    _ = SaveVersionAsync("auto-save");
    _wordsSinceLastSave = 0;
}
```

添加定时器启动（在流式写作开始时）：
```csharp
_autoSaveTimer?.Stop();
_autoSaveTimer = new System.Timers.Timer(AutoSaveIntervalMs);
_autoSaveTimer.Elapsed += async (s, e) => await SaveVersionAsync("auto-save");
_autoSaveTimer.Start();
```

流式写作结束后停止：
```csharp
_autoSaveTimer?.Stop();
_autoSaveTimer?.Dispose();
_autoSaveTimer = null;
```

- [ ] **Step 6: 新增干预命令**

```csharp
[RelayCommand]
private async Task SendInstruction()
{
    if (string.IsNullOrWhiteSpace(_instructionText) || SelectedChapter == null)
        return;

    // 先停止当前写作
    _writingCts?.Cancel();

    // 保存当前版本为 rewrite
    await SaveVersionAsync("rewrite");

    IsWriting = true;
    StatusMessage = "正在根据指令重写章节...";

    try
    {
        var storyService = new StoryService();
        var newContent = await storyService.RewriteChapterAsync(
            SelectedChapter.Content,
            _instructionText,
            SelectedChapter.Title,
            CurrentNovel!.Genre,
            CurrentNovel.WorldSetting,
            chapterTemplate: SelectedChapterTemplate?.Content,
            systemPrompt: SelectedSystemTemplate?.Content);

        // 更新章节内容
        SelectedChapter.Content = newContent;
        await _dbService.UpdateChapterAsync(SelectedChapter);
        ChapterMarkdown = newContent;

        StatusMessage = "章节已根据指令更新";
        _instructionText = string.Empty;

        // 保存为 manual-save 版本
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

private string _instructionText = string.Empty;

[ObservableProperty]
private string instructionText
{
    get => _instructionText;
    set => SetProperty(ref _instructionText, value);
}
```

- [ ] **Step 7: 新增重写本章命令**

```csharp
[RelayCommand]
private async Task RewriteChapter()
{
    InstructionText = "重新写这一章，内容方向不变";
    await SendInstruction();
}
```

- [ ] **Step 8: 新增保存快照命令**

```csharp
[RelayCommand]
private async Task SaveSnapshot()
{
    await SaveVersionAsync("manual-save");
    StatusMessage = "快照已保存";
}
```

- [ ] **Step 9: 新增回滚命令**

```csharp
[RelayCommand]
private async Task RollbackVersion(ChapterVersion version)
{
    if (SelectedChapter == null) return;

    var confirmed = await ShowConfirmDialogAsync("确认回滚？", "回滚后当前内容将被替换。");
    if (!confirmed) return;

    SelectedChapter.Content = version.Content;
    await _dbService.UpdateChapterContentAsync(SelectedChapter.Id, version.Content);
    ChapterMarkdown = version.Content;
    await _dbService.UpdateChapterAsync(SelectedChapter);

    // 保存为手动版本
    await SaveVersionAsync("manual-save");

    StatusMessage = "已回滚到版本 " + version.Id;
}

private async Task<bool> ShowConfirmDialogAsync(string title, string message)
{
    // Avalonia 对话框实现
    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        var dialog = new Window
        {
            Title = title,
            Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = message },
                    new Button { Content = "确认", Command = ... },
                    new Button { Content = "取消" }
                }
            }
        };
        // 实现确认逻辑
    }
    return false;
}
```

- [ ] **Step 10: 新增查看版本命令**

```csharp
[RelayCommand]
private void ViewVersion(ChapterVersion version)
{
    // 弹窗显示版本内容
    ShowVersionContentDialog(version);
}

private void ShowVersionContentDialog(ChapterVersion version)
{
    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        var window = new Window
        {
            Title = $"版本 {version.Id} - {version.Trigger}",
            Width = 600,
            Height = 400,
            Content = new ScrollViewer
            {
                Content = new TextBlock
                {
                    Text = version.Content,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                }
            }
        };
        window.Show();
    }
}
```

- [ ] **Step 11: 修改 SelectedChapterChanged 时加载版本**

```csharp
partial void OnSelectedChapterChanged(Chapter? value)
{
    ChapterMarkdown = value?.Content ?? string.Empty;
    if (value != null)
        _ = LoadVersionsAsync(value.Id);
}
```

- [ ] **Step 12: 提交**

```bash
git add ViewModels/CreateViewModel.cs
git commit -m "feat: add intervention, version history, auto-save to CreateViewModel

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 5: CreateView.axaml UI 更新

**Files:**
- Modify: `Views/CreateView.axaml`

- [ ] **Step 1: 读取 CreateView.axaml 了解现有布局**

```bash
cat Views/CreateView.axaml
```

- [ ] **Step 2: 在控制面板区域添加剧情干预面板**

在现有控制按钮下方添加：
```xml
<!-- 剧情干预面板 -->
<Border Classes="card" Margin="0,8,0,0" Padding="12">
    <StackPanel Spacing="8">
        <TextBlock Text="剧情干预" FontWeight="Bold" />
        <Grid ColumnDefinitions="*,Auto" RowDefinitions="Auto">
            <TextBox x:Name="InstructionInput"
                     Text="{Binding InstructionText}"
                     Watermark="输入指令（如：让主角失忆、加入穿越元素）..."
                     Margin="0,0,8,0" />
            <Button Grid.Column="1" Content="发送指令"
                    Command="{Binding SendInstructionCommand}"
                    Classes="primary" />
        </Grid>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <Button Content="重写本章" Command="{Binding RewriteChapterCommand}" />
            <Button Content="保存快照" Command="{Binding SaveSnapshotCommand}" />
        </StackPanel>
    </StackPanel>
</Border>

<!-- 版本历史面板 -->
<Border Classes="card" Margin="0,8,0,0" Padding="12"
        IsVisible="{Binding Versions.Count}">
    <StackPanel Spacing="8">
        <Grid ColumnDefinitions="*,Auto">
            <TextBlock Text="版本历史" FontWeight="Bold" />
            <Button Content="折叠" Command="{Binding ToggleVersionsCommand}" />
        </Grid>
        <ItemsControl ItemsSource="{Binding Versions}">
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Border Padding="8" Margin="0,4" Background="#1A1A1A">
                        <Grid ColumnDefinitions="*,Auto,Auto">
                            <StackPanel>
                                <TextBlock>
                                    <Run Text="v" /><Run Text="{Binding Id}" />
                                    <Run Text=" · " /><Run Text="{Binding Trigger}" />
                                    <Run Text=" · " /><Run Text="{Binding WordCount}" /><Run Text=" 字" />
                                    <Run Text=" · " /><Run Text="{Binding CreatedAt, StringFormat='{}{0:MM-dd HH:mm}'}" />
                                </TextBlock>
                            </StackPanel>
                            <Button Grid.Column="1" Content="回滚"
                                    Command="{Binding $parent[ItemsControl].((vm:CreateViewModel)BindingContext).RollbackVersionCommand}"
                                    CommandParameter="{Binding}" />
                            <Button Grid.Column="2" Content="查看" Margin="8,0,0,0"
                                    Command="{Binding $parent[ItemsControl].((vm:CreateViewModel)BindingContext).ViewVersionCommand}"
                                    CommandParameter="{Binding}" />
                        </Grid>
                    </Border>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </StackPanel>
</Border>
```

- [ ] **Step 3: 提交**

```bash
git add Views/CreateView.axaml
git commit -m "feat: add intervention panel and version history UI to CreateView

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 6: PromptTemplateViewModel 模板 CRUD

**Files:**
- Modify: `ViewModels/PromptTemplateViewModel.cs`

- [ ] **Step 1: 读取 PromptTemplateViewModel.cs**

```bash
cat ViewModels/PromptTemplateViewModel.cs
```

- [ ] **Step 2: 添加模板 CRUD 命令**

添加：
```csharp
[RelayCommand]
private async Task CreateTemplate()
{
    if (string.IsNullOrWhiteSpace(NewTemplateName) || string.IsNullOrWhiteSpace(NewTemplateContent))
        return;

    var template = new PromptTemplate
    {
        Name = NewTemplateName,
        Type = SelectedTemplateType,
        Content = NewTemplateContent,
        IsBuiltIn = false,
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now
    };

    await _dbService.AddPromptTemplateAsync(template);
    await LoadTemplatesAsync();

    NewTemplateName = string.Empty;
    NewTemplateContent = string.Empty;
}

[RelayCommand]
private async Task UpdateTemplate(PromptTemplate template)
{
    if (template.IsBuiltIn) return; // 内置不可修改

    template.UpdatedAt = DateTime.Now;
    await _dbService.UpdatePromptTemplateAsync(template);
    await LoadTemplatesAsync();
}

[RelayCommand]
private async Task DeleteTemplate(PromptTemplate template)
{
    if (template.IsBuiltIn) return; // 内置不可删除

    var confirmed = await ShowConfirmAsync("确认删除模板？");
    if (!confirmed) return;

    await _dbService.DeletePromptTemplateAsync(template.Id);
    await LoadTemplatesAsync();
}
```

添加新模板的 ObservableProperty：
```csharp
[ObservableProperty]
private string _newTemplateName = string.Empty;

[ObservableProperty]
private string _newTemplateContent = string.Empty;

[ObservableProperty]
private PromptTemplateType _selectedTemplateType = PromptTemplateType.Chapter;
```

- [ ] **Step 3: 提交**

```bash
git add ViewModels/PromptTemplateViewModel.cs
git commit -m "feat: add template CRUD commands to PromptTemplateViewModel

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 7: PromptTemplateView.axaml 模板管理界面

**Files:**
- Modify: `Views/PromptTemplateView.axaml`

- [ ] **Step 1: 读取 PromptTemplateView.axaml**

```bash
cat Views/PromptTemplateView.axaml
```

- [ ] **Step 2: 添加模板管理 UI**

在现有内容基础上添加新建模板表单：
```xml
<!-- 新建模板 -->
<Border Classes="card" Margin="0,0,0,16" Padding="16">
    <StackPanel Spacing="12">
        <TextBlock Text="创建新模板" FontWeight="Bold" FontSize="16" />

        <Grid ColumnDefinitions="Auto,*" RowDefinitions="Auto,Auto,Auto">
            <TextBlock Grid.Row="0" Text="模板名称：" VerticalAlignment="Center" />
            <TextBox Grid.Row="0" Grid.Column="1"
                     Text="{Binding NewTemplateName}"
                     Watermark="输入模板名称..."
                     Margin="0,0,0,8" />

            <TextBlock Grid.Row="1" Text="模板类型：" VerticalAlignment="Center" />
            <ComboBox Grid.Row="1" Grid.Column="1"
                      SelectedItem="{Binding SelectedTemplateType}"
                      Margin="0,0,0,8">
                <ComboBoxItem Content="System" Tag="System" />
                <ComboBoxItem Content="Outline" Tag="Outline" />
                <ComboBoxItem Content="Chapter" Tag="Chapter" />
            </ComboBox>

            <TextBlock Grid.Row="2" Text="模板内容：" VerticalAlignment="Top" />
            <TextBox Grid.Row="2" Grid.Column="1"
                     Text="{Binding NewTemplateContent}"
                     Watermark="输入模板内容（支持占位符：{{genre}}, {{worldSetting}}, {{chapterTitle}}, {{chapterSummary}}, {{previousSummary}}）..."
                     AcceptsReturn="True"
                     MinHeight="150"
                     Margin="0,0,0,8" />
        </Grid>

        <Button Content="创建模板" Command="{Binding CreateTemplateCommand}" Classes="primary" HorizontalAlignment="Right" />
    </StackPanel>
</Border>

<!-- 模板列表 -->
<ItemsControl ItemsSource="{Binding Templates}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Border Classes="card" Margin="0,0,0,8" Padding="12">
                <Grid ColumnDefinitions="*,Auto">
                    <StackPanel>
                        <TextBlock FontWeight="Bold">
                            <Run Text="{Binding Name}" />
                            <Run Text=" · " /><Run Text="{Binding Type}" />
                            <Run Text=" · " /><Run Text="{Binding IsBuiltIn, Converter={x:Static BoolConverters.ToBuiltInText}}" />
                        </TextBlock>
                        <TextBlock Text="{Binding Content}" Opacity="0.7" TextTrimming="CharacterEllipsis" MaxWidth="400" />
                    </StackPanel>
                    <StackPanel Grid.Column="1" Orientation="Horizontal">
                        <Button Content="编辑" Command="{Binding $parent[ItemsControl].((vm:PromptTemplateViewModel)BindingContext).EditTemplateCommand}" CommandParameter="{Binding}" />
                        <Button Content="删除" Margin="8,0,0,0" Command="{Binding $parent[ItemsControl].((vm:PromptTemplateViewModel)BindingContext).DeleteTemplateCommand}" CommandParameter="{Binding}" />
                    </StackPanel>
                </Grid>
            </Border>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

- [ ] **Step 3: 提交**

```bash
git add Views/PromptTemplateView.axaml
git commit -m "feat: add template management UI to PromptTemplateView

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 8: 构建验证

**Files:** 无（验证步骤）

- [ ] **Step 1: 运行 dotnet build 验证编译**

```bash
dotnet build
```

期望：无编译错误

- [ ] **Step 2: 如有编译错误，修复后重新提交**

```bash
git add -A && git commit -m "fix: resolve build errors

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 9: 验证清单自检

对照 spec 检查：

- [ ] 用户输入指令后，AI 能基于指令调整章节内容 → `SendInstructionCommand` 已实现
- [ ] 重写前自动保存当前版本 → `SaveVersionAsync("rewrite")` 在 `SendInstruction` 开始时调用
- [ ] 版本历史面板显示所有版本 → `Versions` ObservableCollection + ItemsControl in axaml
- [ ] 可回滚和查看 → `RollbackVersionCommand` 和 `ViewVersionCommand` 已实现
- [ ] 自动保存每 500 字或 60 秒触发 → `AutoSaveWordThreshold` + `_autoSaveTimer` 已实现
- [ ] 用户可创建、编辑、删除自定义模板 → CRUD 命令已实现
- [ ] 内置模板不可修改和删除 → `if (template.IsBuiltIn) return` 防护已添加

---

## 依赖顺序

```
Task 1 → Task 2 → Task 3 → Task 4 → Task 5 → Task 6 → Task 7 → Task 8 → Task 9
```

---

**Plan complete and saved to `docs/superpowers/plans/2026-04-30-story-intervention-plan.md`.**

Two execution options:

**1. Subagent-Driven (recommended)** - I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** - Execute tasks in this session using executing-plans, batch execution with checkpoints

Which approach?