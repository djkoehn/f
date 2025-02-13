using Godot;
using F.UI;

namespace F.Blocks;

public partial class Output : BaseBlock
{
    [Signal]
    public delegate void TokenProcessedEventHandler(float value);
    
    private ShaderMaterial? _rippleMaterial;
    
    public override void _Ready()
    {
        base._Ready();
        
        // Get the ripple material
        var rippleEffect = GetNode<ColorRect>("RippleEffect");
        _rippleMaterial = rippleEffect.Material as ShaderMaterial;
        if (_rippleMaterial == null)
        {
            GD.PrintErr($"[{Name}] Failed to get ripple shader material");
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (_rippleMaterial == null) return;
        
        // Update ripple center position
        var screenPos = GlobalPosition / GetViewport().GetVisibleRect().Size;
        _rippleMaterial.SetShaderParameter("center", screenPos);
    }

    public override void ProcessToken(Token token)
    {
        // Call base first to trigger animation and process value
        base.ProcessToken(token);
        
        // Then handle output-specific logic
        ProcessValue(token.Value);
        
        // Play traversal complete sound
        AudioManager.Instance?.PlayTraversalComplete();
    }

    public void ProcessValue(float value)
    {
        EmitSignal(SignalName.TokenProcessed, value);
        TriggerRippleEffect();
    }
    
    private void TriggerRippleEffect()
    {
        if (_rippleMaterial == null) return;

        // Reset shader parameters
        _rippleMaterial.SetShaderParameter("ripple_progress", 0.0f);
        _rippleMaterial.SetShaderParameter("fade_progress", 0.0f);
        _rippleMaterial.SetShaderParameter("distortion_strength", 0.0f);
        
        var tween = CreateTween().SetParallel();
        
        // Expand ripple
        tween.TweenMethod(
            Callable.From((float v) => _rippleMaterial.SetShaderParameter("ripple_progress", v)),
            0.0f, 1.0f, 1.0f
        ).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        
        // Quick distortion ramp up then fade
        tween.TweenMethod(
            Callable.From((float v) => _rippleMaterial.SetShaderParameter("distortion_strength", v)),
            0.0f, 0.3f, 0.1f
        );
        tween.TweenMethod(
            Callable.From((float v) => _rippleMaterial.SetShaderParameter("distortion_strength", v)),
            1.0f, 0.0f, 0.9f
        ).SetDelay(0.1f);
        
        // Overall fade out
        tween.TweenMethod(
            Callable.From((float v) => _rippleMaterial.SetShaderParameter("fade_progress", v)),
            0.0f, 1.0f, 1.0f
        ).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
    }
}