using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentNovel.ViewModels;

public partial class AboutViewModel : ViewModelBase
{
    public string AppName { get; } = "FluentAvalonia 演示";
    public string Version { get; } = "1.0.0";
    public string Description { get; } = "展示 FluentAvaloniaUI 控件库的设置面板示例";
}
