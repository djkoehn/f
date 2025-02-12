using Godot;
using F.Blocks;
using System.Collections.Generic;
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
                GetViewport().SetInputAsHandled();
                if (!_inBlockLayer)
                {
                    EmitSignal(SignalName.BlockPlaced, this);
                }
                else
                {
                    _isBeingDragged = !_isBeingDragged;
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

    public bool IsBeingDragged => _isBeingDragged;

    public void SetDragging(bool isDragging)
    {
        _isBeingDragged = isDragging;
        if (isDragging)
        {
            Scale = Vector2.One * AnimConfig.Toolbar.DraggedBlockScale;
        }
        else
        {
            Scale = Vector2.One * (IsInBlockLayer() ? 1.0f : AnimConfig.Toolbar.BlockScale);
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
    }

    public bool IsInBlockLayer()
    {
        return _inBlockLayer;
    }

    public virtual void ResetState()
    {
        Scale = Vector2.One * AnimConfig.Toolbar.BlockScale;
        Rotation = 0;
        ZIndex = AnimConfig.ZIndex.Block;
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
}
