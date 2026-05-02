using Avalonia.Controls;
using Avalonia.Input;
using FluentAvalonia.UI.Windowing;
using AgentNovel.ViewModels;

namespace AgentNovel.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnTreeDoubleClick(object? sender, TappedEventArgs e)
    {
        if (DataContext is not MainViewModel mainVm) return;
        if (mainVm.ProjectPage.SelectedChapter is null || mainVm.ProjectPage.SelectedChapter.IsFolder) return;
        if (mainVm.ProjectPage.CurrentProject is null) return;

        if (string.IsNullOrEmpty(mainVm.ProjectPage.CurrentProjectDir)) return;
        mainVm.NavigateToEditor(mainVm.ProjectPage.CurrentProjectDir, mainVm.ProjectPage.SelectedChapter);
    }
}
