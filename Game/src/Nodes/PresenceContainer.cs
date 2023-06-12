using System.Linq;
using System.Threading.Tasks;
using Chickensoft.GoDotNet;
using Godot;
using tiktaktoe.Autoload;
using tiktaktoe.Messages;
using tiktaktoe.Utils;

namespace tiktaktoe.Nodes;

public partial class PresenceContainer : VBoxContainer
{
	[Export] public PackedScene Presence { get; set; } = null!;

	private Online Online => this.Autoload<Online>();

	public override void _Ready() => this.RemoveAllChild();

	public override void _Process(double delta)
	{
	}

	public async Task AddUser(string userId, string preText = "")
	{
		var result =
			await Online.Execute(client => client.GetUsersAsync(Online.Session, new[] { userId }));
		var user = result?.Users.First();
		var child = Presence.Instantiate<Button>();
		child.UniqueNameInOwner = true;
		child.Name = userId;
		child.Text = preText + (user?.DisplayName == ""
			? string.Format(child.Text, user.Username)
			: string.Format(child.Text, user?.DisplayName, user?.Username));
		AddChild(child);
	}

	public void SetUserMark(string userId, Mark mark)
	{
		var child = GetNode<Presence>(userId);
		if (child == null) return;
		var image = new Image();
		image.Load(mark.GetImagePath());
		child.StateIcon.Texture = ImageTexture.CreateFromImage(image);
	}

	public void SetHighlighted(string userId, bool state = true)
	{
		var child = GetNode<Button>(userId);
		if (child == null) return;
		child.Disabled = state;
	}

	public void RemoveUser(string userId)
	{
		var child = GetNode(userId);
		RemoveChild(child);
	}
}
