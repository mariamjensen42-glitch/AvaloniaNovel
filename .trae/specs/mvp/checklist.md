# Checklist - AINovelFlow MVP

## M1: 基础框架

- [x] 项目使用 Avalonia UI 11.3.4 创建
- [x] App.axaml 正确引用 SukiTheme
- [x] MainWindow 使用 SukiSideMenu 实现侧边栏导航
- [x] NovelDbContext 配置 SQLite 数据库
- [x] 初始迁移成功执行

## M2: 书架功能

- [x] Novel 模型包含 Id、Title、Genre、WorldSetting、CreatedAt、UpdatedAt 字段
- [x] Chapter 模型包含 Id、NovelId、Order、Title、Summary、Content、Status、CreatedAt 字段
- [x] BookshelfView 展示小说列表
- [x] 新建小说对话框可输入标题、选择题材、填写世界观
- [x] 删除小说前显示确认对话框
- [x] 数据持久化到 SQLite

## M3: 设置功能

- [x] SettingsView 包含 API Key 输入框
- [x] API Key 保存到数据库
- [x] 启动时自动加载已保存的 API Key

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

## M6: 导出功能

- [x] 导出按钮在创作页面可用
- [x] 导出的 TXT 文件包含所有章节内容
- [x] 文件保存到用户选择的位置
