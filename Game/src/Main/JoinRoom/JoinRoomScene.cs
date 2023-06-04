using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Main.Adventure;
using tiktaktoe.Main.Classic;
using tiktaktoe.Main.Lobby;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.JoinRoom;

public partial class JoinRoomScene : Node2D
{
	[Export]
	public Label MatchDescription { get; set; }
	
	[Export]
	public SceneHandler JoinButton { get; set; }
	
	[Export]
	public SpinBox JoinCode { get; set; }

	private MatchState _matchState => this.Autoload<MatchState>();

	public override void _Ready()
	{
		MatchDescription.Text = _matchState.Description;
		GD.Randomize();
	}

	public override void _Process(double delta)
	{
	}
	
	private void OnJoinButton_pressed()
	{
		// this is only for demoing
		_matchState.JoinCode = (int) JoinCode.Value;
		JoinButton.Scene = nameof(AdventureScene);
		if (GetNode("/root/SceneManager").GetPreviousScene() == nameof(ClassicScene))
			JoinButton.Scene = nameof(LobbyScene);
		JoinButton.OnPressed();
	}
	
	private void OnJoinCode_value_changed(double joinCode)
	{
	}
	
	private void OnTimer_timeout()
	{
		// check if join code is available on server
		// shows a tool tip if the room is full
		// JoinCode.Value
	}
}
