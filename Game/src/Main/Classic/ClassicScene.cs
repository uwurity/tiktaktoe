using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Main.JoinRoom;
using tiktaktoe.Main.Lobby;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Classic;

public partial class ClassicScene : Node2D
{
	[Export] public SceneHandler PvpButton { get; set; } = null!;
	
	private MatchState MatchState => this.Autoload<MatchState>();
	
	public override void _Ready()
	{
		MatchState.Level = nameof(ClassicScene);
	}

	public override void _Process(double delta)
	{
	}
	
	private void OnPvpButton_pressed()
	{
		// TODO: check if logged in
		PvpButton.OnPressed();
	}
}
