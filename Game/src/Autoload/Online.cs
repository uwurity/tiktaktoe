using Godot;
using System;
using Nakama;
using System.Threading.Tasks;

namespace tiktaktoe.Autoload;

public partial class Online : Node
{
    [Export(PropertyHint.Enum, "http,https,")]
    public string Scheme = "https";

    [Export]
    public string Host = "dry-water-5646.fly.dev";

    [Export(PropertyHint.Range, "1,65535,")]
    public int Port = 7350;

    [Export]
    public string ServerKey = "defaultkey";

    private IClient? _client;

    public event Action<ISession?>? SessionChanged;
    public event Action<ISession>? SessionConnected;
    public event Action<ISocket>? SocketConnected;
    public event Action<Exception>? ConnectionError;

    private ISession? _session;

    public ISession? Session
    {
        get => _session;
        set
        {
            _session = value;

            SessionChanged?.Invoke(value);

            if (value != null)
                SessionConnected?.Invoke(value);
        }
    }

    public ISocket? Socket { get; private set; }

    private bool _socketConnecting;

    public bool IsSocketConnected => Socket is { IsConnected: true };

    public override void _Ready()
    {
        if (_client != null)
        {
            QueueFree();
            return;
        }

        _client = new Client(
            scheme: Scheme,
            host: Host,
            port: Port,
            serverKey: ServerKey
        );
    }

    public override void _Notification(int what)
    {
        if (what != NotificationPredelete) return;
        _client = null;
    }

    public async void ConnectSocket()
    {
        await Execute(async client =>
        {
            if (Socket != null && IsSocketConnected) return;
            if (_socketConnecting) return;
            _socketConnecting = true;

            Socket = Nakama.Socket.From(client);
            await Socket.ConnectAsync(Session);
            _socketConnecting = false;
            SocketConnected?.Invoke(Socket);
        });
    }

    /// <summary>
    /// Use this for all your Nakama calls.
    /// This handles the case when the whole Nakama server is down, where we won't event get
    /// an ApiResponseException back. When this happens, ConnectionError is invoked.
    /// </summary>
    /// <param name="asyncFunc"></param>
    /// <returns></returns>
    public async Task Execute(Func<IClient, Task> asyncFunc)
    {
        try
        {
            if (_client == null) return;
            await asyncFunc(_client);
        }
        catch (Exception e) when (e is not ApiResponseException)
        {
            GD.Print($"{nameof(Online)}: Encountered Exception: {e}");
            // We catch any exception that is not an APIResponseException
            // APIResponseException is normal behaviour
            ConnectionError?.Invoke(e);
        }
    }
}
