using Godot;

namespace tiktaktoe.Nodes;

public partial class Presence : Button
{
	[Export] public TextureRect StateIcon { get; set; } = null!;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
}
