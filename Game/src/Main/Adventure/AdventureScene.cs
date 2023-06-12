using Chickensoft.GoDotLog;
using Chickensoft.GoDotNet;
using DebounceThrottle;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Messages;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Adventure;

public partial class AdventureScene : Node
{
	private bool _validRows;
	private bool _validCols;
	private DebounceDispatcher _rowsAction = null!;
	private DebounceDispatcher _colsAction = null!;

	[Export] public SpinBox Rows { get; set; } = null!;
	[Export] public SpinBox Cols { get; set; } = null!;
	[Export] public SceneHandler FindRoomButton { get; set; } = null!;

	private readonly ILog _log = new GDLog(nameof(AdventureScene));

	private MatchState MatchState => this.Autoload<MatchState>();

	public override void _Ready()
	{
		MatchState.Label.Level = Level.Adventure;
		_rowsAction = new(300);
		_colsAction = new(300);
	}

	public override void _Process(double delta)
	{
	}

	private void OnRows_value_changed(double value)
	{
		_rowsAction.Debounce(() =>
		{
			_validRows = value is >= 5 and <= 15;
			TryEnableFindMatchButton();
		});
	}


	private void OnCols_value_changed(double value)
	{
		_colsAction.Debounce(() =>
		{
			_validCols = value is >= 5 and <= 15;
			TryEnableFindMatchButton();
		});
	}

	private void TryEnableFindMatchButton()
		=> FindRoomButton.Disabled = !(_validRows && _validCols);

	private void OnFindRoom_button_pressed()
	{
		MatchState.Label.BoardSize = new()
		{
			Row = (int)Rows.Value,
			Col = (int)Cols.Value,
		};
		FindRoomButton.OnPressed();
	}
}
