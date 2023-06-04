using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Lobby;

public partial class LobbyScene : Node2D
{
	[Export]
	public Label JoinCode { get; set; }

	private MatchState _matchState => this.Autoload<MatchState>();
	
	public override void _Ready()
	{
		JoinCode.Text = _matchState.JoinCode.ToString();
	}

	public override void _Process(double delta)
	{
	}
	
	private void OnBackButton_pressed()
	{
		// Show a dialog that confirms if user want to exit
		// then call GetNode<SceneHandler>("BackButton").OnPressed()
		if (true)
			GetNode<SceneHandler>("BackButton").OnPressed();
	}
}
