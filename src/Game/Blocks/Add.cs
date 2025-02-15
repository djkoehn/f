using F.Game.Tokens;
using F.Game.Connections;

namespace F.Game.Blocks;

public partial class Add : BaseBlock
{
    private float _value = 1.0f;
    protected ConnectionManager? _connectionManager;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void Initialize(BlockConfig config)
    {
        base.Initialize(config);
        if (config.DefaultValue.HasValue) _value = config.DefaultValue.Value;
    }

    public override void ProcessToken(Token token)
    {
        if (token == null) return;

        token.Value += _value;
        token.ProcessedBlocks.Add(this);

        if (_connectionManager != null)
        {
            (IBlock? nextBlock, ConnectionPipe? pipe) = _connectionManager.GetNextConnection();
            if (nextBlock != null)
                token.MoveTo(nextBlock);
            else
                token.QueueFree();
        }
        else
        {
            token.QueueFree();
        }
    }
}