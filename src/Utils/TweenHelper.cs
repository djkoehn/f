using Godot;
namespace F.Utils;
// Marking the class as partial and extending Node, so it can be instanced in the scene
[Tool]
public partial class TweenHelper : Node
{
    public Tween AnimateScale(Node2D node, Tween? currentTween, Vector2 targetScale, float duration)
    {
        if (currentTween != null && currentTween.IsValid())
            currentTween.Kill();
        var tween = node.CreateTween();
        tween.TweenProperty(node, "scale", targetScale, duration);
        return tween;
    }

    public Tween AnimateAlpha(Node node, Tween? currentTween, float targetAlpha, float duration)
    {
        if (currentTween != null && currentTween.IsValid())
            currentTween.Kill();
        var tween = node.CreateTween();
        tween.TweenProperty(node, "modulate:a", targetAlpha, duration);
        return tween;
    }
} 