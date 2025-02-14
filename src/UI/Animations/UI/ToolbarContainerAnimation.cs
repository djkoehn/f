namespace F.UI.Animations.UI;

public sealed partial class ToolbarContainerAnimation : Node
{
    private const float DURATION = 0.4f;
    private readonly List<(BaseBlock block, Vector2 startPos, Vector2 targetPos)> _blocks = new();
    private float _time;

    public static ToolbarContainerAnimation Create(List<(BaseBlock block, Vector2 targetPos)> blocks)
    {
        var anim = new ToolbarContainerAnimation();
        foreach (var (block, target) in blocks) anim._blocks.Add((block, block.Position, target));
        return anim;
    }

    public override void _Process(double delta)
    {
        _time = Mathf.Min(_time + (float)delta, DURATION);
        var t = _time / DURATION;

        // Use smooth easing for sliding
        t = Easing.OutCubic(t);

        foreach (var (block, start, target) in _blocks) block.Position = start.Lerp(target, t);

        if (_time >= DURATION)
        {
            // Make sure blocks are exactly at target
            foreach (var (block, _, target) in _blocks) block.Position = target;
            QueueFree();
        }
    }
}