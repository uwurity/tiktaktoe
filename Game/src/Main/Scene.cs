using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nakama;
using tiktaktoe.Shared.NakamaHelpers;

namespace tiktaktoe.Main;

public partial class Scene : CanvasLayer
{
    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        const string scheme = "http";
        const string host = "dan-laptop.penguin-major.ts.net";
        const int port = 7350;
        const string serverKey = "defaultkey";
        var client = new Client(scheme, host, port, serverKey);
        
        const string email = "super@heros.com";
        const string password = "batsignal";
        const string username = "hith";

        var signupResult = await client.TrySignup(email, password, username);

        signupResult.Match(
            session => GD.Print(session),
            state => GD.PrintErr(state)
        );
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
