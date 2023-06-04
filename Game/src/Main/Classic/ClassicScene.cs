using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Main.Lobby;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Classic;

public partial class ClassicScene : Node2D
{
	[Export]
	private string? Description;

	private MatchState _matchState => this.Autoload<MatchState>();
	
	public override void _Ready()
	{
		_matchState.Level = nameof(ClassicScene);
		_matchState.Description = Description;
	}

	public override void _Process(double delta)
	{
	}
	
	private void OnPvpButton_pressed()
	{
		_matchState.Description += " with friends!";
		GetNode<SceneHandler>("PvpButton").OnPressed();
	}
}
