using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentNovel.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _enableMicaEffect;

    [ObservableProperty]
    private bool _showStatusBar = true;

    [ObservableProperty]
    private bool _compactMode;

    [ObservableProperty]
    private int _maxRecentFiles = 10;

    [ObservableProperty]
    private double _animationSpeed = 200;

    [ObservableProperty]
    private string _selectedTheme = "System";

    [ObservableProperty]
    private string _selectedAccentColor = "Default";

    public string[] AvailableThemes { get; } =
    [
        "Light",
        "Dark",
        "System"
    ];

    public string[] AccentColors { get; } =
    [
        "Default", "Red", "Orange", "Yellow", "Green", "Blue", "Purple"
    ];

    partial void OnSelectedThemeChanged(string? oldValue, string newValue)
    {
        if (Avalonia.Application.Current == null) return;

        Avalonia.Application.Current.RequestedThemeVariant = newValue switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }
}
