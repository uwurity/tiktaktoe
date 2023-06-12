using System.Linq;
using System.Text;
using Chickensoft.GoDotLog;
using Chickensoft.GoDotNet;
using Godot;
using Nakama;
using tiktaktoe.Autoload;
using tiktaktoe.Messages;
using tiktaktoe.Nodes;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Lobby;

public partial class LobbyScene : Node
{
	private string _titleFormat = null!;
	private string _stateFormat = null!;
	private int _total;

	[Export] public Godot.Label Title { get; set; } = null!;
	[Export] public Godot.Label StateLabel { get; set; } = null!;
	[Export] public PresenceContainer PresenceContainer { get; set; } = null!;
	[Export] public SceneHandler BackButton { get; set; } = null!;
	[Export] public SceneHandler PlayButton { get; set; } = null!;

	private readonly ILog _log = new GDLog(nameof(LobbyScene));

	private Online Online => this.Autoload<Online>();
	private MatchState MatchState => this.Autoload<MatchState>();

	public override async void _Ready()
	{
		base._Ready();

		_titleFormat = Title.Text;
		_stateFormat = StateLabel.Text;

		Title.Text = string.Format(_titleFormat, MatchState.Label.Code);
		UpdateStateLabel();

		Online.SocketConnected += OnSocketConnected;
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		Online.ConnectSocket();
	}

	public override void _ExitTree() => Cleanup();

	private void Cleanup()
	{
		Online.SocketConnected -= OnSocketConnected;
		if (Online.Socket == null) return;
		Online.Socket.ReceivedMatchState -= OnReceivedMatchState;
		Online.Socket.ReceivedMatchPresence -= OnReceivedMatchPresence;
	}

	private async void OnSocketConnected(ISocket socket)
	{
		socket.ReceivedMatchState += OnReceivedMatchState;
		socket.ReceivedMatchPresence += OnReceivedMatchPresence;
		MatchState.Match = await socket.JoinMatchAsync(MatchState.MatchId);
		MatchState.Label = MatchState.Match.Label.FromJson<Messages.Label>()!;
		_total += MatchState.Match.Presences.Count();
		UpdateStateLabel();
		foreach (var matchPresence in MatchState.Match.Presences)
		{
			await PresenceContainer.AddUser(matchPresence.UserId);
		}
	}

	private void OnReceivedMatchState(IMatchState matchState)
	{
		var opCode = (OpCode)matchState.OpCode;

		if (opCode != OpCode.LeaveLobby && opCode != OpCode.Rejoin) return;

		if (opCode == OpCode.Rejoin)
		{
			if (Online.Session == null) return;
			var stateJson = Encoding.UTF8.GetString(matchState.State);
			var rejoin = stateJson.FromJson<RejoinMessage>();
			if (rejoin == null) return;

			MatchState.Board = rejoin.Board;
			MatchState.Mark = rejoin.Mark;
			MatchState.Marks = rejoin.Marks;
			MatchState.Deadline = rejoin.Deadline;
		}

		PlayButton.OnPressed();
	}

	private async void OnReceivedMatchPresence(IMatchPresenceEvent presence)
	{
		foreach (var join in presence.Joins)
		{
			_log.Print($"{join.Username} joined");
			_total++;
			UpdateStateLabel();
			await PresenceContainer.AddUser(join.UserId);
		}

		foreach (var leave in presence.Leaves)
		{
			_log.Print($"{leave.Username} left");
			PresenceContainer.RemoveUser(leave.UserId);
			_total--;
			UpdateStateLabel();
		}
	}

	private void UpdateStateLabel() => StateLabel.Text = string.Format(_stateFormat, _total, MatchState.Label.Size);

	public override void _Process(double delta)
	{
	}

	private async void OnBackButton_pressed()
	{
		Cleanup();
		if (Online.Socket != null)
			await Online.Socket.LeaveMatchAsync(MatchState.MatchId);
		BackButton.OnPressed();
	}
}
