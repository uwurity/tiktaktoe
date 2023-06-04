using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;

namespace tiktaktoe.Utils;

public partial class SceneHandler : Node
{
	[Export] public string Scene { get; set; } = "";
	[Export(PropertyHint.Range, "0,1,")] private float _fadeOutSpeed = 1.0f;
	[Export(PropertyHint.Range, "0,1,")] private float _fadeInSpeed = 1.0f;
	[Export] private string _fadeOutPattern = "fade";
	[Export] private string _fadeInPattern = "fade";
	[Export(PropertyHint.Range, "0,1,")] private float _fadeOutSmoothness = 0.1f; // (float, 0, 1)
	[Export(PropertyHint.Range, "0,1,")] private float _fadeInSmoothness = 0.1f; // (float, 0, 1)
	[Export] private bool _fadeOutInverted;
	[Export] private bool _fadeInInverted;
	[Export(PropertyHint.ColorNoAlpha)] private Color _color = new(0, 0, 0);
	[Export] private float _timeout;
	[Export] private bool _clickable;
	[Export] private bool _addToBack = true;

	private Variant _fadeOutOptions;
	private Variant _fadeInOptions;
	private Variant _generalOptions;

	private Global Global => this.Autoload<Global>();
	private Node SceneManager => Global.SceneManager;	

	public override void _Ready()
	{
		_fadeOutOptions =
			SceneManager.CreateOptions(_fadeOutSpeed, _fadeOutPattern, _fadeOutSmoothness, _fadeOutInverted);
		_fadeInOptions =
			SceneManager.CreateOptions(_fadeInSpeed, _fadeInPattern, _fadeInSmoothness, _fadeInInverted);
		_generalOptions = SceneManager.CreateGeneralOptions(_color, _timeout, _clickable, _addToBack);

		var fadeInFirstSceneOptions = SceneManager.CreateOptions(1, "fade");
		var firstSceneGeneralOptions = SceneManager.CreateGeneralOptions(new Color(0, 0, 0), 1, false);
		SceneManager.ShowFirstScene(fadeInFirstSceneOptions, firstSceneGeneralOptions);
		// code breaks if scene is not recognizable
		SceneManager.ValidateScene(Scene);
		// code breaks if pattern is not recognizable
		SceneManager.ValidatePattern(_fadeOutPattern);
		SceneManager.ValidatePattern(_fadeInPattern);
	}

	public void OnPressed()
		=> SceneManager.ChangeScene(Scene, _fadeOutOptions, _fadeInOptions, _generalOptions);
}
