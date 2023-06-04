using Chickensoft.GoDotLog;
using Chickensoft.GoDotNet;
using CSharpFunctionalExtensions;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Utils.Auth;

namespace tiktaktoe.Main.Adventure;

public partial class AdventureScene : Node2D
{
	private readonly ILog _log = new GDLog(nameof(AdventureScene));

	private MatchState MatchState => this.Autoload<MatchState>();

	public override void _Ready()
	{
		MatchState.Level = nameof(AdventureScene);
	}

	public override void _Process(double delta)
	{
	}
}
