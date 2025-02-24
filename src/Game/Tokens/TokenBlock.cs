using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using F.Framework.Blocks;
using F.Framework.Logging;

namespace F.Game.Tokens;

[Meta(typeof(IAutoNode))]
public partial class TokenBlock : BaseBlock, IProvide<Node>, IDependent
{
    [Dependency] public TokenManager TokenManager => this.DependOn<TokenManager>();

    Node IProvide<Node>.Value()
    {
        return this;
    }

    public override void _Ready()
    {
        base._Ready();
        this.Provide();
    }

    public override void SpawnToken()
    {
        Logger.Token.Print($"SpawnToken called for block {Name}");

        if (!HasOutputConnection())
        {
            Logger.Token.Err("Failed to spawn token - Block has no output connection");
            return;
        }

        Logger.Token.Print($"Spawning token from block {Name}");
        Services.Instance?.Tokens?.SpawnToken(this);
    }

    public override void _Notification(int what)
    {
        this.Notify(what);
    }
}