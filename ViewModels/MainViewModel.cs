using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentNovel.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? _currentPage;

    [ObservableProperty]
    private bool _isProjectView = true;

    [ObservableProperty]
    private bool _isSettingsView;

    [ObservableProperty]
    private bool _isAboutView;

    public MainViewModel(SettingsViewModel settingsViewModel, ProjectViewModel projectViewModel, EditorViewModel editorViewModel)
    {
        SettingsPage = settingsViewModel;
        ProjectPage = projectViewModel;
        EditorPage = editorViewModel;
        AboutPage = new AboutViewModel();
        CurrentPage = EditorPage;
    }

    public SettingsViewModel SettingsPage { get; }
    public ProjectViewModel ProjectPage { get; }
    public EditorViewModel EditorPage { get; }
    public AboutViewModel AboutPage { get; }

    partial void OnIsProjectViewChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            IsSettingsView = false;
            IsAboutView = false;
            CurrentPage = EditorPage;
        }
        else if (!IsSettingsView && !IsAboutView)
        {
            IsProjectView = true;
        }
    }

    partial void OnIsSettingsViewChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            IsProjectView = false;
            IsAboutView = false;
            CurrentPage = SettingsPage;
        }
        else if (!IsProjectView && !IsAboutView)
        {
            IsSettingsView = true;
        }
    }

    partial void OnIsAboutViewChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            IsProjectView = false;
            IsSettingsView = false;
            CurrentPage = AboutPage;
        }
        else if (!IsProjectView && !IsSettingsView)
        {
            IsAboutView = true;
        }
    }

    public void NavigateToEditor(string projectDir, Models.ChapterNode chapter)
    {
        EditorPage.LoadChapter(projectDir, chapter);
        CurrentPage = EditorPage;
        IsProjectView = true;
    }
}
