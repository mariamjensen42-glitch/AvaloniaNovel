using Avalonia.Controls;
using Avalonia.Threading;

namespace AvaloniaNovel.Services;

public interface IDialogManager
{
    void ShowDialog(Control content, string title);
    void DismissDialog();
}

public class DialogManager : IDialogManager
{
    private Window? _owner;
    private Window? _currentDialog;

    public void SetOwner(Window owner)
    {
        _owner = owner;
    }

    public void ShowDialog(Control content, string title)
    {
        if (_owner == null) return;

        _currentDialog = new Window
        {
            Title = title,
            Content = content,
            Width = 800,
            Height = 500,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
        };

        _currentDialog.Closed += (_, _) => _currentDialog = null;

        // Non-blocking Show so DismissDialog() can close it
        _currentDialog.Show(_owner);
    }

    public void DismissDialog()
    {
        if (_currentDialog != null)
        {
            Dispatcher.UIThread.Post(() => _currentDialog.Close());
            _currentDialog = null;
        }
    }
}
