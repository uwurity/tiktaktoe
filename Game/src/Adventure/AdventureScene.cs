using Chickensoft.GoDotLog;
using Chickensoft.GoDotNet;
using CSharpFunctionalExtensions;
using Godot;
using tiktaktoe.Shared.NakamaHelpers;
using tiktaktoe.Shared.Autoloads;

namespace tiktaktoe.Adventure;

public partial class AdventureScene : CanvasLayer
{
    private readonly ILog _log = new GDLog(nameof(AdventureScene));
    
    private ClientNode ClientNode => this.Autoload<ClientNode>();
    
    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        const string email = "super@he.com";
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
}
