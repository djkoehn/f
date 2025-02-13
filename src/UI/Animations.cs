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

    public class BlockReturnAnimation
    {
        private BaseBlock _block;
        private Vector2 _startPos;
        private Vector2 _targetPos;
        private float _time;
        private const float DURATION = AnimConfig.Toolbar.ReturnAnimationDuration;

        public bool IsComplete => _time >= DURATION;
        public BaseBlock Block => _block;

        public BlockReturnAnimation(BaseBlock block, Vector2 startPos, Vector2 targetPos)
        {
            _block = block;
            _startPos = startPos;
            _targetPos = targetPos;
            _time = 0f;
        }

        public void Update(float delta)
        {
            if (IsComplete) return;

            _time = Mathf.Min(_time + delta, DURATION);
            float t = _time / DURATION;
            
            // Spring easing
            float c4 = (2f * Mathf.Pi) / 3f;
            float bounce = Mathf.Sin(-13f * Mathf.Pi/2 * (t + 1f)) * Mathf.Pow(2f, -10f * t) + 1f;
            t = bounce;
            
            // Animate position with spring
            _block.GlobalPosition = _startPos.Lerp(_targetPos, t);
            
            // Add bouncy scale animation
            float scaleBounciness = 1f + (0.2f * bounce - 0.2f);
            _block.Scale = Vector2.One * scaleBounciness;
        }
    }

    public static BlockReturnAnimation CreateBlockReturnAnimation(BaseBlock block, Vector2 startPos, Vector2 targetPos)
    {
        return new BlockReturnAnimation(block, startPos, targetPos);
    }
}
