using Godot;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Play;

public partial class PlayScene : Node2D
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
	
	private void OnBackButton_pressed()
	{
		// Show a dialog that confirms if user want to exit
		// then call GetNode<SceneHandler>("BackButton").OnPressed()
		if (true)
			GetNode<SceneHandler>("BackButton").OnPressed();
	}
}
