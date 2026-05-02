using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using AgentNovel.ViewModels;
using AgentNovel.Views;
using Avalonia.Platform.Storage;

namespace AgentNovel;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var settingsVm = new SettingsViewModel();
            var projectVm = new ProjectViewModel();
            var editorVm = new EditorViewModel();
            var mainVm = new MainViewModel(settingsVm, projectVm, editorVm);
            
            DataContext = mainVm;
            
            var mainWindow = new MainWindow
            {
                DataContext = mainVm,
            };

            projectVm.OpenFolderDialog = async () =>
            {
                var storageProvider = mainWindow.StorageProvider;
                var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "选择项目文件夹",
                    AllowMultiple = false
                });

                return folders.Count > 0 ? folders[0].Path.LocalPath : null;
            };

            desktop.MainWindow = mainWindow;

            await projectVm.TryLoadLastProjectAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
