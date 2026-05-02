using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentNovel.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _appName = "PDF管理工具";

    [ObservableProperty]
    private string _version = "1.0.0";

    [ObservableProperty]
    private string _description = "简单易用的PDF管理工具，支持合并、拆分和页面操作。";
}
