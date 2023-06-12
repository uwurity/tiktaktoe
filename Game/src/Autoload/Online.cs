using System;
using System.Threading.Tasks;
using Godot;
using Nakama;
using Environment = System.Environment;

namespace tiktaktoe.Autoload;

public partial class Online : Node
{
	[Export(PropertyHint.Enum, "http,https,")]
	public string Scheme { get; set; } = "https";

	[Export] public string Host { get; set; } = "tiktaktoe-server.fly.dev";

	[Export(PropertyHint.Range, "1,65535,")]
	public int Port { get; set; } = 443;

	[Export] public string ServerKey { get; set; } = "defaultkey";

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

		var env = Environment.GetEnvironmentVariables();
		if (env.Contains("GODOT_ENV") && (string)(env["GODOT_ENV"] ?? "dev") == "dev")
		{
			Scheme = "http";
			Host = "localhost";
			Port = 7350;
			ServerKey = "defaultkey";
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
		if (Socket != null && IsSocketConnected) return;
		if (_socketConnecting) return;
		_socketConnecting = true;

		Socket = Execute(Nakama.Socket.From);
		_socketConnecting = false;

		if (Socket == null) return;

		await Socket.ConnectAsync(Session);
		SocketConnected?.Invoke(Socket);
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

	public void Execute(Action<IClient> func)
#pragma warning disable CS4014
		=> Execute(client =>
#pragma warning restore CS4014
		{
			func(client);
			return Task.CompletedTask;
		});

	public T? Execute<T>(Func<IClient, T?> func)
		=> ExecuteInternal(() => func(_client!));

	public async Task<T?> Execute<T>(Func<IClient, Task<T>> asyncFunc)
	{
		try
		{
			if (_client != null)
				return await asyncFunc(_client);
		}
		catch (Exception e) when (e is not ApiResponseException)
		{
			GD.Print($"{nameof(Online)}: Encountered Exception: {e}");
			ConnectionError?.Invoke(e);
		}

		return default;
	}

	private T? ExecuteInternal<T>(Func<T?> action)
	{
		try
		{
			if (_client != null)
				return action();
		}
		catch (Exception e) when (e is not ApiResponseException)
		{
			GD.Print($"{nameof(Online)}: Encountered Exception: {e}");
			ConnectionError?.Invoke(e);
		}

		return default;
	}
}
