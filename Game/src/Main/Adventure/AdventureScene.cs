using Chickensoft.GoDotLog;
using Chickensoft.GoDotNet;
using CSharpFunctionalExtensions;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Utils.Auth;

namespace tiktaktoe.Main.Adventure;

public partial class AdventureScene : Node2D
{
	[Export]
	private string? Description;
	
	[Export]
	public Label SceneDescription;

	private readonly ILog _log = new GDLog(nameof(AdventureScene));

	private MatchState _matchState => this.Autoload<MatchState>();

	public override void _Ready()
	{
		_matchState.Level = nameof(AdventureScene);
		_matchState.Description = Description;
		SceneDescription.Text = Description;
	}

	public override void _Process(double delta)
	{
	}
}
