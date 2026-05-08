using System.ComponentModel;
using Avalonia.Controls;
using AvaloniaNovel.ViewModels;

namespace AvaloniaNovel.Views;

public partial class MainWindow : Window
{
    private ListBox? _navListBox;
    private MainWindowViewModel? _vm;
    private Control[]? _pages;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm = DataContext as MainWindowViewModel;
        _navListBox = this.FindControl<ListBox>("NavListBox");

        _pages =
        [
            this.FindControl<Control>("BookshelfPage")!,
            this.FindControl<Control>("CreatePage")!,
            this.FindControl<Control>("SettingsPage")!,
            this.FindControl<Control>("TemplatePage")!,
        ];

        if (_vm != null && _navListBox != null)
        {
            _vm.PropertyChanged += OnViewModelPropertyChanged;
            _navListBox.SelectionChanged += OnNavSelectionChanged;
        }
    }

    private void OnNavSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_navListBox is { SelectedIndex: >= 0 } && _vm != null)
        {
            _vm.SelectedMenuIndex = _navListBox.SelectedIndex;
            ShowPage(_navListBox.SelectedIndex);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedMenuIndex) && _vm != null && _navListBox != null)
        {
            if (_vm.SelectedMenuIndex >= 0 && _vm.SelectedMenuIndex < _navListBox.Items.Count)
            {
                _navListBox.SelectedIndex = _vm.SelectedMenuIndex;
            }
            ShowPage(_vm.SelectedMenuIndex);
        }
    }

    private void ShowPage(int index)
    {
        if (_pages == null) return;
        for (var i = 0; i < _pages.Length; i++)
        {
            _pages[i].IsVisible = i == index;
        }
    }
}
