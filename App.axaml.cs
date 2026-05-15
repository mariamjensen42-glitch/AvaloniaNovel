using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaNovel.Services;
using AvaloniaNovel.ViewModels;
using AvaloniaNovel.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaNovel;

public partial class App : Application
{
    public static ServiceProvider ServiceProvider { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DatabaseInitializer.Initialize();

        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<ICoverImageService, CoverImageService>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<ILLMService, LLMService>();
        services.AddSingleton<IStoryService, StoryService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddSingleton<IDialogManager, DialogManager>();

        // ViewModels
        services.AddSingleton<BookshelfViewModel>();
        services.AddSingleton<CreateViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<PromptTemplateViewModel>();
        services.AddSingleton<MainWindowViewModel>();
    }
}
