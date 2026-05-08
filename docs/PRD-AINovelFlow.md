# PRD：AINovelFlow - AI 网文创作工具

## 1. Overview

### Project Name
AINovelFlow

### Project Type
Windows Desktop Application (Avalonia .NET)

### Core Feature Summary
基于章节流水线的半自动网文创作工具，用户设定题材和世界观后，AI 持续自动写作，用户可中途干预调整剧情走向。支持自定义 Prompt 模板和章节版本管理。

### Target Users
写作爱好者 — 娱乐性创作，不知道写什么，AI 写自己看。

---

## 2. Technical Stack

| Layer | Technology |
|-------|------------|
| UI Framework | Avalonia UI 11.3.4 |
| MVVM | CommunityToolkit.Mvvm 8.4.1 |
| AI Framework | Semantic Kernel 1.x（含流式 API 支持） |
| AI Backend | DeepSeek API（OpenAI 兼容协议） |
| Markdown 渲染 | LiveMarkdown.Avalonia 1.9.2 |
| Database | SQLite + Entity Framework Core 8.0 |
| Theme | SukiUI 6.0.3 |

---

## 3. Architecture

```
┌─────────────────────────────────────────────┐
│                   Views                      │
│         (Avalonia .axaml)                   │
│  ┌──────────────────────────────────────┐   │
│  │  MarkdownRenderer (LiveMarkdown)     │   │
│  │  ObservableStringBuilder (流式绑定)  │   │
│  └──────────────────────────────────────┘   │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│               ViewModels                     │
│      (CommunityToolkit.Mvvm)               │
│  • StreamingBuilder (流式缓冲)              │
│  • CancellationTokenSource (停止写作)       │
│  • StreamingContentUpdated (自动滚动事件)   │
│  • PromptTemplateViewModel (模板管理)       │
│  • ChapterVersion (版本管理)                │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│                Services                      │
│  ┌─────────────┐  ┌─────────────────────┐  │
│  │ LLMService  │  │   StoryService      │  │
│  │ (SK+DeepSeek│  │   (大纲+章节+重写   │  │
│  │ 普通+流式)  │  │   +模板渲染)        │  │
│  └─────────────┘  └─────────────────────┘  │
│  ┌─────────────┐  ┌─────────────────────┐  │
│  │DatabaseService│ │  CoverImageService │  │
│  │ (EF Core    │  │  (封面选择+存储)   │  │
│  │  +模板+版本) │  │                     │  │
│  └─────────────┘  └─────────────────────┘  │
│  ┌─────────────┐  ┌─────────────────────┐  │
│  │DialogManager│  │  ExportService      │  │
│  │ (对话框管理)│  │  (TXT 导出)        │  │
│  └─────────────┘  └─────────────────────┘  │
└─────────────────────────────────────────────┘
```

---

## 4. Data Model

### 4.1 Novel（小说）

| Field | Type | Description |
|-------|------|-------------|
| Id | int | 主键 |
| Title | string | 小说标题 |
| Genre | string | 题材类型 |
| WorldSetting | string | 世界观设定 |
| CoverImagePath | string | 封面图片路径 |
| HasCoverImage | bool | 是否有封面（计算属性） |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime | 更新时间 |
| Chapters | List\<Chapter\> | 章节列表 |

### 4.2 Chapter（章节）

| Field | Type | Description |
|-------|------|-------------|
| Id | int | 主键 |
| NovelId | int | 所属小说外键 |
| Order | int | 章节序号 |
| Title | string | 章节标题 |
| Summary | string | 章节概要 |
| Content | string | 章节正文 |
| Status | ChapterStatus | 状态 |
| CreatedAt | DateTime | 创建时间 |
| Versions | List\<ChapterVersion\> | 版本列表 |

### 4.3 ChapterStatus（枚举）

```csharp
public enum ChapterStatus
{
    Outline = 0,     // 仅有大纲
    Writing = 1,     // 写作中
    Completed = 2,   // 已完成
    Revised = 3      // 已修订
}
```

### 4.4 ChapterVersion（章节版本）

| Field | Type | Description |
|-------|------|-------------|
| Id | int | 主键 |
| ChapterId | int | 所属章节外键 |
| Content | string | 版本内容 |
| WordCount | int | 字数 |
| Trigger | string | 触发类型（auto-save / manual-save / rewrite） |
| CreatedAt | DateTime | 创建时间 |

### 4.5 PromptTemplate（提示词模板）

| Field | Type | Description |
|-------|------|-------------|
| Id | int | 主键 |
| Name | string | 模板名称 |
| Type | PromptTemplateType | 模板类型 |
| Content | string | 模板内容（支持 Mustache 风格占位符） |
| Variables | string | 变量说明 |
| IsBuiltIn | bool | 是否为内置默认模板（不可删除） |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime | 更新时间 |

### 4.6 PromptTemplateType（枚举）

```csharp
public enum PromptTemplateType
{
    System = 0,       // 系统人设（SystemPrompt）
    Outline = 1,      // 大纲生成
    Chapter = 2       // 章节写作
}
```

### 4.7 AppSettings（应用设置）

| Field | Type | Description |
|-------|------|-------------|
| Id | int | 主键 |
| DeepSeekApiKey | string | API 密钥 |

---

## 5. Core Features

### 5.1 项目管理

- **新建小说**：输入标题、选择题材、填写世界观设定、选择封面图片
- **小说列表**：展示所有小说，显示标题、题材、章节数、更新时间、封面
- **删除小说**：确认后删除（同时删除封面图片）
- **打开小说**：加载并进入创作界面

### 5.2 创作流程（Chapter Pipeline）

#### Step 1：生成大纲

AI 根据用户输入的题材和世界观，生成：

- **章节列表**：10-20 章的章节标题
- **章节概要**：每章 50-100 字的简要描述
- **全局大纲**：整体故事主线

支持选择不同的大纲生成模板（通用、爽文节奏、悬疑、言情等）。

```json
{
  "chapters": [
    {
      "title": "第一章 意外穿越",
      "summary": "主角意外穿越到异世界，醒来发现自己身处陌生森林..."
    },
    ...
  ]
}
```

#### Step 2：章节写作

- AI 自动从第一章开始，根据大纲逐章生成正文
- **流式输出**：通过 Semantic Kernel 的 `GetStreamingChatMessageContentsAsync` 逐 token 推送，用户无需等待完整章节生成
- **Markdown 实时渲染**：基于 `ObservableStringBuilder` + `LiveMarkdown.Avalonia`，每收到 token 即解析渲染
- **自动滚动**：流式输出时内容区自动滚动到最新位置
- **停止写作**：通过 `CancellationTokenSource` 随时中断，已生成内容自动保存
- **自动保存版本**：每 500 字自动保存，每 60 秒定时保存
- 每章生成完毕后，状态变为 `Completed`
- 用户可随时暂停、修改上一章、或干预后续剧情
- 支持选择不同的系统人设模板和章节写作模板

#### Step 3：用户干预

- **调整剧情**：发送指令（如"让主角失忆""加入穿越元素"），AI 根据指令重写章节
- **重写章节**：一键重写当前章节，自动保存旧版本
- **手动编辑**：直接编辑正文内容
- **版本回滚**：回滚到任意历史版本

#### Step 4：自动继续

- 用户确认后，AI 自动继续写下一章
- 循环 Step 2-4，直到用户停止或写完全部章节

### 5.3 Prompt 模板管理

- **内置模板**：8 种系统人设模板、4 种大纲生成模板、4 种章节写作模板
- **自定义模板**：创建、编辑、删除自定义模板
- **模板复制**：基于现有模板创建副本
- **类型筛选**：按模板类型（系统人设/大纲生成/章节写作）筛选
- **占位符支持**：模板内容支持 `{{genre}}`、`{{worldSetting}}`、`{{chapterTitle}}` 等变量

**内置系统人设模板：**
- 通用网文作家、玄幻爽文风格、仙侠修真风格、都市言情风格
- 悬疑推理风格、科幻末世风格、历史穿越风格、无限流风格

**内置大纲生成模板：**
- 通用大纲生成、爽文节奏大纲、悬疑大纲生成、言情大纲生成

**内置章节写作模板：**
- 通用章节写作、爽文章节写作、悬疑章节写作、言情章节写作

### 5.4 版本管理

- **自动保存**：写作过程中每 500 字自动保存版本（auto-save）
- **定时保存**：每 60 秒自动保存一次
- **手动快照**：用户可随时保存当前内容快照（manual-save）
- **重写保存**：重写章节时自动保存旧版本（rewrite）
- **版本回滚**：回滚到任意历史版本
- **自动清理**：每个章节保留最近 20 个 auto-save 版本，旧版本自动清理

### 5.5 阅读功能

- **章节列表**：展示所有章节，显示标题和状态
- **阅读正文**：点击章节查看完整内容
- **阅读进度**：标记当前写到第几章

### 5.6 导出功能

- **导出 TXT**：将所有章节导出为单个文本文件
- **导出 EPUB**（可选）：生成电子书格式

### 5.7 设置

- **API 配置**：DeepSeek API Key
- **Prompt 模板管理**：在设置页面内嵌模板管理子页面

---

## 6. User Interface

### 6.1 页面结构

```
┌──────────────────────────────────────────────────┐
│  SukiWindow (SukiUI 主题)                       │
│  ┌────────────────────────────────────────────┐ │
│  │              SukiSideMenu                   │ │
│  │  ┌──────────┬────────────────────────────┐ │ │
│  │  │          │                            │ │ │
│  │  │ 侧边栏   │        主内容区             │ │ │
│  │  │          │                            │ │ │
│  │  │ • 书架   │   (根据选中项显示不同页面)   │ │ │
│  │  │ • 创作   │                            │ │ │
│  │  │ • 设置   │                            │ │ │
│  │  │          │                            │ │ │
│  │  └──────────┴────────────────────────────┘ │ │
│  └────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────┘
```

### 6.2 页面列表

| 页面 | 功能 | 路由 |
|------|------|------|
| 书架 | 小说列表、新建小说（含封面选择） | /bookshelf |
| 创作 | 章节流水线、写作控制、模板选择、版本管理 | /create/{novelId} |
| 设置 | API 配置、Prompt 模板管理 | /settings |

### 6.3 创作页面布局

```
┌─────────────────────────────────────────────────────────┐
│  《小说标题》                   [模板] [版本] [导出]      │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │              章节列表 (左侧)                     │   │
│  │  第一章 意外穿越        [已完成]                 │   │
│  │  第二章 森林探险        [已完成]                │   │
│  │  第三章 遇见女主        [写作中]                │   │
│  │  第四章 ...             [待写]                   │   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │              当前章节内容 (右侧)                  │   │
│  │                                                  │   │
│  │  第三章 遇见女主                                 │   │
│  │  ─────────────────                              │   │
│  │                                                  │   │
│  │  主角继续在森林中前行，突然听到远处传来少女的     │   │
│  │  呼救声。他循声而去，发现一名少女正被魔兽追赶...   │   │
│  │                                                  │   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │  控制面板                                        │   │
│  │  [开始写作]  [停止]  [重写本章]  [保存快照]       │   │
│  │                                                  │   │
│  │  剧情调整指令：________________________________ │   │
│  │  [发送指令]                                      │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

---

## 7. Semantic Kernel Prompt 设计

### 7.1 模板系统

Prompt 模板支持 Mustache 风格占位符，运行时通过 `RenderTemplate` 方法替换变量：

**可用变量：**

| 占位符 | 适用模板类型 | 说明 |
|--------|-------------|------|
| `{{genre}}` | System / Outline / Chapter | 题材 |
| `{{worldSetting}}` | System / Outline / Chapter | 世界观 |
| `{{chapterTitle}}` | Chapter | 章节标题 |
| `{{chapterSummary}}` | Chapter | 章节概要 |
| `{{previousSummary}}` | Chapter | 前文剧情 |
| `{{currentContent}}` | Rewrite | 当前章节内容 |
| `{{instruction}}` | Rewrite | 用户指令 |

### 7.2 系统提示词（默认）

```markdown
你是一个经验丰富的网络小说作家，精通各种网文套路和风格。
你有10年以上的网文创作经验，写过都市、玄幻、悬疑、科幻等多种类型的小说。
你的写作风格：
- 情节紧凑，不拖沓
- 人物刻画鲜明，对话自然
- 善于设置悬念和爽点
- 章节结尾留有钩子，吸引读者继续阅读
```

### 7.3 生成大纲（默认）

```markdown
## 任务
根据以下设定，生成网络小说大纲。

## 输入
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 生成 10-15 章的章节列表
2. 每章需要有 50-100 字的简要描述
3. 确保整体故事有起承转合，高潮迭起
4. 章节标题要吸引人，有网感

## 输出格式
JSON 格式，字段：title（章节标题），summary（章节概要）
```

### 7.4 写作章节（默认）

```markdown
## 任务
根据以下大纲，写出章节正文。

## 输入
- 章节标题：{{chapterTitle}}
- 章节概要：{{chapterSummary}}
- 前文剧情：{{previousSummary}}
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 字数：2000-5000 字
2. 情节紧凑，避免水文
3. 适当加入对话和心理描写
4. 章节结尾留下悬念，吸引读者
5. 注意起承转合，情节要完整

## 输出
直接输出章节正文，不需要额外格式。
```

### 7.5 重写章节（默认）

```markdown
## 任务
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
直接输出修改后的章节正文，不需要额外格式。
```

---

## 8. API Integration

### 8.1 DeepSeek API

```csharp
// NuGet: Microsoft.SemanticKernel
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(
        modelId: "deepseek-chat",
        apiKey: "your-api-key",
        endpoint: new Uri("https://api.deepseek.com"))
    .Build();

// 普通调用
var chatService = kernel.GetRequiredService<IChatCompletionService>();
var result = await chatService.GetChatMessageContentAsync(chatHistory, executionSettings);

// 流式调用（逐 token 推送）
await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings))
{
    var text = chunk.Content;
    if (!string.IsNullOrEmpty(text))
        yield return text;  // 实时推送到 UI
}
```

### 8.2 API 配置

| 配置项 | 值 |
|--------|-----|
| Endpoint | https://api.deepseek.com |
| Model | deepseek-chat |
| Max Tokens | 4096 |
| Temperature | 0.8 |

---

## 9. 数据库

### 9.1 EF Core 配置

```csharp
public class NovelDbContext : DbContext
{
    public DbSet<Novel> Novels => Set<Novel>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();
    public DbSet<PromptTemplate> PromptTemplates => Set<PromptTemplate>();
    public DbSet<ChapterVersion> ChapterVersions => Set<ChapterVersion>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AINovelFlow",
            "ainovelflow.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        options.UseSqlite($"Data Source={dbPath}");
    }
}
```

### 9.2 数据库迁移

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## 10. Project Structure

```
AvaloniaNovel/
├── App.axaml
├── App.axaml.cs
├── Program.cs
├── ViewLocator.cs
│
├── Models/
│   ├── Novel.cs
│   ├── Chapter.cs
│   ├── ChapterStatus.cs
│   ├── ChapterVersion.cs
│   ├── PromptTemplate.cs
│   ├── PromptTemplateType.cs
│   └── AppSettings.cs
│
├── ViewModels/
│   ├── MainWindowViewModel.cs
│   ├── BookshelfViewModel.cs
│   ├── CreateViewModel.cs
│   ├── PromptTemplateViewModel.cs
│   ├── SettingsViewModel.cs
│   ├── Converters.cs
│   ├── IndexToBoolConverter.cs
│   └── ViewModelBase.cs
│
├── Views/
│   ├── MainWindow.axaml
│   ├── MainWindow.axaml.cs
│   ├── BookshelfView.axaml
│   ├── BookshelfView.axaml.cs
│   ├── CreateNovelDialogView.axaml
│   ├── CreateNovelDialogView.axaml.cs
│   ├── CreateView.axaml
│   ├── CreateView.axaml.cs
│   ├── PromptTemplateView.axaml
│   ├── PromptTemplateView.axaml.cs
│   ├── SettingsView.axaml
│   └── SettingsView.axaml.cs
│
├── Services/
│   ├── LLMService.cs
│   ├── StoryService.cs
│   ├── DatabaseService.cs
│   ├── CoverImageService.cs
│   ├── DialogManager.cs
│   ├── ExportService.cs
│   └── DatabaseInitializer.cs
│
├── Data/
│   ├── NovelDbContext.cs
│   └── Migrations/
│
├── Styles/
│   └── GlobalStyles.axaml
│
├── Assets/
│   └── avalonia-logo.ico
│
└── docs/
    └── PRD-AINovelFlow.md
```

---

## 11. Non-Functional Requirements

### 11.1 性能

- 章节生成时间：5-30 秒（取决于章节长度和网络）
- 应用启动时间：< 3 秒
- 数据库查询：< 100ms

### 11.2 可靠性

- API 调用失败时，显示友好错误提示
- 自动保存：每生成 500 字自动保存一次
- 定时保存：每 60 秒自动保存一次
- 版本管理：重写前自动保存旧版本
- 离线模式：API Key 无效时，显示设置引导

### 11.3 兼容性

- Windows 10/11
- .NET 10.0

---

## 12. Milestones

### M1: 基础框架
- [x] 项目搭建（Avalonia + EF Core + SK）
- [x] 数据库配置和迁移
- [x] 基础 UI 框架（SukiSideMenu 导航）

### M2: 书架功能
- [x] 小说 CRUD
- [x] 小说列表展示
- [x] 封面图片选择和存储

### M3: 设置功能
- [x] API Key 配置
- [x] 设置保存和加载

### M4: 创作功能
- [x] 大纲生成（SK + DeepSeek）
- [x] 章节写作
- [x] 流式输出（IAsyncEnumerable + 逐 token 推送）
- [x] Markdown 实时渲染（LiveMarkdown.Avalonia + ObservableStringBuilder）
- [x] 自动滚动（StreamingContentUpdated 事件驱动）
- [x] 停止写作（CancellationTokenSource）
- [x] 进度控制（暂停/继续）

### M5: 干预功能
- [x] 剧情调整指令
- [x] 章节重写
- [x] 手动编辑

### M6: 阅读和导出
- [x] 章节阅读
- [x] TXT 导出

### M7: Prompt 模板系统
- [x] PromptTemplate 数据模型和数据库迁移
- [x] 内置默认模板（8 系统人设 + 4 大纲 + 4 章节）
- [x] 模板 CRUD（创建、编辑、删除、复制）
- [x] 模板类型筛选
- [x] 创作页面模板选择（系统人设、大纲、章节）
- [x] 模板占位符变量替换

### M8: 版本管理
- [x] ChapterVersion 数据模型和数据库迁移
- [x] 自动保存（每 500 字 + 每 60 秒）
- [x] 手动快照保存
- [x] 重写前自动保存旧版本
- [x] 版本回滚
- [x] 旧版本自动清理（保留最近 20 个 auto-save）

---

## 13. Dependencies

```xml
<!-- AvaloniaNovel.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>

  <ItemGroup>
    <!-- Avalonia -->
    <PackageReference Include="Avalonia" Version="11.3.4" />
    <PackageReference Include="Avalonia.Desktop" Version="11.3.4" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.3.4" />
    <PackageReference Include="Avalonia.Fonts.Inter" Version="11.3.4" />

    <!-- SukiUI -->
    <PackageReference Include="SukiUI" Version="6.0.3" />

    <!-- MVVM -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.1" />

    <!-- Markdown 渲染（支持 ObservableStringBuilder 流式输入） -->
    <PackageReference Include="LiveMarkdown.Avalonia" Version="1.9.2" />

    <!-- Database -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.*">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>

    <!-- Semantic Kernel（含流式 API 支持） -->
    <PackageReference Include="Microsoft.SemanticKernel" Version="1.*" />
    <PackageReference Include="Microsoft.SemanticKernel.Connectors.OpenAI" Version="1.*" />
  </ItemGroup>

</Project>
```

---

## 14. Out of Scope（本次不做）

- Epub 导出
- AI 封面生成（图片模型集成）
- 多端同步
- 社区分享
- 语音朗读
- 多语言支持

---

*Document Version: 1.2*
*Created: 2026-04-25*
*Updated: 2026-05-08 — 新增 Prompt 模板系统、版本管理、剧情干预/重写功能；更新数据模型和里程碑*
