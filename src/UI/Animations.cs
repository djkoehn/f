using Godot;
using System;

namespace F.UI;

public static class Animations
{
    public static Tween TriggerBlockAnimation(Node2D node, Vector2 originalPosition, Vector2 originalScale, float originalRotation, float intensityMultiplier = 1.0f)
    {
        var tween = node.CreateTween().SetParallel();
        
        // Random values within reasonable ranges
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        
        var bounceHeight = rng.RandfRange(15f, 25f);
        var scalePunch = rng.RandfRange(1.1f, 1.2f) * intensityMultiplier;
        var rotationAngle = rng.RandfRange(-10f, 10f) * intensityMultiplier;
        
        // Bounce up
        var jumpPosition = originalPosition;
        jumpPosition.Y -= bounceHeight;
        
        tween.TweenProperty(node, "position", jumpPosition, 0.15f)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(node, "position", originalPosition, 0.3f)
            .SetTrans(Tween.TransitionType.Spring).SetEase(Tween.EaseType.Out)
            .SetDelay(0.15f);
            
        // Scale punch
        var punchScale = originalScale * scalePunch;
        tween.TweenProperty(node, "scale", punchScale, 0.15f)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(node, "scale", originalScale, 0.3f)
            .SetTrans(Tween.TransitionType.Spring).SetEase(Tween.EaseType.Out)
            .SetDelay(0.15f);
            
        // Rotation wobble
        tween.TweenProperty(node, "rotation", originalRotation + Mathf.DegToRad(rotationAngle), 0.15f)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(node, "rotation", originalRotation, 0.3f)
            .SetTrans(Tween.TransitionType.Spring).SetEase(Tween.EaseType.Out)
            .SetDelay(0.15f);

        return tween;
    }
}
