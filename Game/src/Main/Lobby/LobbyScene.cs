using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Main.Adventure;
using tiktaktoe.Main.Adventure.Play;
using tiktaktoe.Main.Classic;
using tiktaktoe.Main.Classic.Play;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Lobby;

public partial class LobbyScene : Node2D
{
	[Export] public Label JoinCode { get; set; } = null!;
	[Export] public SceneHandler BackButton { get; set; } = null!;
	[Export] public SceneHandler DemoPlayButton { get; set; } = null!;

	private MatchState MatchState => this.Autoload<MatchState>();
	private Texture texure;
	public override void _Ready()
	{
		JoinCode.Text = MatchState.JoinCode.ToString();
		
		Godot.Label PlayerName1 = this.GetNode<Godot.Label>("NinePatchRect/HBoxContainer/Avatar1/Name");
		PlayerName1.Text = MatchState.Name.ToString();
		Godot.TextureRect Avatar1 = this.GetNode<Godot.TextureRect>("NinePatchRect/HBoxContainer/Avatar1");
		texure = GD.Load<Texture>("res://assets/sad.png");
		Avatar1.Texture = (Texture2D)texure;
		
		Godot.Label PlayerName2 = this.GetNode<Godot.Label>("NinePatchRect/HBoxContainer/Avatar2/Name");
		PlayerName2.Text = MatchState.Name.ToString();
		Godot.TextureRect Avatar2 = this.GetNode<Godot.TextureRect>("NinePatchRect/HBoxContainer/Avatar2");
		texure = GD.Load<Texture>("res://assets/sad.png");
		Avatar2.Texture = (Texture2D)texure;
		
		Godot.Label PlayerName3 = this.GetNode<Godot.Label>("NinePatchRect/HBoxContainer/Avatar3/Name");
		PlayerName3.Text = MatchState.Name.ToString();
		Godot.TextureRect Avatar3 = this.GetNode<Godot.TextureRect>("NinePatchRect/HBoxContainer/Avatar3");
		texure = GD.Load<Texture>("res://assets/sad.png");
		Avatar3.Texture = (Texture2D)texure;
		
		Godot.Label PlayerName4 = this.GetNode<Godot.Label>("NinePatchRect/HBoxContainer/Avatar4/Name");
		PlayerName4.Text = null;
		Godot.TextureRect Avatar4 = this.GetNode<Godot.TextureRect>("NinePatchRect/HBoxContainer/Avatar4");
		texure = GD.Load<Texture>("res://assets/sad.png");
		Avatar4.Texture = null;

		
		Godot.Label State = this.GetNode<Godot.Label>("NinePatchRect/State");
		State.Text = "Waiting for players " + "(3/4)";
		
		


	}

	public override void _Process(double delta)
	{
	}
	
	private void OnBackButton_pressed()
	{
		// TODO: Show a dialog that confirms if user want to exit
		BackButton.OnPressed();
	}
	
	private void OnDemoPlayButton_pressed()
	{
		DemoPlayButton.Scene = MatchState.Level switch
		{
			nameof(ClassicScene) => nameof(ClassicPlayScene),
			nameof(AdventureScene) => nameof(AdventurePlayScene),
			_ => "",
		};

		DemoPlayButton.OnPressed();
	}
}
