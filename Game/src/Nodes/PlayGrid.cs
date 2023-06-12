using System;
using System.Linq;
using Chickensoft.GoDotLog;
using Godot;
using tiktaktoe.Messages;
using tiktaktoe.Utils;

namespace tiktaktoe.Nodes;

public partial class PlayGrid : GridContainer
{
	public event Action<int, int, Action>? OnPlayerPressed;

	public BoardPosition LastMarked { get; private set; } = new();

	[Export(PropertyHint.Range, "5,15,")] public int Row { get; set; }
	[Export(PropertyHint.Range, "5,15,")] public int Col { get; set; }
	[Export] public PackedScene Player { get; set; } = null!;
	[Export] public Mark PlayerMark { get; set; } = Mark.Undefined;

	private readonly ILog _log = new GDLog(nameof(PlayGrid));

	public override void _Ready()
	{
		this.RemoveAllChild();
		// Columns = Col;
	}

	public override void _Process(double delta)
	{
	}

	public void Fill()
	{
		for (var row = 0; row < Row; ++row)
		for (var col = 0; col < Col; ++col)
		{
			var player = Player.Instantiate<TextureButton>();
			player.UniqueNameInOwner = true;
			player.Name = $"{row},{col}";
			var row1 = row;
			var col1 = col;
			player.Pressed += () => OnPlayerPressed?.Invoke(row1, col1, () => SetMark(player, PlayerMark));
			AddChild(player);
		}
	}

	public void Reset()
	{
		foreach (var player in GetChildren().OfType<TextureButton>())
		{
			player.Disabled = false;
		}
	}

	public void Reset(BoardPosition position)
		=> GetCell(position.Row, position.Col).Disabled = false;

	public void DisableOthers()
	{
		// foreach (var player in GetChildren().OfType<TextureButton>())
		// {
		// 	player.TextureDisabled
		// 	player.Disabled = true;
		// }
	}

	public void SetMarkAt(Mark mark, BoardPosition pos)
		=> SetMarkAt(mark, pos.Row, pos.Col);

	private void SetMarkAt(Mark mark, int row, int col)
	{
		SetMark(GetCell(row, col), mark);
		if (mark == PlayerMark) LastMarked = new() { Row = row, Col = col };
	}

	public void SetMark(TextureButton player, Mark mark)
	{
		_log.Print(mark.ToString());
		var image = new Image();
		image.Load(mark.GetImagePath());
		player.TextureDisabled = ImageTexture.CreateFromImage(image);
		player.Disabled = true;
	}

	public TextureButton GetCell(int row, int col) => GetNode<TextureButton>($"{row},{col}");
}
