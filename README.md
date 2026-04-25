# AI Novel Flow

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.3.4-purple.svg)](https://avaloniaui.net/)
[![SukiUI](https://img.shields.io/badge/SukiUI-6.0.3-orange.svg)](https://github.com/kiki-jiji01/SukiUI)
[![EF Core](https://img.shields.io/badge/EF_Core-8.0-red.svg)](https://learn.microsoft.com/ef/core/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%20|%20macOS%20|%20Linux-lightgrey.svg)](https://avaloniaui.net/)

> 基于 Avalonia + SukiUI 的 AI 小说创作桌面工具，内置 DeepSeek AI 支持，帮你从世界观设定出发，自动生成章节大纲并持续写作，让 AI 替你讲故事。

---

## 目录

- [功能特性](#功能特性)
- [技术栈](#技术栈)
- [项目结构](#项目结构)
- [安装步骤](#安装步骤)
- [使用示例](#使用示例)
- [配置说明](#配置说明)
- [数据存储](#数据存储)
- [贡献指南](#贡献指南)
- [许可证](#许可证)

---

## 功能特性

| 功能 | 描述 |
|------|------|
| 📚 **书架管理** | 创建、浏览、删除小说，展示封面、题材和章节数 |
| ✨ **AI 大纲生成** | 输入题材和世界观，AI 自动生成 10-15 章的故事大纲 |
| 📝 **章节自动写作** | 基于大纲逐章生成 2000-5000 字的正文，章节结尾自带悬念钩子 |
| 🔄 **流式输出** | 写作过程实时逐 token 展示，无需等待全部生成完成 |
| 📊 **Markdown 渲染** | 基于 LiveMarkdown.Avalonia 实时渲染章节内容，支持标题、粗体、列表等格式 |
| ⬇️ **自动滚动** | 流式写作时内容区自动滚动到最新位置，阅读无需手动翻页 |
| ⏹️ **停止写作** | 支持随时中断 AI 写作，已生成内容自动保存 |
| 🎭 **剧情干预** | 随时发送指令调整剧情走向，支持重写章节、手动编辑 |
| 🖼️ **封面生成** | AI 生成小说封面图片 |
| 📖 **阅读模式** | 分章节浏览已生成内容 |
| 💾 **本地存储** | SQLite 数据库自动持久化，数据保存在用户目录 |
| 📤 **TXT 导出** | 一键将所有章节导出为带格式的文本文件 |
| ⚙️ **灵活配置** | 支持自定义 DeepSeek API Key 和模型参数 |

---

## 技术栈

| 层次 | 技术 |
|------|------|
| UI 框架 | [Avalonia UI](https://avaloniaui.net/) 11.3.4 |
| UI 主题组件 | [SukiUI](https://github.com/kiki-jiji01/SukiUI) 6.0.3 |
| Markdown 渲染 | [LiveMarkdown.Avalonia](https://www.nuget.org/packages/LiveMarkdown.Avalonia/) 1.9.2 |
| 架构模式 | MVVM（[CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) 8.4.1） |
| AI 框架 | [Microsoft Semantic Kernel](https://learn.microsoft.com/semantic-kernel/) 1.x |
| AI 后端 | [DeepSeek API](https://platform.deepseek.com/)（OpenAI 兼容协议） |
| 数据库 | SQLite + [Entity Framework Core](https://learn.microsoft.com/ef/core/) 8.0 |
| 运行时 | .NET 10.0 |

---

## 项目结构

```
AvaloniaNovel/
├── App.axaml               # 应用入口（主题、LiveMarkdown 样式）
├── App.axaml.cs            # 应用启动逻辑
├── Program.cs              # 启动入口
│
├── Models/                 # 数据模型
│   ├── Novel.cs            # 小说实体
│   ├── Chapter.cs          # 章节实体（含 ChapterStatus 枚举）
│   └── AppSettings.cs      # 应用设置（API Key 等）
│
├── Views/                  # Avalonia 视图（.axaml + .axaml.cs）
│   ├── MainWindow          # 主窗口（侧边栏导航）
│   ├── BookshelfView       # 书架页面
│   ├── CreateView          # 创作页面（章节流水线 + Markdown 渲染）
│   │   ├── CreateView.axaml       # 布局：章节列表 + MarkdownRenderer
│   │   └── CreateView.axaml.cs    # 流式/非流式 MarkdownBuilder 切换、自动滚动
│   ├── ReadView            # 阅读页面
│   └── SettingsView        # 设置页面
│
├── ViewModels/             # MVVM ViewModel 层
│   ├── MainWindowViewModel.cs
│   ├── BookshelfViewModel.cs
│   ├── CreateViewModel.cs  # 流式写作控制、CancellationToken、StreamingBuilder
│   ├── ReadViewModel.cs
│   └── SettingsViewModel.cs
│
├── Services/               # 业务逻辑层
│   ├── LLMService.cs       # AI 调用（普通 + 流式 Semantic Kernel + DeepSeek）
│   ├── StoryService.cs     # 故事生成（大纲 & 章节写作，支持流式回调）
│   ├── DatabaseService.cs  # 数据库 CRUD
│   └── ExportService.cs    # TXT 导出
│
├── Data/                   # EF Core DbContext
│   └── NovelDbContext.cs
│
├── Migrations/             # EF Core 数据库迁移文件
│
├── docs/
│   └── PRD-AINovelFlow.md  # 产品需求文档
│
└── Assets/
    └── app.ico             # 应用图标
```

---

## 安装步骤

### 环境要求

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)（必须）
- Windows 10/11（推荐）；也支持 macOS / Linux
- DeepSeek API Key（[申请地址](https://platform.deepseek.com/)）

### 方式一：克隆源码运行

```bash
# 1. 克隆仓库
git clone https://github.com/your-username/AvaloniaNovel.git
cd AvaloniaNovel

# 2. 恢复 NuGet 依赖
dotnet restore

# 3. 运行（首次运行会自动初始化数据库）
dotnet run
```

### 方式二：构建发布版本

```bash
# Windows 单文件发布
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# macOS
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true

# Linux
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

发布产物位于 `bin/Release/net10.0/<rid>/publish/` 目录。

### 数据库迁移（开发环境）

首次运行时应用会自动执行迁移。若需手动管理：

```bash
# 安装 EF Core CLI 工具
dotnet tool install --global dotnet-ef

# 添加新迁移
dotnet ef migrations add <MigrationName>

# 应用迁移
dotnet ef database update
```

---

## 使用示例

### 1. 配置 API Key

启动应用后，点击左侧侧边栏的 **设置**，填入 DeepSeek API Key：

```
API Key:  sk-xxxxxxxxxxxxxxxxxxxxxxxx
端点:     https://api.deepseek.com（默认，无需修改）
模型:     deepseek-chat
```

### 2. 创建新小说

1. 点击侧边栏 **书架** → 右上角 **新建**
2. 填写小说信息：

   | 字段 | 示例 |
   |------|------|
   | 标题 | 星际迷途 |
   | 题材 | 科幻 |
   | 世界观 | 2387 年，人类已殖民银河系 300 颗星球，主角是一名在战争中失忆的舰队指挥官 |

3. 点击 **创建** → AI 开始生成章节大纲（约 10 秒）

### 3. 章节写作流水线

大纲生成后，进入 **创作** 页面：

```
章节列表（左侧）          当前章节内容（右侧，Markdown 渲染）
─────────────────         ─────────────────────
第一章 星际漂流 [已完成]   第二章 禁忌星球
第二章 禁忌星球 [写作中]   ────────────────────
第三章 记忆碎片 [待写]    主角驾驶破损的飞船进
...                       入了被封锁的...
                          ✍️ 正在生成...  1234 字
```

**流式写作特性：**
- AI 逐 token 实时推送内容，Markdown 实时渲染
- 内容区自动滚动到最新生成位置
- 状态栏实时显示当前字数
- 点击 **停止** 可随时中断，已生成内容自动保存

**控制面板操作：**

```
[生成大纲]  [开始写作]  [停止]  [导出]

剧情调整指令：让主角在这章发现一段隐藏记忆
                                    [发送指令]
```

### 4. 导出小说

创作完成后，在工具栏点击 **导出**，选择保存路径，即可导出为 `.txt` 文件：

```
星际迷途
==================================================

第一章 星际漂流
------------------------------

（正文内容...）

==================================================

第二章 禁忌星球
...
```

---

## 配置说明

应用设置通过界面操作保存至本地 SQLite 数据库，无需手动编辑配置文件。

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| `DeepSeekApiKey` | DeepSeek 平台 API 密钥 | 空（必填） |
| AI 端点 | API 请求地址 | `https://api.deepseek.com` |
| 模型 | 使用的大模型 | `deepseek-chat` |
| 最大 Token | 单次生成上限 | `4096` |
| Temperature | 创意度（0-1，越高越随机） | `0.8` |

> **提示**：Temperature 建议保持在 0.7-0.9，过高会导致内容逻辑混乱，过低会使输出过于平淡。

### NuGet 依赖

| 包名 | 版本 | 用途 |
|------|------|------|
| Avalonia | 11.3.4 | UI 框架 |
| SukiUI | 6.0.3 | 主题与组件库 |
| CommunityToolkit.Mvvm | 8.4.1 | MVVM 基础设施 |
| Microsoft.SemanticKernel | 1.x | AI 调用框架（含流式支持） |
| Microsoft.SemanticKernel.Connectors.OpenAI | 1.x | OpenAI 兼容协议连接器 |
| LiveMarkdown.Avalonia | 1.9.2 | Markdown 实时渲染（支持 `ObservableStringBuilder` 流式输入） |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0 | SQLite 数据库提供程序 |

---

## 数据存储

应用数据库文件自动存储在系统用户目录：

- **Windows**：`%APPDATA%\AINovelFlow\ainovelflow.db`
- **macOS**：`~/Library/Application Support/AINovelFlow/ainovelflow.db`
- **Linux**：`~/.config/AINovelFlow/ainovelflow.db`

数据库包含以下主要表：

| 表名 | 内容 |
|------|------|
| `Novels` | 小说基本信息（标题、题材、世界观、封面路径） |
| `Chapters` | 章节内容（标题、概要、正文、状态） |
| `AppSettings` | 应用配置（API Key 等） |

---

## 贡献指南

欢迎提交 Issue 和 Pull Request！以下是参与贡献的流程。

### 报告 Bug / 提出功能建议

1. 在 [Issues](https://github.com/your-username/AvaloniaNovel/issues) 页面搜索是否已有相关问题
2. 若没有，新建 Issue，并附上：
   - 问题描述（Bug 请包含复现步骤）
   - 运行环境（OS、.NET 版本）
   - 截图或日志（如有）

### 提交代码

#### 开发环境搭建

```bash
# 1. Fork 并克隆仓库
git clone https://github.com/your-username/AvaloniaNovel.git
cd AvaloniaNovel

# 2. 安装依赖
dotnet restore

# 3. 运行开发版本
dotnet run
```

推荐使用以下 IDE：
- [Visual Studio 2022](https://visualstudio.microsoft.com/)（需安装 Avalonia 扩展）
- [JetBrains Rider](https://www.jetbrains.com/rider/)
- [VS Code](https://code.visualstudio.com/)（需安装 C# 和 Avalonia 插件）

#### 分支命名规范

| 类型 | 格式 | 示例 |
|------|------|------|
| 新功能 | `feat/描述` | `feat/epub-export` |
| Bug 修复 | `fix/描述` | `fix/chapter-save-error` |
| 文档更新 | `docs/描述` | `docs/update-readme` |
| 代码重构 | `refactor/描述` | `refactor/llm-service` |

#### 提交信息规范

遵循 [Conventional Commits](https://www.conventionalcommits.org/) 规范：

```
feat: 添加 EPUB 导出功能
fix: 修复章节保存失败的问题
docs: 更新 README 安装步骤
refactor: 重构 LLMService 错误处理
```

#### Pull Request 流程

1. 从 `main` 分支创建你的功能分支
2. 完成开发后，确保代码可正常构建（`dotnet build`）
3. 提交 PR，描述你的改动内容和动机
4. 等待 Code Review，按反馈进行修改

### 代码风格

- 遵循 C# 标准命名规范（PascalCase 类/方法，camelCase 字段）
- ViewModel 继承 `ObservableObject`，命令使用 `[RelayCommand]` 特性
- 新增服务需在 `Services/` 目录下独立文件
- Avalonia 视图使用 Compiled Bindings（`x:DataType` 已全局启用）

---

## 路线图

- [x] 基础 MVVM 框架搭建
- [x] 书架管理（CRUD）
- [x] DeepSeek API 集成（Semantic Kernel）
- [x] AI 大纲生成
- [x] 章节自动写作
- [x] 流式输出（逐 token 实时推送）
- [x] Markdown 实时渲染（LiveMarkdown.Avalonia）
- [x] 流式写作自动滚动
- [x] 停止写作（CancellationToken 中断）
- [x] TXT 导出
- [ ] 剧情干预 / 章节重写
- [ ] EPUB 导出
- [ ] AI 封面生成（图片模型集成）
- [ ] 自定义 Prompt 模板
- [ ] 写作历史 / 版本对比

---

## 许可证

本项目基于 [MIT License](LICENSE) 开源，欢迎自由使用和修改。

---

<div align="center">

如果这个项目对你有帮助，欢迎 ⭐ Star 支持一下！

</div>
