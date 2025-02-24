using F.Framework.Blocks;
using F.Framework.Logging;
using Godot;

namespace F.UI.Animations.Blocks;

public class BlockReturn
{
    private readonly BaseBlock _block;
    private readonly Vector2 _startPosition;
    private readonly Vector2 _endPosition;
    private readonly float _duration;
    private readonly Tween _tween;

    public BlockReturn(BaseBlock block, Vector2 endPosition, float duration = 0.5f)
    {
        _block = block;
        _startPosition = block.GlobalPosition;
        _endPosition = endPosition;
        _duration = duration;
        _tween = block.CreateTween();
    }

    public void Start()
    {
        Logger.UI.Print($"Block return animation starting with Z-index: {_block.ZIndex}");

        _tween.TweenProperty(_block, "global_position", _endPosition, _duration)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.Out);
    }

    public void Cancel()
    {
        _tween.Kill();
    }
}