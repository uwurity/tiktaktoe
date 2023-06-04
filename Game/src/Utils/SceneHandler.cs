using Godot;

namespace tiktaktoe.Utils;

public partial class SceneHandler : Node
{
	[Export] public string Scene { get; set; } = "";
	[Export] private float _fadeOutSpeed = 1.0f;
	[Export] private float _fadeInSpeed = 1.0f;
	[Export] private string _fadeOutPattern = "fade";
	[Export] private string _fadeInPattern = "fade";
	[Export] private float _fadeOutSmoothness = 0.1f; // (float, 0, 1)
	[Export] private float _fadeInSmoothness = 0.1f; // (float, 0, 1)
	[Export] private bool _fadeOutInverted;
	[Export] private bool _fadeInInverted;
	[Export] private Color _color = new(0, 0, 0);
	[Export] private float _timeout;
	[Export] private bool _clickable;
	[Export] private bool _addToBack = true;

	private Variant _fadeOutOptions;
	private Variant _fadeInOptions;
	private Variant _generalOptions;

	private Node _sceneManager = null!;

	public override void _Ready()
	{
		_sceneManager = GetNode("/root/SceneManager");
		_fadeOutOptions =
			_sceneManager.CreateOptions(_fadeOutSpeed, _fadeOutPattern, _fadeOutSmoothness, _fadeOutInverted);
		_fadeInOptions =
			_sceneManager.CreateOptions(_fadeInSpeed, _fadeInPattern, _fadeInSmoothness, _fadeInInverted);
		_generalOptions = _sceneManager.CreateGeneralOptions(_color, _timeout, _clickable, _addToBack);

		var fadeInFirstSceneOptions = _sceneManager.CreateOptions(1, "fade");
		var firstSceneGeneralOptions = _sceneManager.CreateGeneralOptions(new Color(0, 0, 0), 1, false);
		_sceneManager.ShowFirstScene(fadeInFirstSceneOptions, firstSceneGeneralOptions);
		// code breaks if scene is not recognizable
		_sceneManager.ValidateScene(Scene);
		// code breaks if pattern is not recognizable
		_sceneManager.ValidatePattern(_fadeOutPattern);
		_sceneManager.ValidatePattern(_fadeInPattern);
	}

	public void OnPressed()
		=> _sceneManager.ChangeScene(Scene, _fadeOutOptions, _fadeInOptions, _generalOptions);
}
