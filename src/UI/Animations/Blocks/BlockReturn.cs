namespace F.UI.Animations.Blocks;

public sealed partial class BlockReturn : Node2D
{
    [Signal]
    public delegate void ReturnCompletedEventHandler(BaseBlock block);

    private const float DURATION = 0.3f;

    private Vector2 _startPos;
    private Vector2 _targetPos;
    private float _time;

    private BlockReturn(BaseBlock block, Vector2 startPos, Vector2 targetPos)
    {
        Block = block;
        _startPos = startPos;
        _targetPos = targetPos;
        _time = 0f;

        // Set block in front during animation!
        block.ZIndex = ZIndexConfig.Layers.DraggedBlock;
        block.ZAsRelative = false;

        GD.Print($"Block return animation starting with Z-index: {block.ZIndex}");
    }

    public bool IsComplete => _time >= DURATION;
    public BaseBlock Block { get; private set; }

    public static BlockReturn Create(BaseBlock block, Vector2 startPos, Vector2 targetPos)
    {
        return new BlockReturn(block, startPos, targetPos);
    }

    public void Update(float delta)
    {
        if (IsComplete) return;

        _time = Mathf.Min(_time + delta, DURATION);
        var t = _time / DURATION;

        // Use simple sine ease out for smooth direct path
        t = Easing.OutSine(t);

        // Just move block directly to target
        Block.GlobalPosition = _startPos.Lerp(_targetPos, t);
    }

    public override void _Process(double delta)
    {
        Update((float)delta);

        if (IsComplete)
        {
            // DON'T TOUCH BLOCK HERE - Just emit signal and clean up
            EmitSignal(SignalName.ReturnCompleted, Block);
            QueueFree();
        }
    }
}