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

public partial class PromptTemplateViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;

    // ── 模板列表 ─────────────────────────────────────────────────────
    [ObservableProperty]
    private ObservableCollection<PromptTemplate> _templates = new();

    [ObservableProperty]
    private PromptTemplate? _selectedTemplate;

    // ── 编辑表单 ─────────────────────────────────────────────────────
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

    // ── 筛选 ─────────────────────────────────────────────────────────
    [ObservableProperty]
    private PromptTemplateType _filterType = PromptTemplateType.System;

    /// <summary>null 表示显示全部</summary>
    [ObservableProperty]
    private PromptTemplateType? _activeFilter;

    public PromptTemplateType[] TemplateTypes { get; } =
        Enum.GetValues<PromptTemplateType>();

    public string TypeDisplayName(PromptTemplateType type) => type switch
    {
        PromptTemplateType.System => "系统人设",
        PromptTemplateType.Outline => "大纲生成",
        PromptTemplateType.Chapter => "章节写作",
        _ => type.ToString()
    };

    public PromptTemplateViewModel()
    {
        _dbService = new DatabaseService();
        _ = LoadTemplatesAsync();
    }

    public async Task LoadTemplatesAsync()
    {
        var all = await _dbService.GetAllPromptTemplatesAsync();
        ApplyFilter(all);
    }

    [RelayCommand]
    private async Task FilterByType(PromptTemplateType? type)
    {
        ActiveFilter = type;
        var all = await _dbService.GetAllPromptTemplatesAsync();
        ApplyFilter(all);
    }

    private void ApplyFilter(List<PromptTemplate> all)
    {
        var filtered = ActiveFilter.HasValue
            ? all.Where(t => t.Type == ActiveFilter.Value).ToList()
            : all;
        Templates = new ObservableCollection<PromptTemplate>(filtered);
    }

    partial void OnSelectedTemplateChanged(PromptTemplate? value)
    {
        if (value != null)
        {
            EditName = value.Name;
            EditType = value.Type;
            EditContent = value.Content;
        }
    }

    // ── 新建模板 ─────────────────────────────────────────────────────
    [RelayCommand]
    private void NewTemplate()
    {
        IsEditing = true;
        SelectedTemplate = null;
        EditName = string.Empty;
        EditType = FilterType;
        EditContent = string.Empty;
        StatusMessage = string.Empty;
    }

    // ── 编辑选中模板 ─────────────────────────────────────────────────
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

    // ── 保存模板（新建或更新）──────────────────────────────────────
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
                // 更新
                SelectedTemplate.Name = EditName.Trim();
                SelectedTemplate.Type = EditType;
                SelectedTemplate.Content = EditContent;
                await _dbService.UpdatePromptTemplateAsync(SelectedTemplate);
                StatusMessage = "模板已更新";
            }
            else
            {
                // 新建
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

    // ── 取消编辑 ─────────────────────────────────────────────────────
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

    // ── 删除模板 ─────────────────────────────────────────────────────
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

    // ── 复制模板 ─────────────────────────────────────────────────────
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
