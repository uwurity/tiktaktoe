using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Main.Adventure;
using tiktaktoe.Main.Adventure.Play;
using tiktaktoe.Main.Classic;
using tiktaktoe.Main.Classic.Play;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Lobby;

public partial class LobbyScene : Node2D
{
	[Export] public Label JoinCode { get; set; } = null!;
	[Export] public SceneHandler BackButton { get; set; } = null!;
	[Export] public SceneHandler DemoPlayButton { get; set; } = null!;

	private MatchState MatchState => this.Autoload<MatchState>();
	
	public override void _Ready()
	{
		JoinCode.Text = MatchState.JoinCode.ToString();
	}

	public override void _Process(double delta)
	{
	}
	
	private void OnBackButton_pressed()
	{
		// TODO: Show a dialog that confirms if user want to exit
		BackButton.OnPressed();
	}
	
	private void OnDemoPlayButton_pressed()
	{
		DemoPlayButton.Scene = MatchState.Level switch
		{
			nameof(ClassicScene) => nameof(ClassicPlayScene),
			nameof(AdventureScene) => nameof(AdventurePlayScene),
			_ => "",
		};

		DemoPlayButton.OnPressed();
	}
}
