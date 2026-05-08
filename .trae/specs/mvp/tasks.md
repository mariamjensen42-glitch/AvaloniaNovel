# Tasks - AINovelFlow MVP

## M1: 基础框架

- [x] Task 1.1: 创建 Avalonia 项目并配置 SukiUI 主题
  - [x] 创建解决方案和项目结构
  - [x] 添加 SukiUI 和 CommunityToolkit.Mvvm 包
  - [x] 配置 App.axaml 使用 SukiTheme
  - [x] 创建 MainWindow 主窗口框架

- [x] Task 1.2: 配置 EF Core 数据库
  - [x] 添加 EF Core SQLite 包
  - [x] 创建 NovelDbContext
  - [x] 执行初始迁移

- [x] Task 1.3: 创建基础导航结构
  - [x] 创建 SukiSideMenu 侧边栏导航
  - [x] 创建页面路由框架

## M2: 书架功能

- [x] Task 2.1: 实现小说模型和服务
  - [x] 创建 Novel、Chapter 模型
  - [x] 创建 DatabaseService 数据访问服务

- [x] Task 2.2: 实现书架页面
  - [x] 创建 BookshelfView 和 BookshelfViewModel
  - [x] 实现小说列表展示
  - [x] 实现新建小说对话框（含封面选择）
  - [x] 实现删除小说功能

## M3: 设置功能

- [x] Task 3.1: 实现设置页面
  - [x] 创建 SettingsView 和 SettingsViewModel
  - [x] 实现 API Key 输入和保存
  - [x] 实现设置加载和持久化

## M4: 创作功能

- [x] Task 4.1: 实现 Semantic Kernel 集成
  - [x] 创建 LLMService 调用 DeepSeek API
  - [x] 创建 SK Prompt 模板

- [x] Task 4.2: 实现大纲生成
  - [x] 创建 StoryService
  - [x] 实现 OutlinePrompt 生成大纲
  - [x] 实现章节列表解析和保存

- [x] Task 4.3: 实现章节写作
  - [x] 实现流式章节写作
  - [x] 实现开始/暂停控制逻辑
  - [x] 实现写作进度状态更新

- [x] Task 4.4: 实现创作页面
  - [x] 创建 CreateView 和 CreateViewModel
  - [x] 实现章节列表展示
  - [x] 实现当前章节内容展示
  - [x] 实现控制面板（开始/暂停/继续）

## M5: 干预功能

- [x] Task 5.1: 实现剧情干预
  - [x] 实现剧情调整指令输入和发送
  - [x] 实现 RewriteChapterAsync 重写章节
  - [x] 实现重写前自动保存旧版本

- [x] Task 5.2: 实现章节重写
  - [x] 实现一键重写本章
  - [x] 实现重写 Prompt 模板

## M6: 导出功能

- [x] Task 6.1: 实现 TXT 导出
  - [x] 创建 ExportService
  - [x] 实现章节合并导出为 TXT 文件
  - [x] 在创作页面添加导出按钮

## M7: Prompt 模板系统

- [x] Task 7.1: 实现模板数据模型
  - [x] 创建 PromptTemplate 和 PromptTemplateType 模型
  - [x] 执行数据库迁移
  - [x] 实现内置默认模板初始化

- [x] Task 7.2: 实现模板管理页面
  - [x] 创建 PromptTemplateView 和 PromptTemplateViewModel
  - [x] 实现模板列表展示和类型筛选
  - [x] 实现模板 CRUD（创建、编辑、删除、复制）

- [x] Task 7.3: 实现模板选择
  - [x] 在 CreateViewModel 中添加模板选择
  - [x] 实现系统人设、大纲、章节模板选择
  - [x] 实现模板占位符变量替换

## M8: 版本管理

- [x] Task 8.1: 实现版本数据模型
  - [x] 创建 ChapterVersion 模型
  - [x] 执行数据库迁移
  - [x] 在 DatabaseService 中添加版本 CRUD

- [x] Task 8.2: 实现版本保存
  - [x] 实现自动保存（每 500 字）
  - [x] 实现定时保存（每 60 秒）
  - [x] 实现手动快照保存
  - [x] 实现重写前自动保存
  - [x] 实现旧版本自动清理

- [x] Task 8.3: 实现版本回滚
  - [x] 实现版本列表展示
  - [x] 实现版本回滚功能

## Task Dependencies

- Task 1.3 依赖 Task 1.1 和 Task 1.2
- Task 2.1 依赖 Task 1.2
- Task 2.2 依赖 Task 2.1
- Task 3.1 依赖 Task 1.1
- Task 4.1 依赖 Task 3.1
- Task 4.2 依赖 Task 4.1
- Task 4.3 依赖 Task 4.2
- Task 4.4 依赖 Task 4.2 和 Task 4.3
- Task 5.1 依赖 Task 4.4
- Task 5.2 依赖 Task 5.1
- Task 6.1 依赖 Task 4.4
- Task 7.1 依赖 Task 1.2
- Task 7.2 依赖 Task 7.1
- Task 7.3 依赖 Task 7.2 和 Task 4.4
- Task 8.1 依赖 Task 1.2
- Task 8.2 依赖 Task 8.1 和 Task 4.3
- Task 8.3 依赖 Task 8.2
