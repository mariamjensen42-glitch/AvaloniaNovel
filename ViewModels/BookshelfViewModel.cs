using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using AvaloniaNovel.Models;
using AvaloniaNovel.Services;
using AvaloniaNovel.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaNovel.ViewModels;

public partial class BookshelfViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly CoverImageService _coverImageService;
    private readonly IDialogManager _dialogManager;

    [ObservableProperty]
    private ObservableCollection<Novel> _novels = new();

    [ObservableProperty]
    private Novel? _selectedNovel;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDeleteDialogOpen;

    [ObservableProperty]
    private string _newNovelTitle = string.Empty;

    [ObservableProperty]
    private string _newNovelGenre = string.Empty;

    [ObservableProperty]
    private string _newNovelWorldSetting = string.Empty;

    [ObservableProperty]
    private string _newNovelCoverPath = string.Empty;

    [ObservableProperty]
    private string _newNovelCoverFileName = string.Empty;

    [ObservableProperty]
    private Bitmap? _newNovelCoverPreview;

    public string[] GenreOptions { get; } = ["都市", "玄幻", "科幻", "悬疑", "历史", "言情"];
    public bool HasNovels => Novels.Count > 0;
    public int NovelsWithCover => System.Linq.Enumerable.Count(Novels, n => n.HasCoverImage);
    public string CoverStatusText => NovelsWithCover == 0 ? "暂无封面" : $"{NovelsWithCover}/{Novels.Count} 有封面";
    public bool HasNewNovelCover => NewNovelCoverPreview != null;
    public string NewNovelCoverDisplayName =>
        string.IsNullOrWhiteSpace(NewNovelCoverFileName) ? "未选择图片" : NewNovelCoverFileName;

    public event EventHandler<Novel>? NovelOpened;

    public BookshelfViewModel(IDialogManager dialogManager)
    {
        _dbService = new DatabaseService();
        _coverImageService = new CoverImageService();
        _dialogManager = dialogManager;
        AttachNovelCollection(Novels);
        _ = LoadNovelsAsync();
    }

    private async Task LoadNovelsAsync()
    {
        IsLoading = true;
        try
        {
            var novels = await _dbService.GetAllNovelsAsync();
            Novels = new ObservableCollection<Novel>(novels);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenNewNovelDialog()
    {
        NewNovelTitle = string.Empty;
        NewNovelGenre = GenreOptions[0];
        NewNovelWorldSetting = string.Empty;
        ClearCoverSelection();

        _dialogManager.ShowDialog(new CreateNovelDialogView { DataContext = this }, "新建小说");
    }

    [RelayCommand]
    private async Task CreateNovel()
    {
        if (string.IsNullOrWhiteSpace(NewNovelTitle))
        {
            return;
        }

        IsLoading = true;
        try
        {
            var novel = await _dbService.CreateNovelAsync(
                NewNovelTitle.Trim(),
                NewNovelGenre.Trim(),
                NewNovelWorldSetting.Trim(),
                NewNovelCoverPath);

            Novels.Insert(0, novel);
            ClearCoverSelection();
            _dialogManager.DismissDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CancelDialog()
    {
        IsDeleteDialogOpen = false;
        ClearCoverSelection();
        _dialogManager.DismissDialog();
    }

    [RelayCommand]
    private void ConfirmDelete(Novel? novel)
    {
        if (novel == null)
        {
            return;
        }

        SelectedNovel = novel;
        IsDeleteDialogOpen = true;
    }

    [RelayCommand]
    private async Task DeleteNovel()
    {
        if (SelectedNovel == null)
        {
            return;
        }

        await _dbService.DeleteNovelAsync(SelectedNovel.Id);
        Novels.Remove(SelectedNovel);
        SelectedNovel = null;
        IsDeleteDialogOpen = false;
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadNovelsAsync();
    }

    [RelayCommand]
    private void OpenNovel(Novel? novel)
    {
        if (novel != null)
        {
            NovelOpened?.Invoke(this, novel);
        }
    }

    [RelayCommand]
    private async Task SelectCover()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow == null)
        {
            return;
        }

        var selectedPath = await _coverImageService.PickCoverImageAsync(desktop.MainWindow);
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        NewNovelCoverPath = selectedPath;
        NewNovelCoverFileName = Path.GetFileName(selectedPath);
        NewNovelCoverPreview = CreatePreviewBitmap(selectedPath);
    }

    [RelayCommand]
    private void ClearCover()
    {
        ClearCoverSelection();
    }

    private void ClearCoverSelection()
    {
        NewNovelCoverPath = string.Empty;
        NewNovelCoverFileName = string.Empty;
        NewNovelCoverPreview = null;
    }

    private static Bitmap? CreatePreviewBitmap(string filePath)
    {
        try
        {
            return File.Exists(filePath) ? new Bitmap(filePath) : null;
        }
        catch
        {
            return null;
        }
    }

    partial void OnNewNovelCoverPreviewChanged(Bitmap? value)
    {
        OnPropertyChanged(nameof(HasNewNovelCover));
    }

    partial void OnNewNovelCoverFileNameChanged(string value)
    {
        OnPropertyChanged(nameof(NewNovelCoverDisplayName));
    }

    partial void OnNovelsChanged(ObservableCollection<Novel> value)
    {
        AttachNovelCollection(value);
        OnPropertyChanged(nameof(HasNovels));
    }

    private void AttachNovelCollection(ObservableCollection<Novel> collection)
    {
        collection.CollectionChanged -= OnNovelsCollectionChanged;
        collection.CollectionChanged += OnNovelsCollectionChanged;
    }

    private void OnNovelsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasNovels));
        OnPropertyChanged(nameof(NovelsWithCover));
        OnPropertyChanged(nameof(CoverStatusText));
    }
}
