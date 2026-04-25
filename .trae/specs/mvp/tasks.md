# Tasks - AINovelFlow MVP

## M1: 基础框架

- [ ] Task 1.1: 创建 Avalonia 项目并配置 SukiUI 主题
  - [ ] 创建解决方案和项目结构
  - [ ] 添加 SukiUI 和 CommunityToolkit.Mvvm 包
  - [ ] 配置 App.axaml 使用 SukiTheme
  - [ ] 创建 MainWindow 主窗口框架

- [ ] Task 1.2: 配置 EF Core 数据库
  - [ ] 添加 EF Core SQLite 包
  - [ ] 创建 NovelDbContext
  - [ ] 执行初始迁移

- [ ] Task 1.3: 创建基础导航结构
  - [ ] 创建 SukiSideMenu 侧边栏导航
  - [ ] 创建页面路由框架

## M2: 书架功能

- [ ] Task 2.1: 实现小说模型和服务
  - [ ] 创建 Novel、Chapter 模型
  - [ ] 创建 DatabaseService 数据访问服务

- [ ] Task 2.2: 实现书架页面
  - [ ] 创建 BookshelfView 和 BookshelfViewModel
  - [ ] 实现小说列表展示
  - [ ] 实现新建小说对话框
  - [ ] 实现删除小说功能

## M3: 设置功能

- [ ] Task 3.1: 实现设置页面
  - [ ] 创建 SettingsView 和 SettingsViewModel
  - [ ] 实现 API Key 输入和保存
  - [ ] 实现设置加载和持久化

## M4: 创作功能

- [ ] Task 4.1: 实现 Semantic Kernel 集成
  - [ ] 创建 LLMService 调用 DeepSeek API
  - [ ] 创建 SK Prompt 模板

- [ ] Task 4.2: 实现大纲生成
  - [ ] 创建 StoryService
  - [ ] 实现 OutlinePrompt 生成大纲
  - [ ] 实现章节列表解析和保存

- [ ] Task 4.3: 实现章节写作
  - [ ] 创建 ChapterService
  - [ ] 实现 WritingPrompt 章节写作
  - [ ] 实现开始/暂停控制逻辑
  - [ ] 实现写作进度状态更新

- [ ] Task 4.4: 实现创作页面
  - [ ] 创建 CreateView 和 CreateViewModel
  - [ ] 实现章节列表展示
  - [ ] 实现当前章节内容展示
  - [ ] 实现控制面板（开始/暂停/继续）

## M6: 导出功能

- [ ] Task 6.1: 实现 TXT 导出
  - [ ] 创建 ExportService
  - [ ] 实现章节合并导出为 TXT 文件
  - [ ] 在创作页面添加导出按钮

## Task Dependencies

- Task 1.3 依赖 Task 1.1 和 Task 1.2
- Task 2.1 依赖 Task 1.2
- Task 2.2 依赖 Task 2.1
- Task 3.1 依赖 Task 1.1
- Task 4.1 依赖 Task 3.1
- Task 4.2 依赖 Task 4.1
- Task 4.3 依赖 Task 4.2
- Task 4.4 依赖 Task 4.2 和 Task 4.3
- Task 6.1 依赖 Task 4.4
