using System.ComponentModel.DataAnnotations;
using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Profile.SignIn;

public partial class SignInScene : Node2D
{
	[Export] public LineEdit Username { get; set; } = null!;
	[Export] public LineEdit Password { get; set; } = null!;
	
	private Online Online => this.Autoload<Online>();
	private Global Global => this.Autoload<Global>();
	
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
	
	private async void  OnLoginButton_pressed()
	{
		await Online.Execute(async client =>
		{
			var session = await client.AuthenticateEmailAsync(Username.Text, Password.Text);
			if (!session.Created)
			{
				// chinh mau cai khung username va email neu ko dang nhap dc
				// Username.
				return;
			}

			Online.Session = session;
			Global.SceneManager.ResetSceneManager();
			Global.SceneManager.ChangeScene(nameof(MainScene));
		});
	}
	
	

	private void _on_signup_button_pressed()
	{
		// Replace with function body.
	}


	private void _on_forgot_button_pressed()
	{
		// Replace with function body.
	}
	private void OnPressed()
{
	// Replace with function body.
}
}



