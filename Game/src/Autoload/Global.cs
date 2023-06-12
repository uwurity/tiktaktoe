using Godot;

namespace tiktaktoe.Autoload;

public partial class Global : Node
{
	public Node SceneManager { get; private set; } = null!;

	private const string SceneManagerPath = "/root/SceneManager";

	public string? PendingUsername { get; set; } = null;

	public override void _Ready()
	{
		SceneManager = GetNode(SceneManagerPath);
	}

	public override void _Process(double delta)
	{
	}
}
