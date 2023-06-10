using Chickensoft.GoDotLog;
using Chickensoft.GoDotNet;
using CSharpFunctionalExtensions;
using Godot;
using Nakama;
using tiktaktoe.Autoload;
using tiktaktoe.Utils.Auth;

namespace tiktaktoe.Main.Profile.SignUp;

public partial class SignUpScene : Node2D
{
	private readonly ILog _log = new GDLog(nameof(SignUpScene));

	private Online Online => this.Autoload<Online>();

	public override async void _Ready()
	{
		const string email = "super@heros.com";
		const string password = "batsignal";
		const string username = "hith";
		
		_log.Print(string.Join("\r\n", IP.ResolveHostnameAddresses(Online.Host)));

		await Online.Execute(async client =>
		{
			var signupResult = await client.TrySignup(email, password, username);
			signupResult.Session.Match(
				session =>
				{
					_log.Print(session.ToString() ?? "");
					Online.Session = session;
				},
				() => _log.Warn(signupResult.ToString())
			);
		});
	}

	public override void _Process(double delta)
	{
	}
}
