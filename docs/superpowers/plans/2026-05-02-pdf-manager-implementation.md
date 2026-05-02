# PDF管理工具实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将AgentNovel项目改造为PDF管理工具，支持合并、拆分、页面操作功能

**Architecture:** 三栏布局设计 - 左侧导航栏（功能切换）、中间文件列表、右侧预览区。使用PDFsharp处理PDF操作，MVVM架构分离视图和业务逻辑。

**Tech Stack:** Avalonia UI 11.3, FluentAvaloniaUI 2.5, CommunityToolkit.Mvvm 8.4, PDFsharp 1.50, .NET 10

---

## 文件结构

### 新建文件
- `Models/PdfFile.cs` - PDF文件模型
- `Models/PdfPage.cs` - PDF页面模型
- `Services/PdfService.cs` - PDF操作服务
- `Services/ThumbnailService.cs` - 缩略图生成服务
- `ViewModels/MergeViewModel.cs` - 合并功能视图模型
- `ViewModels/SplitViewModel.cs` - 拆分功能视图模型
- `ViewModels/PageManagerViewModel.cs` - 页面管理视图模型
- `Views/MergeView.axaml(.cs)` - 合并功能视图
- `Views/SplitView.axaml(.cs)` - 拆分功能视图
- `Views/PageManagerView.axaml(.cs)` - 页面管理视图
- `Helpers/PageRangeParser.cs` - 页码范围解析器

### 修改文件
- `AgentNovel.csproj` - 添加PDFsharp依赖，移除Markdown.Avalonia
- `ViewModels/MainViewModel.cs` - 改造为功能导航
- `Views/MainWindow.axaml(.cs)` - 改造为三栏布局
- `App.axaml` - 更新应用标题和资源

### 删除文件
- `Models/ChapterNode.cs`
- `Models/NovelProject.cs`
- `Services/NovelProjectService.cs`
- `ViewModels/EditorViewModel.cs`
- `ViewModels/ProjectViewModel.cs`
- `Views/EditorView.axaml(.cs)`
- `Views/ProjectView.axaml(.cs)`
- `Views/AboutView.axaml(.cs)`

---

## Task 1: 清理现有代码

**Files:**
- Delete: `Models/ChapterNode.cs`
- Delete: `Models/NovelProject.cs`
- Delete: `Services/NovelProjectService.cs`
- Delete: `ViewModels/EditorViewModel.cs`
- Delete: `ViewModels/ProjectViewModel.cs`
- Delete: `Views/EditorView.axaml`
- Delete: `Views/EditorView.axaml.cs`
- Delete: `Views/ProjectView.axaml`
- Delete: `Views/ProjectView.axaml.cs`
- Delete: `Views/AboutView.axaml`
- Delete: `Views/AboutView.axaml.cs`

- [ ] **Step 1: 删除小说相关的模型文件**

删除以下文件：
- `Models/ChapterNode.cs`
- `Models/NovelProject.cs`

- [ ] **Step 2: 删除小说相关的服务文件**

删除 `Services/NovelProjectService.cs`

- [ ] **Step 3: 删除小说相关的视图模型文件**

删除以下文件：
- `ViewModels/EditorViewModel.cs`
- `ViewModels/ProjectViewModel.cs`

- [ ] **Step 4: 删除小说相关的视图文件**

删除以下文件：
- `Views/EditorView.axaml`
- `Views/EditorView.axaml.cs`
- `Views/ProjectView.axaml`
- `Views/ProjectView.axaml.cs`
- `Views/AboutView.axaml`
- `Views/AboutView.axaml.cs`

- [ ] **Step 5: 提交删除操作**

```bash
git add -A
git commit -m "chore: 删除小说管理相关代码"
```

---

## Task 2: 更新项目依赖

**Files:**
- Modify: `AgentNovel.csproj`

- [ ] **Step 1: 更新项目文件，添加PDFsharp依赖并移除Markdown.Avalonia**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>

  <ItemGroup>
    <Folder Include="Models\" />
    <AvaloniaResource Include="Assets\**" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.3.10" />
    <PackageReference Include="Avalonia.Desktop" Version="11.3.10" />
    <PackageReference Include="Avalonia.Fonts.Inter" Version="11.3.10" />
    <PackageReference Include="FluentAvaloniaUI" Version="2.5.0" />
    <PackageReference Include="AvaloniaUI.DiagnosticsSupport" Version="2.2.1">
      <IncludeAssets Condition="'$(Configuration)' != 'Debug'">None</IncludeAssets>
      <PrivateAssets Condition="'$(Configuration)' != 'Debug'">All</PrivateAssets>
    </PackageReference>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.1" />
    <PackageReference Include="PdfSharp" Version="1.50.5147" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: 还原依赖包**

```bash
dotnet restore
```

- [ ] **Step 3: 提交依赖更新**

```bash
git add AgentNovel.csproj
git commit -m "chore: 添加PDFsharp依赖，移除Markdown.Avalonia"
```

---

## Task 3: 创建PDF模型

**Files:**
- Create: `Models/PdfFile.cs`
- Create: `Models/PdfPage.cs`

- [ ] **Step 1: 创建PdfPage模型**

```csharp
using System;
using Avalonia.Media.Imaging;

namespace AgentNovel.Models;

public class PdfPage
{
    public int PageNumber { get; set; }
    public int Rotation { get; set; } = 0;
    public Bitmap? Thumbnail { get; set; }
    public bool IsSelected { get; set; }
}
```

- [ ] **Step 2: 创建PdfFile模型**

```csharp
using System;
using System.Collections.ObjectModel;

namespace AgentNovel.Models;

public class PdfFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public long FileSize { get; set; }
    public ObservableCollection<PdfPage> Pages { get; set; } = new();
}
```

- [ ] **Step 3: 提交模型创建**

```bash
git add Models/PdfFile.cs Models/PdfPage.cs
git commit -m "feat: 添加PDF模型类"
```

---

## Task 4: 创建页码范围解析器

**Files:**
- Create: `Helpers/PageRangeParser.cs`

- [ ] **Step 1: 创建页码范围解析器**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgentNovel.Helpers;

public static class PageRangeParser
{
    public static List<int> Parse(string range, int maxPage)
    {
        var pages = new HashSet<int>();
        var parts = range.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.Contains('-'))
            {
                var rangeParts = trimmed.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (rangeParts.Length == 2 &&
                    int.TryParse(rangeParts[0], out int start) &&
                    int.TryParse(rangeParts[1], out int end))
                {
                    for (int i = Math.Max(1, start); i <= Math.Min(maxPage, end); i++)
                    {
                        pages.Add(i);
                    }
                }
            }
            else if (int.TryParse(trimmed, out int pageNum))
            {
                if (pageNum >= 1 && pageNum <= maxPage)
                {
                    pages.Add(pageNum);
                }
            }
        }

        return pages.OrderBy(p => p).ToList();
    }
}
```

- [ ] **Step 2: 提交解析器创建**

```bash
git add Helpers/PageRangeParser.cs
git commit -m "feat: 添加页码范围解析器"
```

---

## Task 5: 创建PDF服务

**Files:**
- Create: `Services/PdfService.cs`

- [ ] **Step 1: 创建PDF服务基础框架**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AgentNovel.Models;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace AgentNovel.Services;

public class PdfService
{
    public async Task<PdfFile> LoadPdfAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var document = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            var fileInfo = new FileInfo(filePath);
            
            var pdfFile = new PdfFile
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                PageCount = document.PageCount,
                FileSize = fileInfo.Length
            };

            for (int i = 0; i < document.PageCount; i++)
            {
                pdfFile.Pages.Add(new PdfPage
                {
                    PageNumber = i + 1,
                    Rotation = (int)document.Pages[i].Rotate
                });
            }

            document.Close();
            return pdfFile;
        });
    }

    public async Task MergePdfsAsync(List<string> filePaths, string outputPath, IProgress<int>? progress = null)
    {
        await Task.Run(() =>
        {
            using var outputDocument = new PdfDocument();
            int totalFiles = filePaths.Count;
            int processedFiles = 0;

            foreach (var filePath in filePaths)
            {
                using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
                foreach (var page in inputDocument.Pages)
                {
                    outputDocument.AddPage(page);
                }
                processedFiles++;
                progress?.Report((processedFiles * 100) / totalFiles);
            }

            outputDocument.Save(outputPath);
        });
    }

    public async Task SplitPdfAsync(string filePath, List<int> pageNumbers, string outputPath)
    {
        await Task.Run(() =>
        {
            using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            using var outputDocument = new PdfDocument();

            foreach (var pageNum in pageNumbers.OrderBy(p => p))
            {
                if (pageNum >= 1 && pageNum <= inputDocument.PageCount)
                {
                    outputDocument.AddPage(inputDocument.Pages[pageNum - 1]);
                }
            }

            outputDocument.Save(outputPath);
        });
    }

    public async Task ReorderPagesAsync(string filePath, List<int> newOrder, string outputPath)
    {
        await Task.Run(() =>
        {
            using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            using var outputDocument = new PdfDocument();

            foreach (var pageNum in newOrder)
            {
                if (pageNum >= 1 && pageNum <= inputDocument.PageCount)
                {
                    outputDocument.AddPage(inputDocument.Pages[pageNum - 1]);
                }
            }

            outputDocument.Save(outputPath);
        });
    }

    public async Task RotatePagesAsync(string filePath, List<int> pageNumbers, int rotation, string outputPath)
    {
        await Task.Run(() =>
        {
            using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            
            foreach (var pageNum in pageNumbers)
            {
                if (pageNum >= 1 && pageNum <= inputDocument.PageCount)
                {
                    var page = inputDocument.Pages[pageNum - 1];
                    page.Rotate = (PdfPageRotate)(((int)page.Rotate + rotation) % 360);
                }
            }

            inputDocument.Save(outputPath);
        });
    }

    public async Task DeletePagesAsync(string filePath, List<int> pageNumbers, string outputPath)
    {
        await Task.Run(() =>
        {
            using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            var pagesToKeep = Enumerable.Range(1, inputDocument.PageCount)
                .Where(p => !pageNumbers.Contains(p))
                .ToList();

            using var outputDocument = new PdfDocument();
            foreach (var pageNum in pagesToKeep)
            {
                outputDocument.AddPage(inputDocument.Pages[pageNum - 1]);
            }

            outputDocument.Save(outputPath);
        });
    }
}
```

- [ ] **Step 2: 提交PDF服务创建**

```bash
git add Services/PdfService.cs
git commit -m "feat: 添加PDF操作服务"
```

---

## Task 6: 创建缩略图服务

**Files:**
- Create: `Services/ThumbnailService.cs`

- [ ] **Step 1: 创建缩略图服务**

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace AgentNovel.Services;

public class ThumbnailService
{
    private readonly int _thumbnailWidth = 200;

    public async Task<Bitmap?> GenerateThumbnailAsync(string pdfPath, int pageNumber)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
                if (pageNumber < 1 || pageNumber > document.PageCount)
                    return null;

                var page = document.Pages[pageNumber - 1];
                return RenderPageToBitmap(page);
            }
            catch
            {
                return null;
            }
        });
    }

    public async Task<Bitmap?> GenerateThumbnailAsync(PdfPage page)
    {
        return await Task.Run(() => RenderPageToBitmap(page));
    }

    private Bitmap? RenderPageToBitmap(PdfPage page)
    {
        try
        {
            var width = _thumbnailWidth;
            var height = (int)(width * (page.Height / page.Width));

            using var stream = new MemoryStream();
            
            // PDFsharp不直接支持渲染，这里创建占位图
            // 实际项目中需要使用PdfiumViewer或其他渲染库
            var pixels = new byte[width * height * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255;     // B
                pixels[i + 1] = 255; // G
                pixels[i + 2] = 255; // R
                pixels[i + 3] = 255; // A
            }

            return new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Premul, pixels, new PixelSize(width, height), new Vector(96, 96), width * 4);
        }
        catch
        {
            return null;
        }
    }
}
```

- [ ] **Step 2: 提交缩略图服务创建**

```bash
git add Services/ThumbnailService.cs
git commit -m "feat: 添加缩略图生成服务"
```

---

## Task 7: 创建合并功能视图模型

**Files:**
- Create: `ViewModels/MergeViewModel.cs`

- [ ] **Step 1: 创建合并视图模型**

```csharp
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AgentNovel.Models;
using AgentNovel.Services;
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
        var storage = App.StorageProvider;
        var files = await storage.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "选择PDF文件",
            AllowMultiple = true,
            FileTypeFilter = new[] { new Avalonia.Platform.Storage.FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } } }
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

        var storage = App.StorageProvider;
        var file = await storage.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
        {
            Title = "保存合并后的PDF",
            SuggestedFileName = "merged.pdf",
            FileTypeChoices = new[] { new Avalonia.Platform.Storage.FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } } }
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
```

- [ ] **Step 2: 提交合并视图模型创建**

```bash
git add ViewModels/MergeViewModel.cs
git commit -m "feat: 添加合并功能视图模型"
```

---

## Task 8: 创建拆分功能视图模型

**Files:**
- Create: `ViewModels/SplitViewModel.cs`

- [ ] **Step 1: 创建拆分视图模型**

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using AgentNovel.Models;
using AgentNovel.Services;
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
        var storage = App.StorageProvider;
        var files = await storage.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "选择PDF文件",
            AllowMultiple = false,
            FileTypeFilter = new[] { new Avalonia.Platform.Storage.FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } } }
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

        var storage = App.StorageProvider;
        var folder = await storage.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
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
```

- [ ] **Step 2: 提交拆分视图模型创建**

```bash
git add ViewModels/SplitViewModel.cs
git commit -m "feat: 添加拆分功能视图模型"
```

---

## Task 9: 创建页面管理视图模型

**Files:**
- Create: `ViewModels/PageManagerViewModel.cs`

- [ ] **Step 1: 创建页面管理视图模型**

```csharp
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AgentNovel.Models;
using AgentNovel.Services;
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
        var storage = App.StorageProvider;
        var files = await storage.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "选择PDF文件",
            AllowMultiple = false,
            FileTypeFilter = new[] { new Avalonia.Platform.Storage.FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } } }
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

        var storage = App.StorageProvider;
        var file = await storage.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
        {
            Title = "保存PDF文件",
            SuggestedFileName = Path.GetFileNameWithoutExtension(CurrentFile.FileName) + "_modified.pdf",
            FileTypeChoices = new[] { new Avalonia.Platform.Storage.FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } } }
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
```

- [ ] **Step 2: 提交页面管理视图模型创建**

```bash
git add ViewModels/PageManagerViewModel.cs
git commit -m "feat: 添加页面管理视图模型"
```

---

## Task 10: 更新主视图模型

**Files:**
- Modify: `ViewModels/MainViewModel.cs`

- [ ] **Step 1: 更新主视图模型**

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentNovel.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage = new MergeViewModel();

    [ObservableProperty]
    private bool _isMergeView = true;

    [ObservableProperty]
    private bool _isSplitView;

    [ObservableProperty]
    private bool _isPageManagerView;

    [ObservableProperty]
    private bool _isSettingsView;

    partial void OnIsMergeViewChanged(bool value)
    {
        if (value)
        {
            CurrentPage = new MergeViewModel();
            IsSplitView = false;
            IsPageManagerView = false;
            IsSettingsView = false;
        }
    }

    partial void OnIsSplitViewChanged(bool value)
    {
        if (value)
        {
            CurrentPage = new SplitViewModel();
            IsMergeView = false;
            IsPageManagerView = false;
            IsSettingsView = false;
        }
    }

    partial void OnIsPageManagerViewChanged(bool value)
    {
        if (value)
        {
            CurrentPage = new PageManagerViewModel();
            IsMergeView = false;
            IsSplitView = false;
            IsSettingsView = false;
        }
    }

    partial void OnIsSettingsViewChanged(bool value)
    {
        if (value)
        {
            CurrentPage = new SettingsViewModel();
            IsMergeView = false;
            IsSplitView = false;
            IsPageManagerView = false;
        }
    }
}
```

- [ ] **Step 2: 提交主视图模型更新**

```bash
git add ViewModels/MainViewModel.cs
git commit -m "feat: 更新主视图模型支持功能导航"
```

---

## Task 11: 创建合并视图

**Files:**
- Create: `Views/MergeView.axaml`
- Create: `Views/MergeView.axaml.cs`

- [ ] **Step 1: 创建合并视图XAML**

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:AgentNovel.ViewModels"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="AgentNovel.Views.MergeView"
             x:DataType="vm:MergeViewModel">

    <Design.DataContext>
        <vm:MergeViewModel />
    </Design.DataContext>

    <Grid RowDefinitions="Auto,*,Auto" Margin="16">
        <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="8" Margin="0,0,0,16">
            <Button Content="添加文件" Command="{Binding AddFilesCommand}" />
            <Button Content="移除文件" Command="{Binding RemoveFileCommand}" 
                    IsEnabled="{Binding SelectedFile, Converter={x:Static ObjectConverters.IsNotNull}}" />
            <Button Content="清空列表" Command="{Binding ClearFilesCommand}" 
                    IsEnabled="{Binding PdfFiles.Count, Converter={x:Static ObjectConverters.ToBoolean}}" />
        </StackPanel>

        <ListBox Grid.Row="1"
                 ItemsSource="{Binding PdfFiles}"
                 SelectedItem="{Binding SelectedFile}"
                 SelectionMode="Single">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <Grid ColumnDefinitions="*,Auto" Margin="4">
                        <StackPanel Grid.Column="0" Spacing="4">
                            <TextBlock Text="{Binding FileName}" FontWeight="Bold" />
                            <TextBlock Text="{Binding PageCount, StringFormat='{}{0} 页'}" Opacity="0.7" />
                        </StackPanel>
                        <TextBlock Grid.Column="1" 
                                   Text="{Binding FileSize, Converter={x:Static ObjectConverters.StringFormat}, ConverterParameter='{}{0:N0} KB'}"
                                   VerticalAlignment="Center"
                                   Opacity="0.7" />
                    </Grid>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>

        <StackPanel Grid.Row="2" Spacing="8" Margin="0,16,0,0">
            <ProgressBar Value="{Binding Progress}" Maximum="100" 
                         IsVisible="{Binding IsProcessing}" />
            <Button Content="合并" Command="{Binding MergeCommand}" 
                    IsEnabled="{Binding !IsProcessing}"
                    HorizontalAlignment="Stretch" />
        </StackPanel>
    </Grid>
</UserControl>
```

- [ ] **Step 2: 创建合并视图代码后台**

```csharp
using Avalonia.Controls;

namespace AgentNovel.Views;

public partial class MergeView : UserControl
{
    public MergeView()
    {
        InitializeComponent();
    }
}
```

- [ ] **Step 3: 提交合并视图创建**

```bash
git add Views/MergeView.axaml Views/MergeView.axaml.cs
git commit -m "feat: 添加合并功能视图"
```

---

## Task 12: 创建拆分视图

**Files:**
- Create: `Views/SplitView.axaml`
- Create: `Views/SplitView.axaml.cs`

- [ ] **Step 1: 创建拆分视图XAML**

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:AgentNovel.ViewModels"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="AgentNovel.Views.SplitView"
             x:DataType="vm:SplitViewModel">

    <Design.DataContext>
        <vm:SplitViewModel />
    </Design.DataContext>

    <Grid RowDefinitions="Auto,Auto,*,Auto" Margin="16">
        <Button Grid.Row="0" Content="选择PDF文件" Command="{Binding SelectFileCommand}"
                HorizontalAlignment="Stretch" Margin="0,0,0,16" />

        <StackPanel Grid.Row="1" Spacing="12" Margin="0,0,0,16"
                    IsVisible="{Binding CurrentFile, Converter={x:Static ObjectConverters.IsNotNull}}">
            <TextBlock Text="{Binding CurrentFile.FileName}" FontWeight="Bold" FontSize="16" />
            <TextBlock Text="{Binding CurrentFile.PageCount, StringFormat='共 {0} 页'}" Opacity="0.7" />
            
            <TextBox Text="{Binding PageRange}" Watermark="页码范围 (如: 1-5, 8, 10-15)" />
            
            <StackPanel Spacing="8">
                <RadioButton Content="提取指定页面" IsChecked="{Binding ExtractMode}" />
                <RadioButton Content="按范围拆分为多个文件" IsChecked="{Binding !ExtractMode}" />
            </StackPanel>
        </StackPanel>

        <StackPanel Grid.Row="2" VerticalAlignment="Center" HorizontalAlignment="Center"
                    IsVisible="{Binding CurrentFile, Converter={x:Static ObjectConverters.IsNull}}">
            <TextBlock Text="请先选择一个PDF文件" Opacity="0.6" />
        </StackPanel>

        <StackPanel Grid.Row="3" Spacing="8" Margin="0,16,0,0">
            <ProgressBar Value="{Binding Progress}" Maximum="100"
                         IsVisible="{Binding IsProcessing}" />
            <Button Content="拆分" Command="{Binding SplitCommand}"
                    IsEnabled="{Binding !IsProcessing}"
                    HorizontalAlignment="Stretch" />
        </StackPanel>
    </Grid>
</UserControl>
```

- [ ] **Step 2: 创建拆分视图代码后台**

```csharp
using Avalonia.Controls;

namespace AgentNovel.Views;

public partial class SplitView : UserControl
{
    public SplitView()
    {
        InitializeComponent();
    }
}
```

- [ ] **Step 3: 提交拆分视图创建**

```bash
git add Views/SplitView.axaml Views/SplitView.axaml.cs
git commit -m "feat: 添加拆分功能视图"
```

---

## Task 13: 创建页面管理视图

**Files:**
- Create: `Views/PageManagerView.axaml`
- Create: `Views/PageManagerView.axaml.cs`

- [ ] **Step 1: 创建页面管理视图XAML**

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:AgentNovel.ViewModels"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="AgentNovel.Views.PageManagerView"
             x:DataType="vm:PageManagerViewModel">

    <Design.DataContext>
        <vm:PageManagerViewModel />
    </Design.DataContext>

    <Grid RowDefinitions="Auto,Auto,*,Auto" Margin="16">
        <Button Grid.Row="0" Content="打开PDF文件" Command="{Binding OpenFileCommand}"
                HorizontalAlignment="Stretch" Margin="0,0,0,16" />

        <StackPanel Grid.Row="1" Orientation="Horizontal" Spacing="8" Margin="0,0,0,16"
                    IsVisible="{Binding CurrentFile, Converter={x:Static ObjectConverters.IsNotNull}}">
            <Button Content="向左旋转" Command="{Binding RotateLeftCommand}"
                    IsEnabled="{Binding SelectedPages.Count, Converter={x:Static ObjectConverters.ToBoolean}}" />
            <Button Content="向右旋转" Command="{Binding RotateRightCommand}"
                    IsEnabled="{Binding SelectedPages.Count, Converter={x:Static ObjectConverters.ToBoolean}}" />
            <Button Content="删除" Command="{Binding DeleteCommand}"
                    IsEnabled="{Binding SelectedPages.Count, Converter={x:Static ObjectConverters.ToBoolean}}" />
        </StackPanel>

        <ListBox Grid.Row="2"
                 ItemsSource="{Binding CurrentFile.Pages}"
                 SelectedItems="{Binding SelectedPages}"
                 SelectionMode="Multiple,Toggle">
            <ListBox.ItemsPanel>
                <ItemsPanelTemplate>
                    <WrapPanel />
                </ItemsPanelTemplate>
            </ListBox.ItemsPanel>
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <Border Width="120" Height="160" Margin="4"
                            BorderBrush="{DynamicResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="4">
                        <Grid RowDefinitions="*,Auto">
                            <TextBlock Grid.Row="0" 
                                       Text="{Binding PageNumber, StringFormat='第 {0} 页'}"
                                       HorizontalAlignment="Center"
                                       VerticalAlignment="Center" />
                            <TextBlock Grid.Row="1" 
                                       Text="{Binding Rotation, StringFormat='旋转 {0}°'}"
                                       HorizontalAlignment="Center"
                                       FontSize="12"
                                       Opacity="0.7" />
                        </Grid>
                    </Border>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>

        <StackPanel Grid.Row="3" Spacing="8" Margin="0,16,0,0"
                    IsVisible="{Binding CurrentFile, Converter={x:Static ObjectConverters.IsNotNull}}">
            <ProgressBar IsVisible="{Binding IsProcessing}" IsIndeterminate="True" />
            <Button Content="保存" Command="{Binding SaveCommand}"
                    IsEnabled="{Binding !IsProcessing}"
                    HorizontalAlignment="Stretch" />
        </StackPanel>
    </Grid>
</UserControl>
```

- [ ] **Step 2: 创建页面管理视图代码后台**

```csharp
using Avalonia.Controls;

namespace AgentNovel.Views;

public partial class PageManagerView : UserControl
{
    public PageManagerView()
    {
        InitializeComponent();
    }
}
```

- [ ] **Step 3: 提交页面管理视图创建**

```bash
git add Views/PageManagerView.axaml Views/PageManagerView.axaml.cs
git commit -m "feat: 添加页面管理视图"
```

---

## Task 14: 更新设置视图模型

**Files:**
- Modify: `ViewModels/SettingsViewModel.cs`

- [ ] **Step 1: 更新设置视图模型**

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentNovel.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _appName = "PDF管理工具";

    [ObservableProperty]
    private string _version = "1.0.0";

    [ObservableProperty]
    private string _description = "简单易用的PDF管理工具，支持合并、拆分和页面操作。";
}
```

- [ ] **Step 2: 提交设置视图模型更新**

```bash
git add ViewModels/SettingsViewModel.cs
git commit -m "feat: 更新设置视图模型"
```

---

## Task 15: 更新设置视图

**Files:**
- Modify: `Views/SettingsView.axaml`

- [ ] **Step 1: 更新设置视图XAML**

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:AgentNovel.ViewModels"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="AgentNovel.Views.SettingsView"
             x:DataType="vm:SettingsViewModel">

    <Design.DataContext>
        <vm:SettingsViewModel />
    </Design.DataContext>

    <ScrollViewer>
        <StackPanel Margin="16" Spacing="16">
            <TextBlock Text="关于" FontSize="20" FontWeight="Bold" />
            
            <Border Background="{DynamicResource CardBackgroundFillColorDefaultBrush}"
                    BorderBrush="{DynamicResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8"
                    Padding="16">
                <StackPanel Spacing="8">
                    <TextBlock Text="{Binding AppName}" FontSize="24" FontWeight="Bold" />
                    <TextBlock Text="{Binding Version, StringFormat='版本: {0}'}" Opacity="0.7" />
                    <TextBlock Text="{Binding Description}" TextWrapping="Wrap" Margin="0,8,0,0" />
                </StackPanel>
            </Border>

            <TextBlock Text="功能说明" FontSize="20" FontWeight="Bold" Margin="0,16,0,0" />
            
            <Border Background="{DynamicResource CardBackgroundFillColorDefaultBrush}"
                    BorderBrush="{DynamicResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8"
                    Padding="16">
                <StackPanel Spacing="12">
                    <TextBlock Text="合并PDF" FontWeight="Bold" />
                    <TextBlock Text="将多个PDF文件合并为一个文件，支持拖拽调整顺序。" TextWrapping="Wrap" Opacity="0.8" />
                    
                    <TextBlock Text="拆分PDF" FontWeight="Bold" Margin="0,8,0,0" />
                    <TextBlock Text="按页码范围拆分PDF文件，支持提取指定页面。" TextWrapping="Wrap" Opacity="0.8" />
                    
                    <TextBlock Text="页面管理" FontWeight="Bold" Margin="0,8,0,0" />
                    <TextBlock Text="重新排序、旋转和删除PDF页面。" TextWrapping="Wrap" Opacity="0.8" />
                </StackPanel>
            </Border>
        </StackPanel>
    </ScrollViewer>
</UserControl>
```

- [ ] **Step 2: 提交设置视图更新**

```bash
git add Views/SettingsView.axaml
git commit -m "feat: 更新设置视图"
```

---

## Task 16: 更新主窗口

**Files:**
- Modify: `Views/MainWindow.axaml`
- Modify: `Views/MainWindow.axaml.cs`

- [ ] **Step 1: 更新主窗口XAML**

```xml
<wnd:AppWindow xmlns="https://github.com/avaloniaui"
               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
               xmlns:wnd="using:FluentAvalonia.UI.Windowing"
               xmlns:vm="using:AgentNovel.ViewModels"
               xmlns:views="using:AgentNovel.Views"
               xmlns:ui="using:FluentAvalonia.UI.Controls"
               xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
               xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
               mc:Ignorable="d" d:DesignWidth="900" d:DesignHeight="650"
               x:Class="AgentNovel.Views.MainWindow"
               x:DataType="vm:MainViewModel"
               Icon="/Assets/avalonia-logo.ico"
               Title="PDF管理工具"
               Width="1100" Height="700"
               MinWidth="900" MinHeight="500"
               WindowStartupLocation="CenterScreen"
               TransparencyLevelHint="Mica"
               Background="Transparent">

    <Design.DataContext>
        <vm:MainViewModel />
    </Design.DataContext>

    <Grid ColumnDefinitions="48,*">
        <Border Grid.Column="0" Background="{DynamicResource SystemControlBackgroundChromeMediumBrush}">
            <StackPanel Spacing="4" Margin="0,48,0,0">
                <ToggleButton IsChecked="{Binding IsMergeView}"
                              Width="40" Height="40"
                              HorizontalAlignment="Center"
                              ToolTip.Tip="合并PDF">
                    <ui:SymbolIcon Symbol="Merge" FontSize="18" />
                </ToggleButton>
                <ToggleButton IsChecked="{Binding IsSplitView}"
                              Width="40" Height="40"
                              HorizontalAlignment="Center"
                              ToolTip.Tip="拆分PDF">
                    <ui:SymbolIcon Symbol="Split" FontSize="18" />
                </ToggleButton>
                <ToggleButton IsChecked="{Binding IsPageManagerView}"
                              Width="40" Height="40"
                              HorizontalAlignment="Center"
                              ToolTip.Tip="页面管理">
                    <ui:SymbolIcon Symbol="Page" FontSize="18" />
                </ToggleButton>
                <ToggleButton IsChecked="{Binding IsSettingsView}"
                              Width="40" Height="40"
                              HorizontalAlignment="Center"
                              ToolTip.Tip="设置">
                    <ui:SymbolIcon Symbol="Setting" FontSize="18" />
                </ToggleButton>
            </StackPanel>
        </Border>

        <ContentControl Grid.Column="1" Content="{Binding CurrentPage}" />
    </Grid>

</wnd:AppWindow>
```

- [ ] **Step 2: 更新主窗口代码后台**

```csharp
using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;

namespace AgentNovel.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();
        TitleBar.ExtendsContentIntoTitleBar = true;
    }
}
```

- [ ] **Step 3: 提交主窗口更新**

```bash
git add Views/MainWindow.axaml Views/MainWindow.axaml.cs
git commit -m "feat: 更新主窗口为PDF管理工具界面"
```

---

## Task 17: 更新App.axaml

**Files:**
- Modify: `App.axaml`

- [ ] **Step 1: 更新App.axaml**

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="AgentNovel.App">
    <Application.Styles>
        <FluentTheme />
        <StyleInclude Source="avares://Avalonia.Fonts.Inter/FluentTheme.axaml" />
        <StyleInclude Source="avares://FluentAvalonia/Styles/FluentAvaloniaTheme.axaml" />
    </Application.Styles>
</Application>
```

- [ ] **Step 2: 提交App.axaml更新**

```bash
git add App.axaml
git commit -m "feat: 更新应用样式"
```

---

## Task 18: 更新ViewLocator

**Files:**
- Modify: `ViewLocator.cs`

- [ ] **Step 1: 更新ViewLocator**

```csharp
using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AgentNovel.ViewModels;

namespace AgentNovel;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
```

- [ ] **Step 2: 提交ViewLocator更新**

```bash
git add ViewLocator.cs
git commit -m "feat: 更新ViewLocator"
```

---

## Task 19: 构建和测试

**Files:**
- None

- [ ] **Step 1: 清理并构建项目**

```bash
dotnet clean
dotnet build
```

- [ ] **Step 2: 运行项目测试**

```bash
dotnet run
```

- [ ] **Step 3: 提交最终版本**

```bash
git add -A
git commit -m "feat: 完成PDF管理工具基础功能"
```

---

## 成功标准
- 项目可以成功编译运行
- 主窗口显示三栏布局
- 左侧导航栏可以切换功能
- 合并功能可以添加、移除文件并执行合并
- 拆分功能可以选择文件并按页码范围拆分
- 页面管理功能可以打开文件并显示页面列表
- 设置页面显示应用信息
