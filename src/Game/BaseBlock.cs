using Godot;
using F.Blocks;

namespace F;

public partial class BaseBlock : Node2D
{
    private bool _inBlockLayer;
    protected BlockMetadata? _metadata;
    private Node2D? _inputSocket;
    private Node2D? _outputSocket;
    private bool _isBeingDragged = false;
    protected ConnectionLayer? ConnectionLayer;

    [Signal]
    public delegate void BlockPlacedEventHandler(BaseBlock block);

    public override void _Ready()
    {
        base._Ready();
        CallDeferred(nameof(InitializeConnections));
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
}
