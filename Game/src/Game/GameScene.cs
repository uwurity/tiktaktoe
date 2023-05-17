using Chickensoft.AutoInject;
using Godot;
using Nakama;
using SuperNodes.Types;

namespace tiktaktoe.Game;

[SuperNode(typeof(Provider))]
public partial class GameScene : Node, IProvide<IClient> {
	private const string Scheme = "https";
	private const string Host = "dry-water-5646.fly.dev";
	private const int Port = 7350;
	private const string ServerKey = "defaultkey";

	public override partial void _Notification(int what);

	IClient IProvide<IClient>.Value() => new Client(Scheme, Host, Port, ServerKey);

	// Call the Provide() method once your dependencies have been initialized.
	private void OnReady() => Provide();
}
