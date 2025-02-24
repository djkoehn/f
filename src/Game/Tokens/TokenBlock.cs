using Godot;
using F.Framework.Blocks;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;

namespace F.Game.Tokens;

[Meta(typeof(IAutoNode))]
public partial class TokenBlock : BaseBlock, IProvide<Node>, IDependent
{
    [Dependency]
    public TokenManager TokenManager => this.DependOn<TokenManager>();

    public override void _Ready()
    {
        base._Ready();
        this.Provide();
    }

    public override void SpawnToken()
    {
        GD.Print($"[TokenBlock Debug] SpawnToken called for block {Name}");
        if (!HasOutputConnection())
        {
            GD.PrintErr($"[TokenBlock Debug] Failed to spawn token - Block has no output connection");
            return;
        }

        GD.Print($"[TokenBlock Debug] Spawning token from block {Name}");
        TokenManager.SpawnToken(this);
    }

    public override void _Notification(int what) => this.Notify(what);

    Node IProvide<Node>.Value() => this;
}