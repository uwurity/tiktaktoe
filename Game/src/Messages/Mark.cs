namespace tiktaktoe.Messages;

public enum Mark
{
    X = 0,
    O = 1,
    Add = 2,
    Eq = 3,
    Undefined = -1,
}

public static partial class MarkExtensions
{
    public static string GetImagePath(this Mark mark) => mark switch
    {
        Mark.X => "res://assets/x.png",
        Mark.O => "res://assets/o.png",
        Mark.Add => "res://assets/plus.png",
        Mark.Eq => "res://assets/minus.png",
        _ => "res://assets/normal.png",
    };

    public static string ToHumanName(this Mark mark) => mark switch
    {
        Mark.X => "X",
        Mark.O => "O",
        Mark.Add => "+",
        Mark.Eq => "-",
        _ => "No one",
    };
}