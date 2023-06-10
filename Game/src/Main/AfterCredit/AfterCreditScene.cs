using Chickensoft.GoDotLog;
using Chickensoft.GoDotNet;
using Godot;
using System;
using tiktaktoe.Autoload;
using tiktaktoe.Utils.Auth;
using CSharpFunctionalExtensions;

namespace tiktaktoe.Main.AfterCredit;

public partial class AfterCreditScene : Node2D
{
	private readonly ILog _log = new GDLog(nameof(AfterCreditScene));

	private MatchState MatchState => this.Autoload<MatchState>();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//MatchState.Level = nameof(AfterCreditScene);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void OnBackButton_pressed(string extra_arg_0)
	{
	 
	}
	private void OnLinkButton_pressed()
	{
		// Replace with function body.
		//OS.ShellOpen("https://github.com/uwurity/tiktaktoe.git");
	}
}






