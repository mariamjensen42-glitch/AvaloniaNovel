# 小说工程结构 - 项目页面实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在左侧 NavigationView 增加"项目"页面，展示小说工程的章节树结构，支持新建/打开工程、添加/删除章节。

**Architecture:** 采用 MVVM 模式，新增 `ProjectViewModel` + `ProjectView` 作为项目页面，新增 `NovelProjectService` 处理工程文件的序列化/反序列化（JSON 格式），章节树使用 `TreeView` 绑定递归的 `ChapterNode` 集合。工程文件存储在独立目录下，包含 `project.json`（元数据）和各章节的 `.md` 文件。

**Tech Stack:** Avalonia 11.3.10, FluentAvaloniaUI 2.5.0, CommunityToolkit.Mvvm 8.4.1, System.Text.Json

---

## 文件结构

| 文件 | 职责 |
|------|------|
| `Models/NovelProject.cs` | 工程数据模型（工程信息、章节树） |
| `Models/ChapterNode.cs` | 章节节点模型（支持递归树结构） |
| `Services/NovelProjectService.cs` | 工程文件的读写服务 |
| `ViewModels/ProjectViewModel.cs` | 项目页面的 ViewModel |
| `Views/ProjectView.axaml` | 项目页面 XAML（TreeView + 工具栏） |
| `Views/ProjectView.axaml.cs` | 项目页面后置代码 |
| `ViewModels/MainViewModel.cs` | 修改：添加 ProjectPage 导航 |
| `Views/MainWindow.axaml` | 修改：NavigationView 添加"项目"菜单项 |
| `App.axaml.cs` | 修改：注册 ProjectViewModel 依赖 |

---

## Task 1: 创建数据模型

**Files:**
- Create: `Models/ChapterNode.cs`
- Create: `Models/NovelProject.cs`

- [ ] **Step 1.1: 创建 ChapterNode 模型**

```csharp
using System;
using System.Collections.ObjectModel;

namespace AgentNovel.Models;

public class ChapterNode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "未命名章节";
    public string FileName { get; set; } = "";
    public bool IsFolder { get; set; }
    public ObservableCollection<ChapterNode> Children { get; set; } = new();
}
```

- [ ] **Step 1.2: 创建 NovelProject 模型**

```csharp
using System;
using System.Collections.ObjectModel;

namespace AgentNovel.Models;

public class NovelProject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "未命名小说";
    public string Author { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public ObservableCollection<ChapterNode> Chapters { get; set; } = new();
}
```

---

## Task 2: 创建工程文件服务

**Files:**
- Create: `Services/NovelProjectService.cs`

- [ ] **Step 2.1: 实现 NovelProjectService**

```csharp
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AgentNovel.Models;

namespace AgentNovel.Services;

public class NovelProjectService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<NovelProject> CreateProjectAsync(string directoryPath, string title)
    {
        var project = new NovelProject { Title = title };
        var projectDir = Path.Combine(directoryPath, $"{title}_{project.Id:N}");
        Directory.CreateDirectory(projectDir);

        await SaveProjectAsync(project, projectDir);
        return project;
    }

    public async Task<NovelProject?> LoadProjectAsync(string projectDir)
    {
        var projectFile = Path.Combine(projectDir, "project.json");
        if (!File.Exists(projectFile)) return null;

        var json = await File.ReadAllTextAsync(projectFile);
        return JsonSerializer.Deserialize<NovelProject>(json, JsonOptions);
    }

    public async Task SaveProjectAsync(NovelProject project, string projectDir)
    {
        project.UpdatedAt = DateTime.Now;
        var projectFile = Path.Combine(projectDir, "project.json");
        var json = JsonSerializer.Serialize(project, JsonOptions);
        await File.WriteAllTextAsync(projectFile, json);
    }

    public string GetChapterFilePath(string projectDir, ChapterNode chapter)
    {
        return Path.Combine(projectDir, chapter.FileName);
    }

    public async Task<string> ReadChapterContentAsync(string projectDir, ChapterNode chapter)
    {
        var path = GetChapterFilePath(projectDir, chapter);
        if (!File.Exists(path)) return "";
        return await File.ReadAllTextAsync(path);
    }

    public async Task WriteChapterContentAsync(string projectDir, ChapterNode chapter, string content)
    {
        var path = GetChapterFilePath(projectDir, chapter);
        await File.WriteAllTextAsync(path, content);
    }
}
```

---

## Task 3: 创建 ProjectViewModel

**Files:**
- Create: `ViewModels/ProjectViewModel.cs`

- [ ] **Step 3.1: 实现 ProjectViewModel**

```csharp
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentNovel.Models;
using AgentNovel.Services;

namespace AgentNovel.ViewModels;

public partial class ProjectViewModel : ViewModelBase
{
    private readonly NovelProjectService _projectService;
    private string? _currentProjectDir;

    [ObservableProperty]
    private NovelProject? _currentProject;

    [ObservableProperty]
    private ChapterNode? _selectedChapter;

    [ObservableProperty]
    private string _statusMessage = "未打开工程";

    public ProjectViewModel()
    {
        _projectService = new NovelProjectService();
    }

    [RelayCommand]
    private async Task CreateProjectAsync()
    {
        // 实际实现需要通过 Avalonia 的 StorageProvider 选择目录
        // 这里先预留，后续在 View 中通过交互触发
        var title = "我的小说";
        var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        CurrentProject = await _projectService.CreateProjectAsync(dir, title);
        _currentProjectDir = Path.Combine(dir, $"{title}_{CurrentProject.Id:N}");
        StatusMessage = $"已创建: {CurrentProject.Title}";
    }

    [RelayCommand]
    private void AddChapter()
    {
        if (CurrentProject == null) return;

        var chapter = new ChapterNode
        {
            Title = $"第 {CurrentProject.Chapters.Count + 1} 章",
            FileName = $"chapter_{Guid.NewGuid():N}.md"
        };
        CurrentProject.Chapters.Add(chapter);
        _projectService.SaveProjectAsync(CurrentProject, _currentProjectDir!);
        StatusMessage = $"已添加: {chapter.Title}";
    }

    [RelayCommand]
    private void AddFolder()
    {
        if (CurrentProject == null) return;

        var folder = new ChapterNode
        {
            Title = "新建卷",
            IsFolder = true
        };
        CurrentProject.Chapters.Add(folder);
        _projectService.SaveProjectAsync(CurrentProject, _currentProjectDir!);
    }

    [RelayCommand]
    private void RemoveChapter()
    {
        if (CurrentProject == null || SelectedChapter == null) return;
        CurrentProject.Chapters.Remove(SelectedChapter);
        _projectService.SaveProjectAsync(CurrentProject, _currentProjectDir!);
        StatusMessage = "已删除章节";
    }
}
```

---

## Task 4: 创建 ProjectView 页面

**Files:**
- Create: `Views/ProjectView.axaml`
- Create: `Views/ProjectView.axaml.cs`

- [ ] **Step 4.1: 创建 ProjectView.axaml**

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="using:AgentNovel.ViewModels"
             xmlns:ui="using:FluentAvalonia.UI.Controls"
             x:Class="AgentNovel.Views.ProjectView"
             x:DataType="vm:ProjectViewModel"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="600">

    <Grid RowDefinitions="Auto,*">
        <!-- 工具栏 -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="8" Margin="16,12">
            <Button Content="新建工程" Command="{Binding CreateProjectCommand}" />
            <Button Content="添加章节" Command="{Binding AddChapterCommand}" />
            <Button Content="添加卷" Command="{Binding AddFolderCommand}" />
            <Button Content="删除" Command="{Binding RemoveChapterCommand}" />
        </StackPanel>

        <!-- 章节树 -->
        <Border Grid.Row="1" Margin="16,0,16,16" BorderBrush="{DynamicResource CardStrokeColorDefaultBrush}" BorderThickness="1" CornerRadius="8">
            <Grid RowDefinitions="*,Auto">
                <TreeView Grid.Row="0" ItemsSource="{Binding CurrentProject.Chapters}" SelectedItem="{Binding SelectedChapter}" Margin="8">
                    <TreeView.ItemTemplate>
                        <TreeDataTemplate ItemsSource="{Binding Children}">
                            <StackPanel Orientation="Horizontal" Spacing="8">
                                <ui:SymbolIcon Symbol="{Binding IsFolder, Converter={StaticResource FolderIconConverter}}" />
                                <TextBlock Text="{Binding Title}" />
                            </StackPanel>
                        </TreeDataTemplate>
                    </TreeView.ItemTemplate>
                </TreeView>

                <TextBlock Grid.Row="1" Text="{Binding StatusMessage}" Margin="12,8" Opacity="0.6" FontSize="12" />
            </Grid>
        </Border>
    </Grid>

</UserControl>
```

- [ ] **Step 4.2: 创建 ProjectView.axaml.cs**

```csharp
using Avalonia.Controls;

namespace AgentNovel.Views;

public partial class ProjectView : UserControl
{
    public ProjectView()
    {
        InitializeComponent();
    }
}
```

---

## Task 5: 修改 MainViewModel 添加导航

**Files:**
- Modify: `ViewModels/MainViewModel.cs`

- [ ] **Step 5.1: 修改 MainViewModel 添加 ProjectPage**

将 `MainViewModel` 修改为：

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;

namespace AgentNovel.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? _currentPage;

    [ObservableProperty]
    private object? _selectedMenuItem;

    public MainViewModel(SettingsViewModel settingsViewModel, ProjectViewModel projectViewModel)
    {
        SettingsPage = settingsViewModel;
        ProjectPage = projectViewModel;
        AboutPage = new AboutViewModel();
        CurrentPage = projectViewModel;
    }

    public SettingsViewModel SettingsPage { get; }
    public ProjectViewModel ProjectPage { get; }
    public AboutViewModel AboutPage { get; }

    partial void OnSelectedMenuItemChanged(object? oldValue, object? newValue)
    {
        if (newValue is not NavigationViewItem item) return;

        var tag = item.Tag?.ToString();
        CurrentPage = tag switch
        {
            "project" => ProjectPage,
            "settings" => SettingsPage,
            "about" => AboutPage,
            _ => ProjectPage
        };
    }
}
```

---

## Task 6: 修改 MainWindow.axaml 添加菜单项

**Files:**
- Modify: `Views/MainWindow.axaml`

- [ ] **Step 6.1: NavigationView 添加"项目"菜单项**

在 `NavigationView.MenuItems` 中，在"设置"之前添加：

```xml
<ui:NavigationViewItem Content="项目" Tag="project">
    <ui:NavigationViewItem.IconSource>
        <ui:SymbolIconSource Symbol="{x:Static ui:Symbol.Library}" />
    </ui:NavigationViewItem.IconSource>
</ui:NavigationViewItem>
```

同时修改窗口标题：`Title="AgentNovel - 小说工程"`

---

## Task 7: 修改 App.axaml.cs 注册依赖

**Files:**
- Modify: `App.axaml.cs`

- [ ] **Step 7.1: 注册 ProjectViewModel 并传入 MainViewModel**

将 `OnFrameworkInitializationCompleted` 修改为：

```csharp
public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        var settingsVm = new SettingsViewModel();
        var projectVm = new ProjectViewModel();
        desktop.MainWindow = new MainWindow
        {
            DataContext = new MainViewModel(settingsVm, projectVm),
        };
    }

    base.OnFrameworkInitializationCompleted();
}
```

---

## Task 8: 编译验证

- [ ] **Step 8.1: 编译项目**

Run: `dotnet build`
Expected: Build succeeded with 0 errors

---

## 后续扩展（不在本次计划内）

1. `CreateProjectAsync` 中通过 `StorageProvider` 弹出文件夹选择对话框
2. `OpenProjectAsync` 打开已有工程
3. 章节拖拽排序
4. 双击章节打开编辑器页面
5. 最近打开工程列表
