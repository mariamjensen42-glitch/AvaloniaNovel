# AINovelFlow MVP Spec

## Why

AINovelFlow 需要一个最小可行产品版本，用于验证核心创作流程。用户需要能够创建小说、管理章节，并通过 AI 自动生成章节内容。

## What Changes

MVP 版本实现以下核心功能：

- 项目基础框架搭建（Avalonia + EF Core + SukiUI）
- 数据库配置和迁移
- 小说 CRUD 操作（创建、列表、删除、打开）
- 设置页面（DeepSeek API Key 配置）
- 章节流水线基础功能（生成大纲、AI 写作）
- 章节列表展示和状态管理
- TXT 文本导出

## Impact

- 新增规格文档：`spec.md`（本文档）
- 受影响的功能范围：M1、M2、M3、M4（部分）、M6（部分）
- 不包含：Epub 导出、多语言支持、AI 配图等高级功能

## ADDED Requirements

### Requirement: 项目基础框架

系统 SHALL 提供完整的 Avalonia + EF Core + SukiUI 项目结构

#### Scenario: 项目启动
- **WHEN** 用户运行应用程序
- **THEN** 显示 SukiUI 主题的主窗口，包含侧边栏导航

### Requirement: 小说管理

系统 SHALL 提供小说的创建、列表展示、删除功能

#### Scenario: 创建小说
- **WHEN** 用户在书架页面点击"新建小说"
- **THEN** 弹出对话框输入标题、选择题材、填写世界观设定
- **AND** 保存后小说出现在列表中

#### Scenario: 删除小说
- **WHEN** 用户选中小说并点击删除
- **THEN** 弹出确认对话框
- **AND** 确认后小说从数据库和列表中移除

### Requirement: 设置管理

系统 SHALL 提供 DeepSeek API Key 的配置和持久化

#### Scenario: 保存 API Key
- **WHEN** 用户在设置页面输入 API Key
- **THEN** 配置保存到数据库
- **AND** 下次启动时自动加载

### Requirement: 大纲生成

系统 SHALL 提供基于题材和世界观生成小说大纲的功能

#### Scenario: 生成大纲
- **WHEN** 用户创建新小说后点击"生成大纲"
- **THEN** 调用 DeepSeek API 生成 10-15 章的章节列表
- **AND** 每章包含标题和 50-100 字的概要

### Requirement: 章节写作

系统 SHALL 提供 AI 自动写作功能，用户可控制开始/暂停

#### Scenario: 开始写作
- **WHEN** 用户点击"开始写作"
- **THEN** AI 从第一章开始逐章生成正文
- **AND** 每章状态更新为"已完成"

#### Scenario: 暂停写作
- **WHEN** 用户点击"暂停"
- **THEN** 当前章节写作完成后停止
- **AND** 状态保持不变，用户可继续

### Requirement: 导出功能

系统 SHALL 提供 TXT 格式的文本导出

#### Scenario: 导出 TXT
- **WHEN** 用户在创作页面点击"导出"
- **THEN** 所有章节内容合并为单个 TXT 文件
- **AND** 保存到用户选择的位置

## Data Model

### Novel（小说）

| Field | Type | Description |
|-------|------|-------------|
| Id | int | 主键 |
| Title | string | 小说标题 |
| Genre | string | 题材类型 |
| WorldSetting | string | 世界观设定 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime | 更新时间 |

### Chapter（章节）

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

### ChapterStatus（枚举）

```csharp
public enum ChapterStatus
{
    Outline = 0,     // 仅有大纲
    Writing = 1,      // 写作中
    Completed = 2,    // 已完成
}
```

### AppSettings（应用设置）

| Field | Type | Description |
|-------|------|-------------|
| Id | int | 主键 |
| DeepSeekApiKey | string | API 密钥 |

## Architecture

```
AINovelFlow/
├── Views/           # Avalonia .axaml 页面
├── ViewModels/      # CommunityToolkit.Mvvm ViewModels
├── Services/        # LLMService, StoryService, DatabaseService
├── Models/          # Novel, Chapter, AppSettings
├── Data/            # NovelDbContext
└── Assets/          # 资源文件
```

## 技术栈

| Layer | Technology |
|-------|------------|
| UI Framework | Avalonia UI 11.3.4 |
| MVVM | CommunityToolkit.Mvvm 8.4.1 |
| AI Framework | Semantic Kernel |
| AI Backend | DeepSeek API |
| Database | SQLite + Entity Framework Core |
| Theme | SukiUI 6.0.3 |
| Target Framework | .NET 10.0 |

## Out of Scope（本期不做）

- Epub 导出
- 剧情调整指令
- 章节重写
- 手动编辑章节
- 多端同步
- 社区分享
- AI 配图
- 语音朗读
- 多语言支持
