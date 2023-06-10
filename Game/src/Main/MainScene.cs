using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Main.Adventure;
using tiktaktoe.Main.JoinRoom;
using tiktaktoe.Utils;

namespace tiktaktoe.Main;

public partial class MainScene : Node2D
{
	[Export] public SceneHandler AdventureButton { get; set; } = null!;

	private MatchState MatchState => this.Autoload<MatchState>();

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
	
	private void OnAdventureButton_pressed()
	{
		// TODO: check if logged in
		MatchState.Level = nameof(AdventureScene);
		AdventureButton.OnPressed();
	}
	private void ONCreditButton_pressed()
	{
		// Replace with function body.
		//CreditButton.Onpressed();
	}
}












