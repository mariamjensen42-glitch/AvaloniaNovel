# AI Novel Flow

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.3.4- purple.svg)](https://avaloniaui.net/)
[![SukiUI](https://img.shields.io/badge/SukiUI-6.0.3-orange.svg)](https://github.com/kiki-jiji01/SukiUI)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%20|%20macOS%20|%20Linux-lightgrey.svg)](https://avaloniaui.net/)

基于 Avalonia + SukiUI 的 AI 小说创作工具，支持通过 AI 辅助生成小说章节。

## 技术栈

- **框架**: Avalonia 11.3.4 (.NET 10.0)
- **UI 组件**: SukiUI 6.0.3
- **ORM**: Entity Framework Core 8.0 + SQLite
- **AI 集成**: Microsoft Semantic Kernel (OpenAI)
- **MVVM**: CommunityToolkit.Mvvm 8.4.1

## 项目结构

```
AvaloniaNovel/
├── Models/          # 数据模型
│   ├── Novel.cs     # 小说实体
│   ├── Chapter.cs   # 章节实体
│   └── AppSettings.cs
├── Views/           # Avalonia 视图
│   ├── MainWindow   # 主窗口
│   ├── BookshelfView   # 书架视图
│   ├── CreateView   # 创建小说视图
│   └── SettingsView # 设置视图
├── ViewModels/      # MVVM ViewModels
├── Services/        # 业务逻辑
│   ├── LLMService.cs      # AI 服务
│   ├── DatabaseService.cs # 数据库服务
│   ├── StoryService.cs    # 故事生成逻辑
│   └── ExportService.cs   # 导出服务
├── Data/            # EF Core DbContext
└── Migrations/      # 数据库迁移

## 功能特性

- 📚 **书架管理** - 小说列表、封面、状态追踪
- ✨ **AI 生成** - 基于设定和大纲自动生成章节
- 🖼️ **封面生成** - AI 生成小说封面图片
- 📖 **阅读模式** - 分章节浏览已生成内容
- 💾 **本地存储** - SQLite 数据库持久化
- ⚙️ **灵活配置** - 支持 OpenAI API 自定义配置

## 快速开始

### 环境要求

- .NET 10.0 SDK
- Windows 10+ / macOS / Linux

### 运行项目

```bash
cd AvaloniaNovel
dotnet run
```

### 发布

```bash
dotnet publish -c Release
```

## 配置

在设置界面配置 OpenAI API Key 和模型参数。

## 许可证

MIT
