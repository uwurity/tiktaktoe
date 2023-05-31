using Godot;
using System;
using Chickensoft.GoDotNet;

namespace tiktaktoe.Autoload;

public partial class OnlineErrorHandler : Node
{
    private AcceptDialog? _acceptDialog;
    
    private Online Online => this.Autoload<Online>();

    public override void _Ready() => Online.ConnectionError += ConnectionError;

    public override void _Notification(int what)
    {
        if (what != NotificationPredelete) return;
        Online.ConnectionError -= ConnectionError;
    }

    private void ConnectionError(Exception ex)
    {
        _acceptDialog = new()
        {
            Title = "Connection Lost",
            DialogText = "Cannot connect to Nakama server. Is Nakama running?",
        };
        _acceptDialog.Show();
    }
}
