using System;
using Chickensoft.GoDotLog;
using Chickensoft.GoDotNet;
using DebounceThrottle;
using Godot;
using Nakama;
using tiktaktoe.Autoload;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Profile.SignIn;

public partial class SignInScene : Node
{
    private bool _validUsername;
    private bool _validPassword;
    private DebounceDispatcher _usernameAction = null!;
    private DebounceDispatcher _passwordAction = null!;

    [Export] public LineEdit Username { get; set; } = null!;
    [Export] public LineEdit Password { get; set; } = null!;
    [Export] public SceneHandler LoginButton { get; set; } = null!;
    [Export] public SceneHandler SignUpButton { get; set; } = null!;
    [Export] public SceneHandler ForgotButton { get; set; } = null!;

    private readonly ILog _log = new GDLog(nameof(SignInScene));

    private Online Online => this.Autoload<Online>();
    private Global Global => this.Autoload<Global>();

    public override void _Ready()
    {
        base._Ready();
        TryEnableLoginButton();
        _usernameAction = new(500);
        _passwordAction = new(500);
        Online.SessionConnected += OnSessionConnected;
        Online.SessionChanged += OnSessionChanged;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Online.SessionConnected -= OnSessionConnected;
        Online.SessionChanged -= OnSessionChanged;
    }

    private void OnSessionConnected(ISession session)
    {
        LoginButton.OnPressed();
        Global.SceneManager.ResetSceneManager();
    }

    private void OnSessionChanged(ISession? session)
    {
        if (session != null) return;
        Username.SetFontColor(Colors.Red);
        Password.SetFontColor(Colors.Red);
        LoginButton.Disabled = false;
        LoginButton.Text = "Login";
    }

    public override void _Process(double delta)
    {
    }

    private async void OnLoginButton_pressed()
    {
        Username.Editable = false;
        Password.Editable = false;
        LoginButton.Disabled = true;
        LoginButton.Text = "Logging in...";

        string? username = null;
        var email = "";

        if (Username.Text.IsEmail())
            email = Username.Text;
        else
            username = Username.Text;

        try
        {
            Online.Session = await Online.Execute(client =>
                client.AuthenticateEmailAsync(email, Password.Text, username, create: false));
        }
        catch (ApiResponseException e)
        {
            _log.Err(e.GrpcStatusCode switch
            {
                5 => "Account not exists",
                16 => "Invalid credentials",
                _ => e.Message
            });
        }
        catch (Exception)
        {
            OnSessionChanged(null);
        }

        LoginButton.Text = "Login";
        LoginButton.Disabled = false;
        Username.Editable = true;
        Password.Editable = true;
    }

    private void OnUsername_text_changed(string newUsername)
    {
        Global.PendingUsername = newUsername;
        Username.ResetFontColor();
        _usernameAction.Debounce(() =>
        {
            _validUsername = newUsername.Contains('@')
                ? Username.VerifyEmail(newUsername)
                : !string.IsNullOrWhiteSpace(newUsername) && Username.VerifyUsername(newUsername);
            TryEnableLoginButton();
        });
    }

    private void OnPassword_text_changed(string newPassword)
    {
        Password.ResetFontColor();
        _passwordAction.Debounce(() =>
        {
            _validPassword = Password.VerifyPassword(newPassword);
            TryEnableLoginButton();
        });
    }

    private void TryEnableLoginButton()
        => LoginButton.Disabled = !(_validUsername && _validPassword);

    private void OnSignUpButton_pressed()
    {
        if (!string.IsNullOrWhiteSpace(Username.Text)) Global.PendingUsername = Username.Text;
        SignUpButton.OnPressed();
    }

    private void OnForgotButton_pressed()
    {
        if (!string.IsNullOrWhiteSpace(Username.Text)) Global.PendingUsername = Username.Text;
        ForgotButton.OnPressed();
    }
}