using Godot;

namespace tiktaktoe.Autoload;

public partial class MatchState : Node
{
	public string? Level { get; set; }
	public int JoinCode { get; set; }
	public string? Description { get; set; }
	public bool Leader { get; set; }
	
	public override void _Ready() {}
}
