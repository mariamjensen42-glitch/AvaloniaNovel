using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media.Imaging;

namespace AgentNovel.Models;

public partial class PdfPage : ObservableObject
{
    [ObservableProperty]
    private int _pageNumber;

    [ObservableProperty]
    private int _rotation;

    [ObservableProperty]
    private Bitmap? _thumbnail;

    [ObservableProperty]
    private bool _isSelected;
}
