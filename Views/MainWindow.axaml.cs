using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using AvaloniaNovel.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Controls;

namespace AvaloniaNovel.Views;

public partial class MainWindow : SukiWindow
{
    private SukiSideMenu? _sideMenu;
    private MainWindowViewModel? _vm;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm = DataContext as MainWindowViewModel;
        _sideMenu = this.FindControl<SukiSideMenu>("SideMenu");

        if (_vm != null && _sideMenu != null)
        {
            _vm.PropertyChanged += OnViewModelPropertyChanged;

            var items = _sideMenu.Items.OfType<SukiSideMenuItem>().ToList();
            if (items.Count > 0)
            {
                _sideMenu.SelectedItem = items[0];
            }

            _sideMenu.SelectionChanged += (_, args) =>
            {
                if (args.AddedItems.Count > 0 && args.AddedItems[0] is SukiSideMenuItem menuItem)
                {
                    var index = menuItem.Header?.ToString() switch
                    {
                        "书架" => 0,
                        "创作" => 1,
                        "设置" => 2,
                        _ => 0
                    };
                    _vm.SelectedMenuIndex = index;
                }
            };
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedMenuIndex) && _vm != null && _sideMenu != null)
        {
            var items = _sideMenu.Items.OfType<SukiSideMenuItem>().ToList();
            if (_vm.SelectedMenuIndex >= 0 && _vm.SelectedMenuIndex < items.Count)
            {
                _sideMenu.SelectedItem = items[_vm.SelectedMenuIndex];
            }
        }
    }
}