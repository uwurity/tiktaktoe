using Godot;

namespace tiktaktoe.Utils;

public static class ControlHelper
{
    public static void SetFontColor(this Control control, Color color)
        => control.AddThemeColorOverride("font_color", color);

    public static void ResetFontColor(this Control control)
        => control.RemoveThemeColorOverride("font_color");

    public static void RemoveAllChild(this Control control)
    {
        foreach (var child in control.GetChildren())
        {
            child.QueueFree();
        }
    }
}