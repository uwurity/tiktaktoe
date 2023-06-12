using Chickensoft.GoDotNet;
using Godot;
using Nakama;
using tiktaktoe.Messages;

namespace tiktaktoe.Autoload;

public partial class MatchState : Node
{
	public Messages.Label Label { get; set; } = null!;

	public bool Host => Label.Creator == (Online.Session?.UserId ?? "");

	public string? MatchId { get; set; }

	public IMatch? Match { get; set; }

	public Board? Board { get; set; }

	public Mark PlayerMark { get; set; } = Mark.Undefined;
	public Mark Mark { get; set; } = Mark.Undefined;

	public Marks? Marks { get; set; }

	public double Deadline { get; set; }

	private Online Online => this.Autoload<Online>();

	public override void _Ready()
	{
		Label = new()
		{
			Level = Level.Undefined,
		};
	}
}
