using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Classic.Play;

public partial class ClassicPlayScene : Node2D
{
	[Export] public SceneHandler BackButton { get; set; } = null!;
	
	private Global Global => this.Autoload<Global>();
	
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
	
	private void OnBackButton_pressed()
	{
		// TODO: confirms if user want to exit
		BackButton.OnPressed();
		Global.SceneManager.ResetSceneManager();
	}
}
