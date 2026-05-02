# PDF管理工具设计文档

## 概述
将AgentNovel项目改造为PDF管理工具，支持合并、拆分、页面操作（删除、旋转、重新排序）功能，面向个人用户，注重简单易用。

## 技术选型
- **PDF处理库**: PDFsharp (MIT许可证，纯.NET实现，跨平台兼容)
- **UI框架**: Avalonia UI 11.3 + FluentAvaloniaUI 2.5
- **MVVM框架**: CommunityToolkit.Mvvm 8.4
- **目标框架**: .NET 10

## 整体架构

### 界面布局（三栏设计）
- **左侧导航栏 (48px)**: 功能切换按钮
  - 合并功能
  - 拆分功能
  - 页面管理
  - 设置
- **中间文件列表 (240px)**: 显示已添加的PDF文件，支持拖拽排序
- **右侧预览区**: 显示PDF页面缩略图网格，支持选择和操作

### 核心组件

#### Models
- `PdfFile`: 存储PDF文件信息
  - Id: Guid
  - FileName: string
  - FilePath: string
  - PageCount: int
  - Pages: ObservableCollection<PdfPage>
  - FileSize: long

- `PdfPage`: 存储单个页面信息
  - PageNumber: int
  - Rotation: int (0, 90, 180, 270)
  - Thumbnail: Bitmap?
  - IsSelected: bool

#### Services
- `PdfService`: 封装PDFsharp操作
  - LoadPdfAsync(filePath): 加载PDF文件
  - MergePdfsAsync(filePaths, outputPath): 合并多个PDF
  - SplitPdfAsync(filePath, pageRanges, outputDir): 拆分PDF
  - ReorderPages(filePath, newOrder, outputPath): 重新排序页面
  - RotatePages(filePath, pageNumbers, rotation, outputPath): 旋转页面
  - DeletePages(filePath, pageNumbers, outputPath): 删除页面

- `ThumbnailService`: 生成PDF页面缩略图
  - GenerateThumbnailAsync(pdfPath, pageNumber, width, height): 生成单个页面缩略图
  - GenerateAllThumbnailsAsync(pdfPath, width, height): 生成所有页面缩略图

#### ViewModels
- `MainViewModel`: 主视图模型，管理当前功能页面
- `MergeViewModel`: 合并功能视图模型
- `SplitViewModel`: 拆分功能视图模型
- `PageManagerViewModel`: 页面管理视图模型
- `SettingsViewModel`: 设置视图模型

#### Views
- `MainWindow`: 主窗口
- `MergeView`: 合并功能视图
- `SplitView`: 拆分功能视图
- `PageManagerView`: 页面管理视图
- `SettingsView`: 设置视图

## 功能模块设计

### 1. 合并功能
**用户流程:**
1. 点击"添加文件"按钮或拖拽PDF文件到列表
2. 拖拽文件调整合并顺序
3. 点击"合并"按钮
4. 选择保存位置和文件名
5. 显示进度条，完成后提示成功

**界面元素:**
- 文件列表（支持拖拽排序）
- 添加文件按钮
- 移除文件按钮
- 清空列表按钮
- 合并按钮
- 进度条

### 2. 拆分功能
**用户流程:**
1. 点击"选择文件"按钮选择单个PDF
2. 显示PDF页面预览
3. 输入页码范围（如：1-5, 8, 10-15）
4. 选择拆分模式：
   - 按范围拆分为多个文件
   - 提取指定页面为新文件
5. 点击"拆分"按钮
6. 选择保存位置
7. 显示进度条，完成后提示成功

**界面元素:**
- 文件选择按钮
- 页面预览区域
- 页码范围输入框
- 拆分模式选择（RadioButton）
- 拆分按钮
- 进度条

### 3. 页面管理功能
**用户流程:**
1. 点击"打开文件"按钮选择PDF
2. 显示所有页面缩略图网格
3. 执行操作：
   - 拖拽页面重新排序
   - 选择页面后点击旋转按钮（90°/180°/270°）
   - 选择页面后点击删除按钮
4. 实时预览修改效果
5. 点击"保存"按钮
6. 选择保存位置（支持覆盖原文件或另存为新文件）

**界面元素:**
- 打开文件按钮
- 页面缩略图网格（支持拖拽和选择）
- 旋转按钮组（90°左旋、90°右旋、180°）
- 删除按钮
- 撤销按钮
- 保存按钮
- 另存为按钮

## 数据流设计

### 加载PDF流程
```
用户添加文件 → PdfService.LoadPdfAsync() 
→ 创建PdfFile对象 
→ ThumbnailService.GenerateAllThumbnailsAsync() 
→ 更新PdfFile.Pages中的Thumbnail属性 
→ UI显示缩略图
```

### 合并PDF流程
```
用户点击合并 → PdfService.MergePdfsAsync() 
→ 显示进度条 
→ 完成后提示成功 
→ 询问是否打开输出文件
```

### 拆分PDF流程
```
用户输入页码范围 → 解析页码范围 
→ PdfService.SplitPdfAsync() 
→ 显示进度条 
→ 完成后提示成功
```

### 页面管理流程
```
用户操作页面 → 更新PdfPage状态 
→ 实时更新预览 
→ 用户点击保存 → PdfService应用所有更改 
→ 保存到文件
```

## 错误处理策略

### 文件相关错误
- **文件损坏**: 显示错误提示"文件已损坏或不是有效的PDF文件"，跳过该文件
- **文件被占用**: 提示"文件正在被其他程序使用，请关闭后重试"
- **权限不足**: 提示"没有访问该文件的权限，请以管理员身份运行或选择其他文件"
- **文件不存在**: 提示"文件不存在或已被移动"

### 操作相关错误
- **磁盘空间不足**: 提前检测目标磁盘空间，不足时提示用户
- **操作失败**: 显示详细错误信息，提供重试选项
- **页码范围无效**: 提示"页码范围无效，请输入有效的页码范围（如：1-5, 8, 10-15）"

### 性能相关错误
- **内存不足**: 处理大文件时监控内存使用，接近上限时提示用户
- **操作超时**: 长时间操作支持取消，超时后提示用户

## 性能优化

### 缩略图加载
- 异步加载缩略图，不阻塞UI线程
- 优先加载可见区域的缩略图
- 缩略图缓存，避免重复生成
- 大文件只加载前20页缩略图，滚动时动态加载更多

### 大文件处理
- 显示进度条，支持取消操作
- 分批处理页面，避免内存溢出
- 后台线程处理，保持UI响应

### 内存管理
- 及时释放不再使用的PDF文档对象
- 缩略图使用适当尺寸（建议宽度200px）
- 大文件处理完成后主动GC

## 测试策略

### 单元测试
- `PdfService`核心操作测试
  - 合并多个PDF文件
  - 按页码范围拆分PDF
  - 页面重新排序
  - 页面旋转
  - 页面删除
- 页码范围解析测试
  - 有效范围：1-5, 8, 10-15
  - 无效范围：0, -1, 999（超出页数）
  - 边界情况：单页、全部页面
- 文件路径处理测试
  - 相对路径转绝对路径
  - 特殊字符处理
  - 长路径处理

### 集成测试
- 完整合并流程：添加文件 → 排序 → 合并 → 验证输出
- 完整拆分流程：选择文件 → 输入范围 → 拆分 → 验证输出
- 页面管理流程：重新排序 → 旋转 → 删除 → 保存 → 验证输出

### UI测试
- 文件拖拽功能
- 页面缩略图显示
- 错误提示显示
- 进度条显示

## 项目结构

```
AgentNovel/
├── Models/
│   ├── PdfFile.cs
│   └── PdfPage.cs
├── Services/
│   ├── PdfService.cs
│   ├── ThumbnailService.cs
│   └── SettingsService.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── MergeViewModel.cs
│   ├── SplitViewModel.cs
│   ├── PageManagerViewModel.cs
│   ├── SettingsViewModel.cs
│   └── ViewModelBase.cs
├── Views/
│   ├── MainWindow.axaml(.cs)
│   ├── MergeView.axaml(.cs)
│   ├── SplitView.axaml(.cs)
│   ├── PageManagerView.axaml(.cs)
│   └── SettingsView.axaml(.cs)
├── Converters/
│   └── PageNumberConverter.cs
├── Helpers/
│   └── PageRangeParser.cs
├── App.axaml(.cs)
├── Program.cs
└── ViewLocator.cs
```

## 依赖包

```xml
<PackageReference Include="PdfSharp" Version="1.50.5147" />
<PackageReference Include="Avalonia" Version="11.3.10" />
<PackageReference Include="Avalonia.Desktop" Version="11.3.10" />
<PackageReference Include="Avalonia.Fonts.Inter" Version="11.3.10" />
<PackageReference Include="FluentAvaloniaUI" Version="2.5.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.1" />
```

## 实施计划

### 阶段1：基础架构
1. 添加PDFsharp依赖
2. 创建Models（PdfFile, PdfPage）
3. 创建PdfService基础框架
4. 创建ThumbnailService

### 阶段2：核心功能
1. 实现合并功能
2. 实现拆分功能
3. 实现页面管理功能

### 阶段3：UI实现
1. 改造MainWindow布局
2. 实现MergeView
3. 实现SplitView
4. 实现PageManagerView

### 阶段4：优化和测试
1. 性能优化
2. 错误处理完善
3. 单元测试
4. 集成测试

## 成功标准
- 用户可以成功合并多个PDF文件
- 用户可以按页码范围拆分PDF文件
- 用户可以重新排序、旋转、删除PDF页面
- 界面响应流畅，操作直观
- 错误提示清晰，不会崩溃
