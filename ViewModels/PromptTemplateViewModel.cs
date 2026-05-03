using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaNovel.Models;
using AvaloniaNovel.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaNovel.ViewModels;

public sealed record FilterOption(string Label, PromptTemplateType? Type);

public partial class PromptTemplateViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;

    [ObservableProperty]
    private ObservableCollection<PromptTemplate> _templates = new();

    [ObservableProperty]
    private PromptTemplate? _selectedTemplate;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private PromptTemplateType _editType = PromptTemplateType.System;

    [ObservableProperty]
    private string _editContent = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public PromptTemplateType[] TemplateTypes { get; } =
        Enum.GetValues<PromptTemplateType>();

    public string TypeDisplayName(PromptTemplateType type) => type switch
    {
        PromptTemplateType.System => "系统人设",
        PromptTemplateType.Outline => "大纲生成",
        PromptTemplateType.Chapter => "章节写作",
        _ => type.ToString()
    };

    public List<FilterOption> FilterOptions { get; } = new()
    {
        new("全部", null),
        new("系统人设", PromptTemplateType.System),
        new("大纲生成", PromptTemplateType.Outline),
        new("章节写作", PromptTemplateType.Chapter)
    };

    [ObservableProperty]
    private FilterOption? _selectedFilterOption;

    [ObservableProperty]
    private PromptTemplateType? _activeFilter;

    public bool IsFilterAll => !ActiveFilter.HasValue;
    public bool IsFilterSystem => ActiveFilter == PromptTemplateType.System;
    public bool IsFilterOutline => ActiveFilter == PromptTemplateType.Outline;
    public bool IsFilterChapter => ActiveFilter == PromptTemplateType.Chapter;

    public bool IsDetailEmpty => SelectedTemplate == null;
    public bool IsDetailViewing => SelectedTemplate != null && !IsEditing;
    public bool IsDetailEditing => IsEditing;

    public PromptTemplateViewModel()
    {
        _dbService = new DatabaseService();
        SelectedFilterOption = FilterOptions[0];
        _ = LoadTemplatesAsync();
    }

    partial void OnSelectedFilterOptionChanged(FilterOption? value)
    {
        if (value != null)
            _ = FilterByType(value.Type);
    }

    [RelayCommand]
    private async Task FilterByType(PromptTemplateType? type)
    {
        ActiveFilter = type;
        OnPropertyChanged(nameof(IsFilterAll));
        OnPropertyChanged(nameof(IsFilterSystem));
        OnPropertyChanged(nameof(IsFilterOutline));
        OnPropertyChanged(nameof(IsFilterChapter));
        var all = await _dbService.GetAllPromptTemplatesAsync();
        ApplyFilter(all);
    }

    public async Task LoadTemplatesAsync()
    {
        var all = await _dbService.GetAllPromptTemplatesAsync();
        ApplyFilter(all);
    }

    private void ApplyFilter(List<PromptTemplate> all)
    {
        var type = SelectedFilterOption?.Type;
        var filtered = type.HasValue
            ? all.Where(t => t.Type == type.Value).ToList()
            : all;
        Templates = new ObservableCollection<PromptTemplate>(filtered);
    }

    partial void OnSelectedTemplateChanged(PromptTemplate? value)
    {
        NotifyDetailState();
        if (value != null)
        {
            EditName = value.Name;
            EditType = value.Type;
            EditContent = value.Content;
        }
    }

    partial void OnIsEditingChanged(bool value)
    {
        NotifyDetailState();
    }

    private void NotifyDetailState()
    {
        OnPropertyChanged(nameof(IsDetailEmpty));
        OnPropertyChanged(nameof(IsDetailViewing));
        OnPropertyChanged(nameof(IsDetailEditing));
    }

    [RelayCommand]
    private void NewTemplate()
    {
        IsEditing = true;
        SelectedTemplate = null;
        EditName = string.Empty;
        EditType = PromptTemplateType.System;
        EditContent = string.Empty;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private void EditTemplate()
    {
        if (SelectedTemplate == null) return;
        IsEditing = true;
        EditName = SelectedTemplate.Name;
        EditType = SelectedTemplate.Type;
        EditContent = SelectedTemplate.Content;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SaveTemplate()
    {
        if (string.IsNullOrWhiteSpace(EditName) || string.IsNullOrWhiteSpace(EditContent))
        {
            StatusMessage = "名称和内容不能为空";
            return;
        }

        IsSaving = true;
        try
        {
            if (SelectedTemplate != null)
            {
                SelectedTemplate.Name = EditName.Trim();
                SelectedTemplate.Type = EditType;
                SelectedTemplate.Content = EditContent;
                await _dbService.UpdatePromptTemplateAsync(SelectedTemplate);
                StatusMessage = "模板已更新";
            }
            else
            {
                var newTemplate = new PromptTemplate
                {
                    Name = EditName.Trim(),
                    Type = EditType,
                    Content = EditContent,
                    IsBuiltIn = false
                };
                var saved = await _dbService.CreatePromptTemplateAsync(newTemplate);
                Templates.Add(saved);
                SelectedTemplate = saved;
                StatusMessage = "模板已创建";
            }

            IsEditing = false;
            await LoadTemplatesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存失败: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        if (SelectedTemplate != null)
        {
            EditName = SelectedTemplate.Name;
            EditType = SelectedTemplate.Type;
            EditContent = SelectedTemplate.Content;
        }
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteTemplate()
    {
        if (SelectedTemplate == null) return;
        if (SelectedTemplate.IsBuiltIn)
        {
            StatusMessage = "内置模板不可删除";
            return;
        }

        try
        {
            await _dbService.DeletePromptTemplateAsync(SelectedTemplate.Id);
            Templates.Remove(SelectedTemplate);
            SelectedTemplate = null;
            IsEditing = false;
            StatusMessage = "模板已删除";
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DuplicateTemplate()
    {
        if (SelectedTemplate == null) return;

        try
        {
            var copy = new PromptTemplate
            {
                Name = $"{SelectedTemplate.Name} (副本)",
                Type = SelectedTemplate.Type,
                Content = SelectedTemplate.Content,
                IsBuiltIn = false
            };
            var saved = await _dbService.CreatePromptTemplateAsync(copy);
            Templates.Add(saved);
            SelectedTemplate = saved;
            StatusMessage = "模板已复制";
        }
        catch (Exception ex)
        {
            StatusMessage = $"复制失败: {ex.Message}";
        }
    }
}
