using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaNovel.Services;

namespace AvaloniaNovel.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IDatabaseService _dbService;

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>提示词模板管理子 ViewModel</summary>
    public PromptTemplateViewModel PromptTemplateViewModel { get; }

    public SettingsViewModel(IDatabaseService dbService, PromptTemplateViewModel promptTemplateViewModel)
    {
        _dbService = dbService;
        PromptTemplateViewModel = promptTemplateViewModel;
        
        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        var settings = await _dbService.GetAppSettingsAsync();
        if (settings != null)
        {
            ApiKey = settings.DeepSeekApiKey;
        }
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        IsSaving = true;
        StatusMessage = string.Empty;
        try
        {
            await _dbService.SaveAppSettingsAsync(ApiKey);
            StatusMessage = "设置已保存";
        }
        catch
        {
            StatusMessage = "保存失败";
        }
        finally
        {
            IsSaving = false;
        }
    }
}
