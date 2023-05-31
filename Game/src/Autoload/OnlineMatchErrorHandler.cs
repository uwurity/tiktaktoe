using Godot;

namespace tiktaktoe.Autoload;

public partial class OnlineMatchErrorHandler : Node
{
    private AcceptDialog? _acceptDialog;

    public override void _Ready()
    {
        // if (OnlineMatch.Global != null)
        //     OnlineMatch.Global.OnError += OnError;
    }

    private void OnError(string message)
    {
        _acceptDialog = new()
        {
            Title = "Connection Lost",
            DialogText = message,
        };
        _acceptDialog.Show();
    }
}