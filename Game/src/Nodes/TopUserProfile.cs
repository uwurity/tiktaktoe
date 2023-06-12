using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Utils;

namespace tiktaktoe.Nodes;

public partial class TopUserProfile : MarginContainer
{
	[Export] public SceneHandler ProfileButton { get; set; } = null!;

	private Online Online => this.Autoload<Online>();

	public override void _Ready()
	{
		if (Online.Session == null) return;
		ProfileButton.Text = string.Format(ProfileButton.Text, Online.Session.Username);
	}

	public override void _Process(double delta)
	{
	}
}
