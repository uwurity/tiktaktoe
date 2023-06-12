using System;
using System.Linq;
using System.Text;
using Chickensoft.GoDotLog;
using Chickensoft.GoDotNet;
using Godot;
using Godot.Collections;
using Nakama;
using tiktaktoe.Autoload;
using tiktaktoe.Messages;
using tiktaktoe.Nodes;
using tiktaktoe.Utils;
using Environment = System.Environment;

namespace tiktaktoe.Main.Play;

public partial class PlayScene : Node
{
	private string _occupiedFormat = null!;
	private int _totalGrid;
	private IChannel? _channel;

	[Export] public PlayGrid PlayGrid { get; set; } = null!;
	[Export] public Godot.Label OccupiedLabel { get; set; } = null!;
	[Export] public Godot.Label Title { get; set; } = null!;
	[Export] public PresenceContainer PresenceContainer { get; set; } = null!;
	[Export] public SceneHandler BackButton { get; set; } = null!;
	[Export] public TextEdit ChannelMessages { get; set; } = null!;
	[Export] public LineEdit Message { get; set; } = null!;

	private readonly ILog _log = new GDLog(nameof(PlayScene));

	private Global Global => this.Autoload<Global>();
	private Online Online => this.Autoload<Online>();
	private MatchState MatchState => this.Autoload<MatchState>();

	public override async void _Ready()
	{
		_occupiedFormat = OccupiedLabel.Text;
		Online.Socket!.ReceivedMatchState += OnReceivedMatchState;
		Online.Socket.ReceivedMatchPresence += OnReceivedMatchPresence;
		PlayGrid.OnPlayerPressed += OnPlayerPressed;
		Online.Socket.ReceivedChannelMessage += OnReceivedChannelMessage;
		PlayGrid.Row = MatchState.Label.BoardSize.Row;
		PlayGrid.Col = MatchState.Label.BoardSize.Col;
		_totalGrid = PlayGrid.Row * PlayGrid.Col;
		OccupiedLabel.Text = string.Format(_occupiedFormat, 0, _totalGrid);
		await Online.Socket.SendMatchStateAsync(MatchState.MatchId, (long)OpCode.Ready, "");
		_channel = await Online.Socket.JoinChatAsync(MatchState.Label.Code.ToString(), ChannelType.Room,
			persistence: false, hidden: true);
	}

	private void OnReceivedChannelMessage(IApiChannelMessage message)
	{
		var userIdentity = string.IsNullOrEmpty(message.Username) ? message.SenderId : message.Username;
		var content = message.Content.FromJson<Dictionary<string, string>>();
		ChannelMessages.Text += $"[{userIdentity}]: {content!["message"]}" + Environment.NewLine;
	}

	private async void OnMessage_text_submitted(string newMessage)
	{
		var content = new Dictionary<string, string> { { "message", newMessage } }.ToJson();
		await Online.Socket!.WriteChatMessageAsync(_channel!.Id, content);
		Message.Clear();
	}

	private async void OnPlayerPressed(int row, int col, Action setMark)
	{
		if (PlayGrid.PlayerMark != MatchState.Mark) return;
		var msg = new MoveMessage
		{
			Position = new() { Row = row, Col = col },
		};
		await Online.Socket!.SendMatchStateAsync(MatchState.MatchId, (long)OpCode.Move, msg.ToJson());
		setMark();
	}

	private async void OnReceivedMatchPresence(IMatchPresenceEvent presence)
	{
		foreach (var join in presence.Joins)
		{
			_log.Print($"{join.Username} joined");
			await PresenceContainer.AddUser(join.UserId);
		}

		foreach (var leave in presence.Leaves)
		{
			_log.Print($"{leave.Username} left");
			PresenceContainer.RemoveUser(leave.UserId);
		}
	}

	private async void OnReceivedMatchState(IMatchState matchState)
	{
		var json = Encoding.UTF8.GetString(matchState.State);
		var opCode = (OpCode)matchState.OpCode;

		switch (opCode)
		{
			case OpCode.Start:
				var start = json.FromJson<StartMessage>();
				if (start is null) break;
				MatchState.Marks = start.Marks;
				PlayGrid.PlayerMark = MatchState.Marks![Online.Session!.UserId].Value;
				PlayGrid.Fill();

				foreach (var kv in start.Marks)
				{
					await PresenceContainer.AddUser(kv.Key, preText: kv.Key == Online.Session.UserId ? "(You)" : "");
				}

				MatchState.Mark = start.Mark;

				UpdateMarks(start.Marks);
				SetHighlighted(start.Mark);

				_log.Print("Game started!");
				break;

			case OpCode.Update:
				var update = json.FromJson<UpdateMessage>();

				if (update is null) break;

				_log.Print(update.Marked.ToString());

				if (update.Position != null)
					PlayGrid.SetMarkAt(update.Marked, update.Position);
				else
					_log.Print($"User {update.Marked} skipped their turn");

				MatchState.Mark = update.Mark;

				SetHighlighted(update.Mark, update.Marked);
				OccupiedLabel.Text = string.Format(_occupiedFormat, update.MoveCount, _totalGrid);
				break;

			case OpCode.Rejected:
				PlayGrid.Reset(PlayGrid.LastMarked);
				break;

			case OpCode.Done:
				var done = json.FromJson<DoneMessage>();
				if (done is null) break;
				Title.Text = "It's a draw!";
				Title.Visible = true;
				if (done.WinnerPositions is not null)
					Title.Text = $"{done.Winner.ToHumanName()} won the game!";
				PlayGrid.SetMarkAt(done.Winner, done.Position);
				MatchState.Mark = Mark.Undefined;
				Cleanup();
				await Online.Socket!.LeaveMatchAsync(MatchState.MatchId);
				await Online.Socket.CloseAsync();
				MatchState.MatchId = Guid.Empty.ToString();
				BackButton.Text = "< Return to Main menu";
				break;
		}
	}

	private void UpdateMarks(Marks marks)
	{
		foreach (var markInfo in marks)
		{
			PresenceContainer.SetUserMark(markInfo.Key, markInfo.Value.Value);
		}
	}

	private void SetHighlighted(Mark currentMark, Mark? previousMark = null)
	{
		if (previousMark is not null)
		{
			PresenceContainer.SetHighlighted(FindMarkKey(previousMark), false);
		}

		PresenceContainer.SetHighlighted(FindMarkKey(currentMark));
	}

	private string FindMarkKey(Mark? mark)
		=> MatchState.Marks!.FirstOrDefault(kv => kv.Value.Value == mark).Key;

	private void Cleanup()
	{
		Online.Socket!.ReceivedMatchState -= OnReceivedMatchState;
		Online.Socket.ReceivedMatchPresence -= OnReceivedMatchPresence;
		PlayGrid.OnPlayerPressed -= OnPlayerPressed;
	}

	private async void OnBackButton_pressed()
	{
		Cleanup();
		await Online.Socket!.CloseAsync();
		if (Online.Socket != null && MatchState.MatchId != Guid.Empty.ToString())
			await Online.Socket.LeaveMatchAsync(MatchState.MatchId);
		BackButton.OnPressed();
		Global.SceneManager.ResetSceneManager();
	}

	public override void _Process(double delta)
	{
	}
}
