# AgentNovel 项目规范

## 技术栈
- Avalonia UI 11.3, FluentAvaloniaUI 2.5, CommunityToolkit.Mvvm 8.4, .NET 10

## MVVM
- ViewModel 继承 ViewModelBase，用 `[ObservableProperty]`/`[RelayCommand]` 生成代码
- 异步命令返回 `Task`，属性变更回调用 `partial void OnXxxChanged`
- XAML 必须设 `x:DataType`，窗口用 `AppWindow`，页面用 `UserControl`

## XAML
- 用 `Grid` 复杂布局，`StackPanel` 简单线性排列
- 边距用 4 的倍数，字体 14/20/28px
- 颜色用主题资源如 `{DynamicResource CardStrokeColorDefaultBrush}`，禁止硬编码

## 窗口
- `AppWindow` + `TransparencyLevelHint="Mica"` + `Background="Transparent"`
- 内容扩展标题栏：`TitleBar.ExtendsContentIntoTitleBar = true`

## 数据与服务
- 集合用 `ObservableCollection<T>`，路径用 `Path.Combine`
- 服务纯逻辑无 UI 依赖，异步方法以 Async 结尾，JSON 用 `System.Text.Json`

## 禁止
- ViewModel 引用 Avalonia UI 类型
- `async void`（事件处理器除外）
- 手写 `INotifyPropertyChanged`
- 提交 bin/obj 变更
