using Godot;

namespace tiktaktoe.Utils;

public static class SceneManagerExtensions
{
	public static Variant CreateOptions(this Node sceneManager, params Variant[] args)
		=> sceneManager.Call("create_options", args: args);
	
	public static Variant CreateGeneralOptions(this Node sceneManager, params Variant[] args)
		=> sceneManager.Call("create_general_options", args: args);
	
	public static Variant ShowFirstScene(this Node sceneManager, params Variant[] args)
		=> sceneManager.Call("show_first_scene", args: args);
	
	public static Variant ValidateScene(this Node sceneManager, params Variant[] args)
		=> sceneManager.Call("validate_scene", args: args);
	
	public static Variant ValidatePattern(this Node sceneManager, params Variant[] args)
		=> sceneManager.Call("validate_pattern", args: args);
	
	public static Variant ChangeScene(this Node sceneManager, params Variant[] args)
		=> sceneManager.Call("change_scene", args: args);

	public static string GetPreviousScene(this Node sceneManager)
		=> sceneManager.Call("get_previous_scene").AsString();

	public static Variant ChangeSceneToLoadedScene(this Node sceneManager, params Variant[] args)
		=> sceneManager.Call("change_scene_to_loaded_scene", args: args);
	
	public static Variant SetRecordedScene(this Node sceneManager, params Variant[] args)
		=> sceneManager.Call("set_recorded_scene", args: args);

	public static Variant LoadSceneInteractive(this Node sceneManager, params Variant[] args)
		=> sceneManager.Call("load_scene_interactive", args: args);

	public static Error LoadPercentChanged(this Node sceneManager, Callable callable)
		=> sceneManager.Connect("load_percent_changed", callable);
	
	public static Error LoadFinished(this Node sceneManager, Callable callable)
		=> sceneManager.Connect("load_finished", callable);
	
	public static Variant ResetSceneManager(this Node sceneManager)
		=> sceneManager.Call("reset_scene_manager");
}
