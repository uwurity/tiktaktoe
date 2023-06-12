using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Messages;

namespace tiktaktoe.Main.Classic;

public partial class ClassicScene : Node
{
	private MatchState MatchState => this.Autoload<MatchState>();

	public override void _Ready()
	{
		MatchState.Label.Level = Level.Classic;
		MatchState.Label.BoardSize = new() { Row = 5, Col = 5 };
	}

	public override void _Process(double delta)
	{
	}
}
