using F.Game.Tokens;
using F.Config;

namespace F.Game.Blocks;

public partial class Input : BaseBlock
{
    private float _value = 1.0f;

    public override void Initialize(BlockConfig config)
    {
        base.Initialize(config);
        if (config.DefaultValue.HasValue) _value = config.DefaultValue.Value;
    }

    public float GetValue()
    {
        return _value;
    }

    public void SetValue(float value)
    {
        _value = value;
    }

    public override void ProcessToken(Token token)
    {
        if (token == null) return;

        token.Value = _value;
        token.ProcessedBlocks.Add(this);

        if (_connectionManager == null) return;
        var (nextBlock, pipe) = _connectionManager.GetNextConnection();
        if (nextBlock != null)
            token.MoveTo(nextBlock);
        else
            token.QueueFree();
    }
}