using Chickensoft.GoDotNet;
using DebounceThrottle;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Profile.ResetPassword;

public partial class ResetPasswordScene : Node
{
	private bool _validEmail;
	private DebounceDispatcher _emailAction = null!;

	[Export] public LineEdit Email { get; set; } = null!;
	[Export] public Button ResetPasswordButton { get; set; } = null!;

	private Global Global => this.Autoload<Global>();
	private Online Online => this.Autoload<Online>();

	public override void _Ready()
	{
		_emailAction = new(300);
		if (string.IsNullOrWhiteSpace(Global.PendingUsername) || !Global.PendingUsername.Contains('@')) return;
		Email.Editable = Online.Session == null;
		ResetPasswordButton.Disabled = Online.Session == null;
		Email.Text = Global.PendingUsername;
	}

	public override void _Process(double delta)
	{
	}

	private void OnEmail_text_changed(string newEmail)
	{
		Email.ResetFontColor();
		_emailAction.Debounce(() =>
		{
			_validEmail = Email.VerifyEmail(newEmail);
			TryEnableResetPasswordButton();
		});
	}

	private void OnResetPasswordButton_pressed()
	{
		// handle this server side
		// await Online.Execute(async client => await client.)
	}

	private void TryEnableResetPasswordButton()
		=> ResetPasswordButton.Disabled = !_validEmail;
}
