using Chickensoft.GoDotLog;
using Godot;
using Nakama;
using Chickensoft.GoDotNet;
using CSharpFunctionalExtensions.ValueTasks;
using tiktaktoe.Shared.NakamaHelpers;
using tiktaktoe.Shared.Autoloads;

namespace tiktaktoe.Main;

public partial class MainScene : CanvasLayer
{
    private readonly ILog _log = new GDLog(nameof(MainScene));
    
    private ClientNode ClientNode => this.Autoload<ClientNode>();
    
    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {   
        const string email = "super@heros.com";
        const string password = "batsignal";
        const string username = "hith";

        await ClientNode.Client.Execute(async client =>
        {
            var signupResult = await client.TrySignup(email, password, username);

            signupResult.Match(
                session => _log.Print(session?.ToString() ?? ""),
                state => _log.Warn(state.ToString())
            );
        });
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
    
    private void OnClassicButton_pressed() => GetTree().ChangeSceneToFile("res://src/Classic/ClassicScene.tscn");
    
    private void OnAdventureButton_pressed() => GetTree().ChangeSceneToFile("res://src/Adventure/AdventureScene.tscn");
    
    private void OnSettingsButton_pressed() => GetTree().ChangeSceneToFile("res://src/Settings/SettingsScene.tscn");
}
