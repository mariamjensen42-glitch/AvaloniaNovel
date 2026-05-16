using System.Threading;
using System.Threading.Tasks;
using AgentNovel.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;

namespace AgentNovel.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private int _notificationId;

    [ObservableProperty]
    private ViewModelBase _currentPage = new MergeViewModel();

    [ObservableProperty]
    private bool _isMergeView = true;
    [ObservableProperty]
    private bool _isSplitView;
    [ObservableProperty]
    private bool _isPageManagerView;
    [ObservableProperty]
    private bool _isSettingsView;

    [ObservableProperty]
    private bool _isNotificationVisible;

    [ObservableProperty]
    private string _notificationMessage = string.Empty;

    [ObservableProperty]
    private InfoBarSeverity _notificationSeverity = InfoBarSeverity.Informational;

    public MainViewModel()
    {
        WeakReferenceMessenger.Default.Register<NotificationMessage>(this, (_, m) =>
        {
            ShowNotification(m.Value.Message, m.Value.Type);
        });
    }

    private async void ShowNotification(string message, NotificationType type)
    {
        var id = Interlocked.Increment(ref _notificationId);

        NotificationMessage = message;
        NotificationSeverity = type switch
        {
            NotificationType.Success => InfoBarSeverity.Success,
            NotificationType.Error => InfoBarSeverity.Error,
            NotificationType.Warning => InfoBarSeverity.Warning,
            _ => InfoBarSeverity.Informational
        };
        IsNotificationVisible = true;

        await Task.Delay(3000);

        if (id == _notificationId)
        {
            IsNotificationVisible = false;
        }
    }

    partial void OnIsMergeViewChanged(bool value)
    {
        if (value)
        {
            CurrentPage = new MergeViewModel();
            IsSplitView = false;
            IsPageManagerView = false;
            IsSettingsView = false;
        }
    }

    partial void OnIsSplitViewChanged(bool value)
    {
        if (value)
        {
            CurrentPage = new SplitViewModel();
            IsMergeView = false;
            IsPageManagerView = false;
            IsSettingsView = false;
        }
    }

    partial void OnIsPageManagerViewChanged(bool value)
    {
        if (value)
        {
            CurrentPage = new PageManagerViewModel();
            IsMergeView = false;
            IsSplitView = false;
            IsSettingsView = false;
        }
    }

    partial void OnIsSettingsViewChanged(bool value)
    {
        if (value)
        {
            CurrentPage = new SettingsViewModel();
            IsMergeView = false;
            IsSplitView = false;
            IsPageManagerView = false;
        }
    }
}
