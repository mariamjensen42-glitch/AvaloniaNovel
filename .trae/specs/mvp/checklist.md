# Checklist - AINovelFlow MVP

## M1: 基础框架

- [x] 项目使用 Avalonia UI 11.3.4 创建
- [x] App.axaml 正确引用 SukiTheme
- [x] MainWindow 使用 SukiSideMenu 实现侧边栏导航
- [x] NovelDbContext 配置 SQLite 数据库
- [x] 初始迁移成功执行

## M2: 书架功能

- [x] Novel 模型包含 Id、Title、Genre、WorldSetting、CoverImagePath、CreatedAt、UpdatedAt 字段
- [x] Chapter 模型包含 Id、NovelId、Order、Title、Summary、Content、Status、CreatedAt 字段
- [x] BookshelfView 展示小说列表
- [x] 新建小说对话框可输入标题、选择题材、填写世界观、选择封面图片
- [x] 删除小说前显示确认对话框
- [x] 删除小说时同时删除封面图片
- [x] 数据持久化到 SQLite

## M3: 设置功能

- [x] SettingsView 包含 API Key 输入框
- [x] API Key 保存到数据库
- [x] 启动时自动加载已保存的 API Key
- [x] 设置页面内嵌 Prompt 模板管理

## M4: 创作功能

- [x] LLMService 正确调用 DeepSeek API
- [x] 大纲生成返回 10-15 章的章节列表
- [x] 每章包含标题和概要
- [x] 章节写作生成 2000-5000 字正文
- [x] 开始写作从第一章开始
- [x] 暂停写作在当前章节完成后停止
- [x] 章节状态正确更新（Writing -> Completed）
- [x] CreateView 左侧显示章节列表
- [x] CreateView 右侧显示当前章节内容
- [x] 控制面板包含开始/暂停按钮
- [x] 支持选择系统人设、大纲、章节模板

## M5: 干预功能

- [x] 剧情调整指令输入框可用
- [x] 发送指令后 AI 根据指令重写章节
- [x] 重写前自动保存旧版本
- [x] 一键重写本章功能可用
- [x] 重写使用 RewriteChapterAsync 和重写模板

## M6: 导出功能

- [x] 导出按钮在创作页面可用
- [x] 导出的 TXT 文件包含所有章节内容
- [x] 文件保存到用户选择的位置

## M7: Prompt 模板系统

- [x] PromptTemplate 模型包含 Id、Name、Type、Content、Variables、IsBuiltIn、CreatedAt、UpdatedAt 字段
- [x] PromptTemplateType 枚举包含 System、Outline、Chapter 三种类型
- [x] 内置默认模板初始化（8 系统人设 + 4 大纲 + 4 章节）
- [x] PromptTemplateView 展示模板列表
- [x] 支持按类型筛选模板
- [x] 创建自定义模板功能可用
- [x] 编辑模板功能可用
- [x] 删除自定义模板功能可用（内置模板不可删除）
- [x] 复制模板功能可用
- [x] 创作页面可选择系统人设模板
- [x] 创作页面可选择大纲生成模板
- [x] 创作页面可选择章节写作模板
- [x] 模板占位符变量正确替换

## M8: 版本管理

- [x] ChapterVersion 模型包含 Id、ChapterId、Content、WordCount、Trigger、CreatedAt 字段
- [x] 写作过程中每 500 字自动保存版本
- [x] 每 60 秒定时自动保存
- [x] 手动保存快照功能可用
- [x] 重写章节前自动保存旧版本
- [x] 版本列表展示可用
- [x] 版本回滚功能可用
- [x] 旧版本自动清理（保留最近 20 个 auto-save）
