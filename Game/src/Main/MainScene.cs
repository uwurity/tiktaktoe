using Chickensoft.AutoInject;
using Chickensoft.GoDotLog;
using CSharpFunctionalExtensions;
using Godot;
using Nakama;
using SuperNodes.Types;
using tiktaktoe.Game;
using tiktaktoe.Shared.Auth;

namespace tiktaktoe.Main;

[SuperNode(typeof(Dependent))]
public partial class MainScene : CanvasLayer
{
	public override partial void _Notification(int what);
	
	private readonly ILog _log = new GDLog(nameof(MainScene));
	
	[Dependency]
	private IClient Client => DependOn<IClient>();
	
	public async void OnResolved()
	{
		// All of my dependencies are now available! Do whatever you want with 
		// them here.
		const string email = "super@heros.com";
		const string password = "batsignal";
		const string username = "hith";

		var signupResult = await Client.TrySignup(email, password, username);

		signupResult.Session.Match(
			session => _log.Print(session.ToString() ?? ""),
			() => _log.Warn(signupResult.ToString())
		);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnClassicButton_pressed() => GetParent<GameScene>().ChangeSceneToFile("res://src/Classic/ClassicScene.tscn");
	
	private void OnAdventureButton_pressed() => GetParent<GameScene>().ChangeSceneToFile("res://src/Adventure/AdventureScene.tscn");
	
	private void OnSettingsButton_pressed() => GetParent<GameScene>().ChangeSceneToFile("res://src/Settings/SettingsScene.tscn");
}
