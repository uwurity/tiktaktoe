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
	[Export] public Label MatchDescription { get; set; } = null!;
	[Export] public SceneHandler JoinButton { get; set; } = null!;
	[Export] public SpinBox JoinCode { get; set; } = null!;

	private Global Global => this.Autoload<Global>();
	private MatchState MatchState => this.Autoload<MatchState>();

	public override void _Ready()
	{
		MatchDescription.Text = MatchState.Level switch
		{
			nameof(ClassicScene) => "Just some good ol' Tiktaktoe",
			nameof(AdventureScene) => "Adventuring with friends!",
			_ => "",
		};
	}

	public override void _Process(double delta)
	{
	}
	
	private void OnJoinButton_pressed()
	{
		var matchExists = false;

		var nextSceneIfMatchNotExists = MatchState.Level switch
		{
			nameof(ClassicScene) => nameof(LobbyScene),
			_ => MatchState.Level,
		};

		MatchState.JoinCode = (int) JoinCode.Value;

		JoinButton.Scene = matchExists ? nameof(LobbyScene) : nextSceneIfMatchNotExists;
		
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
