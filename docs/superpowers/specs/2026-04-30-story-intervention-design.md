# 设计：剧情干预 / 章节重写 / 版本历史 / 自定义模板

**日期：** 2026-04-30
**状态：** 已批准

---

## 1. 概述

为 AINovelFlow 添加以下功能：

1. **剧情干预**：用户通过自然语言指令干预当前章节的剧情走向
2. **章节重写**：AI 根据指令重新生成当前章节内容
3. **版本历史**：实时自动保存写作过程，支持回滚
4. **自定义 Prompt 模板**：用户可创建、编辑、删除自己的写作模板

---

## 2. 数据模型

### 2.1 新增 ChapterVersion 表

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int | 主键 |
| ChapterId | int | 外键，指向 Chapter |
| Content | string | 该版本的正文 |
| WordCount | int | 字数 |
| Trigger | string | 触发来源（"auto-save" / "manual-save" / "rewrite"） |
| CreatedAt | DateTime | 保存时间 |

### 2.2 PromptTemplate 扩展

现有 `PromptTemplate` 模型已存在，本次无需修改数据模型，仅扩展 CRUD 功能。

---

## 3. 版本保存策略

| 触发方式 | Trigger 值 | 说明 |
|----------|------------|------|
| AI 每输出 500 字 | `auto-save` | 自动保存 |
| AI 每 60 秒 | `auto-save` | 定时保存 |
| 用户点击"保存快照" | `manual-save` | 手动保存 |
| 章节重写前 | `rewrite` | 重写前自动保存当前内容 |

**版本覆盖规则：**
- `auto-save`：每个 Chapter 保留最近 20 个自动版本，超出后删除最旧的
- `manual-save`：每个 Chapter 保留所有手动快照，无上限
- `rewrite`：每个 Chapter 只保留最新 1 个 rewrite 版本（覆盖）

---

## 4. 功能详情

### 4.1 剧情干预

**交互流程：**

1. 用户在"指令输入框"输入自然语言指令（如"让主角在这章失忆"）
2. 点击"发送指令"按钮
3. AI 先自动保存当前版本（trigger: `rewrite`）
4. AI 根据当前章节内容 + 指令重新生成章节
5. 新内容替换当前章节，旧内容已在版本历史中

**StoryService 新增方法：**

```csharp
// 剧情干预重写（非流式，用于指令驱动）
public async Task<string> RewriteChapterAsync(
    string currentContent,    // 当前章节正文
    string instruction,       // 用户指令
    string chapterTitle,
    string genre, string worldSetting,
    string? chapterTemplate = null,
    string? systemPrompt = null)
```

### 4.2 章节重写

用户点击"重写本章"按钮，效果等同于：
- 发送指令："重新写这一章，内容方向不变"
- 保存当前版本为 `rewrite`
- AI 重新生成章节

### 4.3 版本历史 UI

```
┌─ 版本历史 ────────────────────────────[折叠]─┐
│  📌 v3  rewrite    2026-04-30 15:23  [回滚] [查看] │
│  📌 v2  auto-save  2026-04-30 15:18  [回滚] [查看] │
│  📌 v1  auto-save  2026-04-30 15:10  [回滚] [查看] │
└──────────────────────────────────────────────────┘
```

- "查看"：弹窗显示该版本内容（只读）
- "回滚"：将章节内容恢复为该版本，确认对话框

### 4.4 自定义 Prompt 模板管理

**模板类型：**
- `System`：系统人设/角色设定
- `Outline`：大纲生成
- `Chapter`：章节写作

**支持的操作：**
- 查看所有模板（按类型分组）
- 新建自定义模板（选择类型 + 输入名称和内容）
- 编辑自定义模板（内置模板仅查看，不可修改）
- 删除自定义模板（仅用户自定义模板可删除）
- 模板内容支持占位符变量：`{{genre}}`, `{{worldSetting}}`, `{{chapterTitle}}`, `{{chapterSummary}}`, `{{previousSummary}}`

---

## 5. 架构改动

| 文件 | 改动 |
|------|------|
| `Models/ChapterVersion.cs` | 新增，ChapterVersion 实体 |
| `Models/Chapter.cs` | 新增导航属性 `Versions` |
| `Models/PromptTemplate.cs` | 已存在，本次不修改 |
| `Data/NovelDbContext.cs` | 新增 `DbSet<ChapterVersion>` |
| `Migrations/` | 新增 ChapterVersion 迁移 |
| `Services/DatabaseService.cs` | 新增 GetVersions/AddVersion/GetLatestVersion/UpdateChapterContent 方法 |
| `Services/StoryService.cs` | 新增 RewriteChapterAsync 方法 |
| `ViewModels/CreateViewModel.cs` | 新增版本历史、干预、重写、回滚命令 |
| `ViewModels/PromptTemplateViewModel.cs` | 新增模板 CRUD 命令 |
| `Views/CreateView.axaml` | 新增剧情干预面板、版本历史面板 UI |
| `Views/PromptTemplateView.axaml` | 新增模板管理界面 |

---

## 6. 页面布局

```
┌─ 创作页 ────────────────────────────────────────────────────┐
│                                                               │
│  [模板 ▼]                                                       │
│                                                               │
│  ┌─ 章节列表 ──┐  ┌─ 内容区 ─────────────────────────────┐    │
│  │ 第1章  ✓   │  │  # 第三章 遇见女主                     │    │
│  │ 第2章  ✓   │  │  正文内容...                           │    │
│  │ 第3章  📝  │  └───────────────────────────────────────┘    │
│  └────────────┘                                               │
│                                                               │
│  ┌─ 剧情干预 ─────────────────────────────────────────────┐  │
│  │  指令：让主角在这章失忆                        [发送]     │  │
│  │                          [停止] [重写本章] [保存快照]    │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌─ 版本历史 ──────────────────────────────── [折叠] ────────┐  │
│  │  v3  rewrite    2026-04-30 15:23   [回滚] [查看]         │  │
│  │  v2  auto-save  2026-04-30 15:18   [回滚] [查看]         │  │
│  │  v1  auto-save  2026-04-30 15:10   [回滚] [查看]         │  │
│  └──────────────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────────────┘
```

---

## 7. 实现顺序

1. 新增 `ChapterVersion` 模型和迁移
2. 扩展 `DatabaseService` 的版本相关方法
3. 扩展 `StoryService` 添加 `RewriteChapterAsync`
4. 扩展 `CreateViewModel` 添加版本历史、干预、重写、回滚命令
5. 更新 `CreateView.axaml` 添加剧情干预面板和版本历史面板
6. 扩展 `PromptTemplateViewModel` 添加模板 CRUD
7. 更新 `PromptTemplateView.axaml` 添加完整模板管理界面

---

## 8. 异常处理

| 场景 | 处理 |
|------|------|
| 重写时网络错误 | 显示错误提示，当前版本已在重写前保存 |
| 回滚时章节被锁定（正在写作） | 禁止回滚，提示先停止写作 |
| 版本历史为空 | 显示"暂无版本历史"占位符 |
| 模板内容为空 | 禁止保存，提示填写内容 |

---

## 9. 验收标准

- [ ] 用户输入指令后，AI 能基于指令调整章节内容
- [ ] 重写前自动保存当前内容到版本历史
- [ ] 版本历史面板显示所有版本，可回滚和查看
- [ ] 自动保存每 500 字或 60 秒触发一次
- [ ] 用户可创建、编辑、删除自定义模板
- [ ] 内置模板不可修改和删除