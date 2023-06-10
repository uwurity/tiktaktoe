using Chickensoft.GoDotNet;
using Godot;
using System;
using tiktaktoe.Autoload;
using tiktaktoe.Main.Adventure;
using tiktaktoe.Main.Adventure.Play;
using tiktaktoe.Main.Classic;
using tiktaktoe.Main.Classic.Play;
using tiktaktoe.Utils;

namespace tiktaktoe.Main.Lobby.Winner;

public partial class WinnerScene : Node2D
{
	
	[Export] public Label Title { get; set; } = null!;
	[Export] public Label Time { get; set; } = null!;

	// Called when the node enters the scene tree for the first time.
	private Texture texure;
	public override void _Ready()
	{
		Godot.Label Title = this.GetNode<Godot.Label>("NinePatchRect/Title");
		Title.Text="Victory or Lost";
		
		//set player name
		Godot.Label PlayerName1 = this.GetNode<Godot.Label>("NinePatchRect/VBoxContainer/Player1/Name");
		PlayerName1.Text = "Player1 win";
		//set avatar
		Godot.TextureRect Avatar1 = this.GetNode<Godot.TextureRect>("NinePatchRect/VBoxContainer/Player1/Avatar");
		texure = GD.Load<Texture>("res://assets/sad.png");
		Avatar1.Texture = (Texture2D)texure;
		//set winner
		Godot.TextureRect WinnerLabel1 = this.GetNode<Godot.TextureRect>("NinePatchRect/VBoxContainer/Player1/Winner");
		texure = GD.Load<Texture>("res://assets/win.png");
		WinnerLabel1.Texture = (Texture2D)texure;
		PlayerName1.Modulate=new Color(243,185,46);
		
		
		
		Godot.Label PlayerName2 = this.GetNode<Godot.Label>("NinePatchRect/VBoxContainer/Player2/Name");
		PlayerName2.Text = "Player2";
		Godot.TextureRect Avatar2 = this.GetNode<Godot.TextureRect>("NinePatchRect/VBoxContainer/Player2/Avatar");
		texure = GD.Load<Texture>("res://assets/sad.png");
		Avatar2.Texture = (Texture2D)texure;
		
		Godot.Label PlayerName3 = this.GetNode<Godot.Label>("NinePatchRect/VBoxContainer/Player3/Name");
		PlayerName3.Text = "Player3";
		Godot.TextureRect Avatar3 = this.GetNode<Godot.TextureRect>("NinePatchRect/VBoxContainer/Player3/Avatar");
		texure = GD.Load<Texture>("res://assets/sad.png");
		Avatar3.Texture = (Texture2D)texure;
		
		Godot.Label PlayerName4 = this.GetNode<Godot.Label>("NinePatchRect/VBoxContainer/Player4/Name");
		PlayerName4.Text =null;
		Godot.TextureRect Avatar4 = this.GetNode<Godot.TextureRect>("NinePatchRect/VBoxContainer/Player4/Avatar");
		texure = GD.Load<Texture>("res://assets/sad.png");
		Avatar4.Texture = null;
		
		Godot.Label Time = this.GetNode<Godot.Label>("NinePatchRect/TimeLable/Time");
		Time.Text = "06:00";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
