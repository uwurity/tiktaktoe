using CSharpFunctionalExtensions;
using Godot;
using Nakama;

namespace tiktaktoe.Shared.Autoloads;

public partial class ClientNode : Node
{
    public Maybe<IClient> Client;
    
    public override void _Ready()
    {
        const string scheme = "http";
        const string host = "dan-laptop-docker-desktop";
        const int port = 7350;
        const string serverKey = "defaultkey";
        Client = new Client(scheme, host, port, serverKey);
    }
}
