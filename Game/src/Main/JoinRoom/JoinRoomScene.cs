using System.Linq;
using Chickensoft.GoDotNet;
using DebounceThrottle;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Main.Lobby;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.JoinRoom;

public partial class JoinRoomScene : Node
{
	private bool _validJoinCode;
	private DebounceDispatcher _joinCodeAction = null!;

	[Export] public Godot.Label Title { get; set; } = null!;
	[Export] public SceneHandler JoinButton { get; set; } = null!;
	[Export] public SpinBox JoinCode { get; set; } = null!;

	private Online Online => this.Autoload<Online>();
	private MatchState MatchState => this.Autoload<MatchState>();

	public override void _Ready()
	{
		_joinCodeAction = new(300);
		Title.Text = MatchState.Label.Level.ToString();
		JoinButton.Disabled = true;
		TryEnableJoinButton((int)JoinCode.Value);
	}

	public override void _Process(double delta)
	{
	}

	private void OnJoinCode_value_changed(double newJoinCode)
	{
		_joinCodeAction.Debounce(() =>
		{
			_validJoinCode = newJoinCode.VerifyJoinCode();
			if (!_validJoinCode) return;
			TryEnableJoinButton((int)newJoinCode);
		});
	}

	private async void TryEnableJoinButton(int joinCode)
	{
		if (Online.Session == null) return;

		MatchState.Label.Code = joinCode;
		JoinButton.Disabled = true;
		JoinButton.Text = "Checking...";

		// query for available match, and wait for match to open if true
		var query = string.Join(' ', MatchState.Label.ToString().Split().SkipLast(1));

		var result = await Online.Execute(client =>
			client.ListMatchesAsync(Online.Session, 0, Validators.GetMaxPlayers(MatchState.Label.Level),
				Validators.Limit,
				Validators.Authoritative, "", query));

		if (result == null || !result.Matches.Any())
		{
			JoinButton.Disabled = false;
			MatchState.Match = null;
			MatchState.MatchId = null;
			JoinButton.Text = "Create match";
			return;
		}

		var label = result.Matches.First().Label.FromJson<Messages.Label>();

		if (label is { Open: 1 })
		{
			JoinButton.Disabled = false;
			MatchState.MatchId = result.Matches.First().MatchId;
			JoinButton.Text = "Join match";
			return;
		}

		JoinButton.Text = "Match is closed";

		if (label != null && label.Level == MatchState.Label.Level) return;

		JoinButton.Text = "Room has different level";
	}

	private async void OnCreateMatch()
	{
		if (Online.Session == null) return;

		MatchState.Label.Creator = Online.Session.UserId;

		MatchState.MatchId = await Online.Execute(client => client.CreateMatch(Online.Session, MatchState.Label));

		if (MatchState.MatchId == "") return;

		OnJoinMatch();
	}

	private void OnJoinMatch()
	{
		JoinButton.Scene = nameof(LobbyScene);
		JoinButton.OnPressed();
	}

	private void OnJoinButton_pressed()
	{
		if (JoinButton.Text == "Join match")
			OnJoinMatch();
		else
			OnCreateMatch();
	}
}
