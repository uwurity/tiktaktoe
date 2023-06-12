using Chickensoft.GoDotLog;
using Chickensoft.GoDotNet;
using DebounceThrottle;
using Godot;
using Nakama;
using tiktaktoe.Autoload;
using tiktaktoe.Utils;
using tiktaktoe.Utils.Auth;

namespace tiktaktoe.Main.Profile.SignUp;

public partial class SignUpScene : Node
{
    private bool _validUsername;
    private bool _validPassword;
    private bool _validPasswordCheck;
    private bool _validEmail;
    private bool _acceptTos;
    private DebounceDispatcher _usernameAction = null!;
    private DebounceDispatcher _emailAction = null!;
    private DebounceDispatcher _passwordAction = null!;
    private DebounceDispatcher _passwordCheckAction = null!;

    [Export] public LineEdit Username { get; set; } = null!;
    [Export] public LineEdit Email { get; set; } = null!;
    [Export] public LineEdit Password { get; set; } = null!;
    [Export] public LineEdit PasswordCheck { get; set; } = null!;
    [Export] public CheckBox TosCheckBox { get; set; } = null!;
    [Export] public SceneHandler SignUpButton { get; set; } = null!;

    private readonly ILog _log = new GDLog(nameof(SignUpScene));

    private Global Global => this.Autoload<Global>();
    private Online Online => this.Autoload<Online>();

    public override void _Ready()
    {
        base._Ready();
        TryEnableSignUpButton();
        _usernameAction = new(300);
        _emailAction = new(300);
        _passwordAction = new(300);
        _passwordCheckAction = new(300);
        if (!string.IsNullOrWhiteSpace(Global.PendingUsername))
        {
            if (Global.PendingUsername.Contains('@'))
                Email.Text = Global.PendingUsername;
            else
                Username.Text = Global.PendingUsername;
        }

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
        _log.Print(session.ToString() ?? "");
        SignUpButton.OnPressed();
        Global.SceneManager.ResetSceneManager();
    }

    private void OnSessionChanged(ISession? session)
    {
        if (session != null) return;
        Username.SetFontColor(Colors.Red);
        Email.SetFontColor(Colors.Red);
        Password.SetFontColor(Colors.Red);
        PasswordCheck.SetFontColor(Colors.Red);
        SignUpButton.Disabled = false;
        SignUpButton.Text = "Create an account";
    }

    private async void OnSignUpButton_pressed()
    {
        SignUpButton.Disabled = true;
        SignUpButton.Text = "Signing up...";

        var result = await Online.Execute(client =>
            client.TrySignup(Email.Text, Password.Text, Username.Text));

        Online.Session = result?.Session.GetValueOrDefault();
    }

    public override void _Process(double delta)
    {
    }

    private void OnUsername_text_changed(string newUsername)
    {
        Username.ResetFontColor();
        _usernameAction.Debounce(() =>
        {
            _validUsername = Username.VerifyUsername(newUsername);
            TryEnableSignUpButton();
        });
    }

    private void OnEmail_text_changed(string newEmail)
    {
        Email.ResetFontColor();
        _emailAction.Debounce(() =>
        {
            _validEmail = Email.VerifyEmail(newEmail);
            TryEnableSignUpButton();
        });
    }

    private void OnPassword_text_changed(string newPassword)
    {
        Password.ResetFontColor();
        _passwordAction.Debounce(() =>
        {
            _validPassword = Password.VerifyPassword(newPassword);
            TryEnableSignUpButton();
        });
    }

    private void OnPasswordCheck_text_changed(string newPasswordCheck)
    {
        PasswordCheck.ResetFontColor();
        _passwordCheckAction.Debounce(() =>
        {
            _validPasswordCheck = Password.Text == newPasswordCheck;
            if (!_validPasswordCheck) PasswordCheck.SetFontColor(Colors.Red);
            TryEnableSignUpButton();
        });
    }

    private void OnTosCheckBox_toggled(bool toggled)
    {
        _acceptTos = toggled;
        if (_acceptTos) TosCheckBox.ResetFontColor();
        else TosCheckBox.SetFontColor(Colors.Red);
        TryEnableSignUpButton();
    }

    private void TryEnableSignUpButton()
        => SignUpButton.Disabled =
            !((_validUsername || _validEmail) && _validPassword && _validPasswordCheck && _acceptTos);
}