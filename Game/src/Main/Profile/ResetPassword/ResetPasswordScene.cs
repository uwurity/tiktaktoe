using System.Text.RegularExpressions;
using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Profile.ResetPassword;

public partial class ResetPasswordScene : Node2D
{
	[Export]
	public LineEdit Email { get; set; }

	private Online Online => this.Autoload<Online>();

	public override void _Ready()
	{
		if (Online.Session == null)
		{
			GetTree().ChangeSceneToFile("res://src/Main/MainScene.cs");
			return;
		}
	}

	public override void _Process(double delta)
	{
	}

	private void ResetPassword_pressed()
	{
		if (!Validators.EmailRegex().IsMatch(Email.Text))
		{
			// handle
			return;
		}
		// handle this server side
		// await Online.Execute(async client => await client.)
	}
}
