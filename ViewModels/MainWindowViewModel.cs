using System;
using System.Threading.Tasks;
using AvaloniaNovel.Models;
using AvaloniaNovel.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaNovel.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _selectedMenuIndex;

    public string AppTitle => "AINovelFlow";
    public string AppTagline => "AI 网文创作工具";

    public string ActiveSectionTitle => SelectedMenuIndex switch
    {
        0 => "书架",
        1 => "创作",
        2 => "设置",
        3 => "模板",
        _ => "书架"
    };

    public string ActiveSectionDescription => SelectedMenuIndex switch
    {
        0 => "作品总览",
        1 => "章节工作台",
        2 => "应用设置",
        3 => "提示词管理",
        _ => "作品总览"
    };

    public IDialogManager DialogManager { get; }
    public BookshelfViewModel BookshelfViewModel { get; }
    public CreateViewModel CreateViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }
    public PromptTemplateViewModel PromptTemplateViewModel { get; }

    public MainWindowViewModel()
    {
        DialogManager = new DialogManager();
        BookshelfViewModel = new BookshelfViewModel(DialogManager);
        CreateViewModel = new CreateViewModel();
        SettingsViewModel = new SettingsViewModel();
        PromptTemplateViewModel = new PromptTemplateViewModel();

        BookshelfViewModel.NovelOpened += OnNovelOpened;

        OnPropertyChanged(nameof(SelectedMenuIndex));
        OnPropertyChanged(nameof(ActiveSectionTitle));
        OnPropertyChanged(nameof(ActiveSectionDescription));
    }

    private async void OnNovelOpened(object? sender, Novel novel)
    {
        await CreateViewModel.LoadNovelAsync(novel);
        SelectedMenuIndex = 1;
    }

    partial void OnSelectedMenuIndexChanged(int value)
    {
        OnPropertyChanged(nameof(ActiveSectionTitle));
        OnPropertyChanged(nameof(ActiveSectionDescription));
    }
}
