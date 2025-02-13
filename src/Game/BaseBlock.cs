using Godot;
using F.Blocks;
using System.Collections.Generic;
using System.Linq;
using F.UI;

namespace F;

public partial class BaseBlock : Node2D
{
    private bool _inBlockLayer;
    protected BlockMetadata? _metadata;
    private Node2D? _inputSocket;
    private Node2D? _outputSocket;
    private bool _isBeingDragged = false;
    public ConnectionLayer? ConnectionLayer;
    private Tween? _scaleTween;
    private Sprite2D? _shadowSprite;
    private Tween? _shadowTween;
    private ColorRect? _connectionBounds;
    private Tween? _snapTween;
    
    // Store original transform values
    protected Vector2 _originalPosition;
    protected Vector2 _originalScale;
    protected float _originalRotation;

    [Signal]
    public delegate void BlockPlacedEventHandler(BaseBlock block);

    public override void _Ready()
    {
        base._Ready();
        CallDeferred(nameof(InitializeConnections));
        CallDeferred(nameof(InitializeShadow));
        CallDeferred(nameof(InitializeBounds));
        
        // Store initial transform values
        _originalPosition = Position;
        _originalScale = Scale;
        _originalRotation = Rotation;
    }

    private void InitializeConnections()
    {
        _inputSocket = GetNodeOrNull<Node2D>("BlockInputSocket");
        _outputSocket = GetNodeOrNull<Node2D>("BlockOutputSocket");

        ConnectionLayer = GameManager.Instance?.ConnectionLayer;
        if (ConnectionLayer == null)
        {
            GD.PrintErr($"Could not find ConnectionLayer for {Name} through GameManager");
        }
    }

    private void InitializeShadow()
    {
        // Debug shadow node search
        var children = GetChildren();
        foreach (var child in children)
        {
            GD.Print($"[{Name}] Child node: {child.Name}");
        }

        // Try to get shadow sprite by direct path first
        _shadowSprite = GetNodeOrNull<Sprite2D>("shadow");
        if (_shadowSprite == null)
        {
            // Try finding it by type and name
            foreach (var child in children)
            {
                if (child is Sprite2D sprite && child.Name.ToString().ToLower().Contains("shadow"))
                {
                    _shadowSprite = sprite;
                    GD.Print($"[{Name}] Found shadow sprite by search");
                    break;
                }
            }
        }
        
        if (_shadowSprite == null)
        {
        //     GD.PrintErr($"[{Name}] Could not find shadow sprite!");
            return;
        }

        GD.Print($"[{Name}] Shadow sprite found and initialized");
        _shadowSprite.Visible = false;
        if (_shadowSprite.Material is ShaderMaterial material)
        {
            material.SetShaderParameter("offset", Vector2.Zero);
            material.SetShaderParameter("blur_amount", 0.0f);
            material.SetShaderParameter("shadow_opacity", 0.0f);
        }
    }

    private void InitializeBounds()
    {
        if (ConnectionLayer != null)
        {
            var connection = ConnectionLayer.GetNodeOrNull<Node2D>("Connection");
            if (connection != null)
            {
                // _connectionBounds = connection.GetNode<ColorRect>("Bounds");
                // if (_connectionBounds == null)
                // {
                //     GD.PrintErr($"Could not find Bounds ColorRect in Connection for {Name}");
                // }
            }
            else
            {
                GD.PrintErr($"Could not find Connection node in ConnectionLayer for {Name}");
            }
        }
    }

    private Rect2? GetConnectionBounds()
    {
        // Re-fetch bounds if needed (in case original reference was disposed)
        if (_connectionBounds == null || !IsInstanceValid(_connectionBounds))
        {
            if (ConnectionLayer != null)
            {
                var connection = ConnectionLayer.GetNodeOrNull<Node2D>("Connection");
                if (connection != null)
                {
                    _connectionBounds = connection.GetNode<ColorRect>("Bounds");
                }
            }
        }

        // Return bounds rect if we have valid bounds
        if (_connectionBounds != null && IsInstanceValid(_connectionBounds))
        {
            return _connectionBounds.GetGlobalRect();
        }

        return null;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && 
            mouseEvent.ButtonIndex == MouseButton.Left && 
            mouseEvent.Pressed)
        {
            var mousePos = GetViewport().GetMousePosition();
            var globalRect = GetRect();
            
            if (globalRect.HasPoint(mousePos))
            {
                // Check if any block is currently being dragged
                if (GameManager.Instance?.ConnectionLayer != null)
                {
                    var anyBlockDragging = GameManager.Instance.ConnectionLayer
                        .GetChildren()
                        .Cast<Node>()
                        .Where(n => n is BaseBlock)
                        .Cast<BaseBlock>()
                        .Any(b => b != this && b.IsBeingDragged);
                        
                    if (anyBlockDragging)
                    {
                        // Don't allow picking up blocks while another is being dragged
                        return;
                    }
                }

                GetViewport().SetInputAsHandled();
                if (!_inBlockLayer)
                {
                    GD.Print($"Block {Name} clicked in toolbar");
                    EmitSignal(SignalName.BlockPlaced, this);
                }
                else
                {
                    _isBeingDragged = !_isBeingDragged;
                    GD.Print($"Block {Name} drag state changed to {_isBeingDragged}");
                    if (_isBeingDragged)
                    {
                        GameManager.Instance?.HandleBlockDrag(this);
                    }
                    else
                    {
                        GameManager.Instance?.HandleBlockDrop();
                    }
                }
            }
        }
    }

    public override void _Process(double delta)
    {
        if (_isBeingDragged && _inBlockLayer)
        {
            // Follow mouse position freely without bounds checking
            GlobalPosition = GetViewport().GetMousePosition();
        }
    }

    public override void _Draw()
    {
    }

    public bool IsBeingDragged => _isBeingDragged;

    public void SetDragging(bool isDragging)
    {
        GD.Print($"[{Name}] SetDragging called with isDragging={isDragging}, Parent={GetParent()?.Name}");
        
        _isBeingDragged = isDragging;
        
        // Update z-indices based on context
        if (isDragging)
        {
            // Always put dragged blocks on top
            ZIndex = AnimConfig.ZIndex.DraggedBlock;
        }
        else if (_inBlockLayer)
        {
            // When dropping in ConnectionLayer, put below toolbar
            ZIndex = AnimConfig.ZIndex.PlacedBlock;
        }
        
        if (_shadowSprite != null)
        {
            _shadowSprite.ZIndex = -1; // Always keep shadow behind the block
        }
        
        UpdateShadowState();
        
        // Kill existing tweens
        if (_scaleTween != null && _scaleTween.IsValid())
        {
            _scaleTween.Kill();
        }
        if (_shadowTween != null && _shadowTween.IsValid())
        {
            _shadowTween.Kill();
        }

        // Create new animation
        _scaleTween = CreateTween();
        _shadowTween = CreateTween();
        
        // Set up both animations with the same settings
        _scaleTween.SetTrans(Tween.TransitionType.Elastic);
        _scaleTween.SetEase(Tween.EaseType.Out);
        _shadowTween.SetTrans(Tween.TransitionType.Elastic);
        _shadowTween.SetEase(Tween.EaseType.Out);
        
        // Animate scale
        float targetScale = isDragging ? AnimConfig.Toolbar.DraggedBlockScale : 1.0f;
        _scaleTween.TweenProperty(this, "scale", Vector2.One * targetScale, 0.3f);
        
        // Animate shadow position
        if (_shadowSprite != null)
        {
            Vector2 targetPos = isDragging ? new Vector2(20, 20) : Vector2.Zero;
            _shadowTween.TweenProperty(_shadowSprite, "position", targetPos, 0.3f);
        }
    }

    public virtual Rect2 GetRect()
    {
        float size = GameConfig.BLOCK_SIZE;
        Vector2 halfSize = new Vector2(size, size) / 2;
        return new Rect2(GlobalPosition - halfSize, new Vector2(size, size));
    }

    public void Initialize(BlockMetadata metadata)
    {
        _metadata = metadata;
    }

    public void SetInBlockLayer(bool value)
    {
        _inBlockLayer = value;
        UpdateShadowState();
    }

    public bool IsInBlockLayer()
    {
        return _inBlockLayer;
    }

    public virtual void ResetState()
    {
        // Cancel any existing scale animation
        if (_scaleTween != null && _scaleTween.IsValid())
        {
            _scaleTween.Kill();
        }
        if (_shadowTween != null && _shadowTween.IsValid())
        {
            _shadowTween.Kill();
        }

        // Create new scale animation
        _scaleTween = CreateTween();
        _scaleTween.SetTrans(Tween.TransitionType.Elastic);
        _scaleTween.SetEase(Tween.EaseType.Out);
        _scaleTween.TweenProperty(this, "scale", Vector2.One, 0.3f);

        Rotation = 0;
        ZIndex = AnimConfig.ZIndex.Block;
        
        if (_shadowSprite != null)
        {
            _shadowSprite.Visible = false;
            _shadowSprite.Position = Vector2.Zero;
            if (_shadowSprite.Material is ShaderMaterial material)
            {
                material.SetShaderParameter("offset", Vector2.Zero);
                material.SetShaderParameter("blur_amount", 0.0f);
                material.SetShaderParameter("shadow_opacity", 0.0f);
                material.SetShaderParameter("shadow_color", new Color(0, 0, 0, 1));
            }
        }
    }

    public virtual void ProcessToken(Token token)
    {
        if (_metadata != null && _metadata.Action != null)
        {
            token.Value = _metadata.Action(token.Value);
        }

        // Send token to next block
        SendTokenToNextBlock(token);
    }

    public void FinishProcessing(Token token)
    {
        // Mark this block as processed for the token
        token.ProcessedBlocks.Add(this);
        GD.Print($"Block {Name} finished processing token with value {token.Value}");

        // Send token to the next block
        SendTokenToNextBlock(token);
    }

    public void SendTokenToNextBlock(Token token)
    {
        if (ConnectionLayer == null) return;

        var (nextBlock, pipe) = ConnectionLayer.GetNextBlockAndPipe(this);
        
        if (nextBlock != null)
        {
            token.MoveTo(nextBlock);
        }
        else
        {
            // No next block, remove the token from the game
            token.QueueFree();
        }
    }

    public virtual Vector2 GetTokenPosition()
    {
        // Default implementation returns the block's global position
        // Subclasses can override this to customize the token position
        return GlobalPosition;
    }

    public void TriggerAnimation(Token token)
    {
        // Use current position and scale for the animation
        var currentPos = Position;
        var currentScale = Scale;
        var currentRotation = Rotation;
        
        // Calculate intensity multiplier based on number of blocks hit (starting at 1.0, max at 2.0)
        float intensity = Mathf.Min(1.0f + (token.ProcessedBlocks.Count * 0.2f), 2.0f);
        
        Animations.TriggerBlockAnimation(this, currentPos, currentScale, currentRotation, intensity);
    }

    private void UpdateShadowState()
    {
        if (_shadowSprite == null) return;

        var parent = GetParent();
        bool inConnectionLayer = parent?.Name == "ConnectionLayer";
        bool shouldShowShadow = inConnectionLayer && _isBeingDragged;
        
        GD.Print($"[{Name}] UpdateShadowState:");
        GD.Print($"  Parent: {parent?.Name}");
        GD.Print($"  InConnectionLayer: {inConnectionLayer}");
        GD.Print($"  IsBeingDragged: {_isBeingDragged}");
        GD.Print($"  ShouldShowShadow: {shouldShowShadow}");

        // Set visibility
        _shadowSprite.Visible = shouldShowShadow;
        
        // Update shader parameters
        if (_shadowSprite.Material is ShaderMaterial material)
        {
            // Always update parameters whether visible or not
            if (shouldShowShadow)
            {
                material.SetShaderParameter("offset", new Vector2(10f, 10f));
                material.SetShaderParameter("blur_amount", 2.0f);
                material.SetShaderParameter("shadow_opacity", 0.5f);
                material.SetShaderParameter("shadow_color", new Color(0, 0, 0, 1));
            }
            else
            {
                material.SetShaderParameter("offset", Vector2.Zero);
                material.SetShaderParameter("blur_amount", 0.0f);
                material.SetShaderParameter("shadow_opacity", 0.0f);
            }
            GD.Print($"  Material params updated - opacity: {(shouldShowShadow ? 0.5f : 0.0f)}");
        }
        else
        {
            GD.PrintErr($"[{Name}] Shadow sprite has no shader material!");
        }
    }

    public void ClearConnections()
    {
        // Get all sockets
        var sockets = GetChildren()
            .Where(n => n is Node2D && n.Name.ToString().Contains("Socket"))
            .Cast<Node2D>();

        foreach (var socket in sockets)
        {
            // Remove any connected pipes
            var pipes = socket.GetChildren().Where(n => n is ConnectionPipe).ToList();
            foreach (var pipe in pipes)
            {
                pipe.QueueFree();
            }
        }
    }
}
