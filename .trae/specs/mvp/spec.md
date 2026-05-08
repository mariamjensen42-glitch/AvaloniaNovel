# AINovelFlow MVP Spec

## Why

AINovelFlow 需要一个最小可行产品版本，用于验证核心创作流程。用户需要能够创建小说、管理章节，并通过 AI 自动生成章节内容，同时支持自定义 Prompt 模板和版本管理。

## What Changes

MVP 版本实现以下核心功能：

- 项目基础框架搭建（Avalonia + EF Core + SukiUI）
- 数据库配置和迁移
- 小说 CRUD 操作（创建、列表、删除、打开，含封面图片）
- 设置页面（DeepSeek API Key 配置）
- 章节流水线基础功能（生成大纲、AI 写作）
- 章节列表展示和状态管理
- 剧情干预和章节重写
- 自定义 Prompt 模板管理
- 章节版本管理（自动保存、手动快照、版本回滚）
- TXT 文本导出

## Impact

- 新增规格文档：`spec.md`（本文档）
- 受影响的功能范围：M1-M8
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
- **THEN** 弹出对话框输入标题、选择题材、填写世界观设定、选择封面图片
- **AND** 保存后小说出现在列表中

#### Scenario: 删除小说
- **WHEN** 用户选中小说并点击删除
- **THEN** 弹出确认对话框
- **AND** 确认后小说从数据库和列表中移除
- **AND** 同时删除关联的封面图片

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
- **AND** 支持选择不同的大纲生成模板

### Requirement: 章节写作

系统 SHALL 提供 AI 自动写作功能，用户可控制开始/暂停

#### Scenario: 开始写作
- **WHEN** 用户点击"开始写作"
- **THEN** AI 从第一章开始逐章生成正文
- **AND** 每章状态更新为"已完成"
- **AND** 支持选择系统人设模板和章节写作模板

#### Scenario: 暂停写作
- **WHEN** 用户点击"暂停"
- **THEN** 当前章节写作完成后停止
- **AND** 状态保持不变，用户可继续

### Requirement: 剧情干预

系统 SHALL 提供剧情调整和章节重写功能

#### Scenario: 发送剧情指令
- **WHEN** 用户输入剧情调整指令并点击"发送指令"
- **THEN** AI 根据指令重写当前章节
- **AND** 重写前自动保存旧版本

#### Scenario: 重写章节
- **WHEN** 用户点击"重写本章"
- **THEN** AI 重新生成当前章节内容
- **AND** 重写前自动保存旧版本

### Requirement: Prompt 模板管理

系统 SHALL 提供自定义 Prompt 模板的管理功能

#### Scenario: 查看模板列表
- **WHEN** 用户在设置页面查看模板管理
- **THEN** 显示所有模板，支持按类型筛选

#### Scenario: 创建自定义模板
- **WHEN** 用户点击"新建模板"
- **THEN** 可输入模板名称、选择类型、编辑模板内容
- **AND** 模板内容支持占位符变量

#### Scenario: 删除自定义模板
- **WHEN** 用户删除非内置模板
- **THEN** 模板从数据库中移除
- **AND** 内置模板不可删除

### Requirement: 版本管理

系统 SHALL 提供章节版本管理功能

#### Scenario: 自动保存版本
- **WHEN** 写作过程中每生成 500 字或每 60 秒
- **THEN** 自动保存当前内容为 auto-save 版本

#### Scenario: 手动保存快照
- **WHEN** 用户点击"保存快照"
- **THEN** 保存当前内容为 manual-save 版本

#### Scenario: 版本回滚
- **WHEN** 用户选择某个历史版本并回滚
- **THEN** 章节内容恢复为该版本的内容

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
| CoverImagePath | string | 封面图片路径 |
| HasCoverImage | bool | 是否有封面（计算属性） |
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
    Revised = 3       // 已修订
}
```

### ChapterVersion（章节版本）

| Field | Type | Description |
|-------|------|-------------|
| Id | int | 主键 |
| ChapterId | int | 所属章节外键 |
| Content | string | 版本内容 |
| WordCount | int | 字数 |
| Trigger | string | 触发类型（auto-save / manual-save / rewrite） |
| CreatedAt | DateTime | 创建时间 |

### PromptTemplate（提示词模板）

| Field | Type | Description |
|-------|------|-------------|
| Id | int | 主键 |
| Name | string | 模板名称 |
| Type | PromptTemplateType | 模板类型 |
| Content | string | 模板内容（支持占位符变量） |
| Variables | string | 变量说明 |
| IsBuiltIn | bool | 是否为内置默认模板 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime | 更新时间 |

### PromptTemplateType（枚举）

```csharp
public enum PromptTemplateType
{
    System = 0,       // 系统人设
    Outline = 1,      // 大纲生成
    Chapter = 2       // 章节写作
}
```

### AppSettings（应用设置）

| Field | Type | Description |
|-------|------|-------------|
| Id | int | 主键 |
| DeepSeekApiKey | string | API 密钥 |

## Architecture

```
AvaloniaNovel/
├── Views/           # Avalonia .axaml 页面
├── ViewModels/      # CommunityToolkit.Mvvm ViewModels
├── Services/        # LLMService, StoryService, DatabaseService, CoverImageService, DialogManager, ExportService
├── Models/          # Novel, Chapter, ChapterVersion, PromptTemplate, AppSettings
├── Data/            # NovelDbContext
├── Styles/          # GlobalStyles.axaml
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
- AI 封面生成（图片模型集成）
- 多端同步
- 社区分享
- 语音朗读
- 多语言支持
