using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;

namespace tiktaktoe.Main.Profile;

public partial class ProfileScene : Node2D
{
	[Export]
	public Image Avatar { get; set; }

	[Export]
	public Label Username { get; set; }
	
	[Export]
	public Label Email { get; set; }

	private Online Online => this.Autoload<Online>();

	public override async void _Ready()
	{
		if (Online.Session == null)
		{
			GetTree().ChangeSceneToFile("res://src/Main/Main/MainScene.tscn");
			return;
		}

		if (Online.Session.IsExpired)
		{
			GetTree().ChangeSceneToFile("res://src/Main/Profile/SignIn/SignInScene.tscn");
			return;
		}

		using var request = new HttpRequest();
		request.DownloadFile = "user://avatar.jpg";
		request.RequestCompleted += (_, __, ___, body) => Avatar.LoadJpgFromBuffer(body);

		await Online.Execute(async client =>
		{
			var account = await client.GetAccountAsync(Online.Session);
			request.Request(account.User.AvatarUrl, new []{ "Content-Type: image/jpeg" });
			Username.Text = account.User.Username;
			Email.Text = account.Email;
		});
	}

	public override void _Process(double delta)
	{
	}

	public void OnChangePassword_pressed() =>
		GetTree().ChangeSceneToFile("res://src/Main/Profile/ResetPassword/ResetPasswordScene.tscn");

	public async void OnLogout_pressed()
	{
		await Online.Execute(async client => await client.SessionLogoutAsync(Online.Session));
		GetTree().ChangeSceneToFile("res://src/Main/MainScene.tscn");
	}
}
