using Avalonia.Controls;

namespace AvaloniaNovel.Services;

public interface IDialogManager
{
    void ShowDialog(Control content, string title);
    void DismissDialog();
}

public class DialogManager : IDialogManager
{
    private Window? _owner;

    public void SetOwner(Window owner)
    {
        _owner = owner;
    }

    public void ShowDialog(Control content, string title)
    {
        if (_owner == null) return;

        var dialog = new Window
        {
            Title = title,
            Content = content,
            Width = 800,
            Height = 500,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
        };
        dialog.ShowDialog(_owner);
    }

    public void DismissDialog()
    {
    }
}
