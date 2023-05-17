using Godot;

namespace tiktaktoe.Game;

public static class GameSceneExtensions
{
    public static void ChangeSceneToFile(this GameScene gameScene, string path)
    {
        var scene = GD.Load<PackedScene>(path).Instantiate();
        gameScene.RemoveChild(gameScene.GetChild(0));
        gameScene.AddChild(scene);
    }
}
