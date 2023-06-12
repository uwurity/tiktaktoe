using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Profile;

public partial class ProfileScene : Node
{
	[Export] public TextureRect Avatar { get; set; } = null!;
	[Export] public Label DisplayName { get; set; } = null!;
	[Export] public Label Username { get; set; } = null!;
	[Export] public Label Email { get; set; } = null!;
	[Export] public SceneHandler LogoutButton { get; set; } = null!;
	[Export] public HttpRequest HttpRequest { get; set; } = null!;

	private Global Global => this.Autoload<Global>();
	private Online Online => this.Autoload<Online>();

	public override async void _Ready()
	{
		if (Online.Session == null) return;

		var account = await Online.Execute(client => client.GetAccountAsync(Online.Session));

		if (account == null) return;

		if (!string.IsNullOrWhiteSpace(account.User.AvatarUrl))
		{
			HttpRequest.Request(account.User.AvatarUrl, new[] { "Content-Type: image/jpeg" });
		}

		SetEmptyOrName(Username, account.User.Username);
		SetEmptyOrName(DisplayName, account.User.DisplayName);
		SetEmptyOrName(Email, account.Email);
	}

	private static void SetEmptyOrName(Label label, string name)
	{
		label.Text = name;
		if (!string.IsNullOrWhiteSpace(name)) return;
		label.Text = "<empty>";
		label.SetFontColor(Colors.Yellow);
	}

	private void OnHttpRequest_request_completed(long result, long responseCode, string[] headers, byte[] body)
	{
		var image = new Image();
		image.LoadJpgFromBuffer(body);
		Avatar.Texture = ImageTexture.CreateFromImage(image);
	}

	public override void _Process(double delta)
	{
	}

	private async void OnLogoutButton_pressed()
	{
		await Online.Execute(client => client.SessionLogoutAsync(Online.Session));
		LogoutButton.OnPressed();
		Global.SceneManager.ResetSceneManager();
	}
}
