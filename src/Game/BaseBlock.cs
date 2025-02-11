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
    private bool _isProcessingToken = false;
    private Token? _processingToken;
    private float _processingTime = 0f;
    
    // Each block can override this to set its processing duration
    protected virtual float ProcessingDuration => 0.2f;

    [Signal]
    public delegate void BlockPlacedEventHandler(BaseBlock block);

    public override void _Ready()
    {
        base._Ready();
        // Wait a frame to ensure GameManager is initialized
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
        else
        {
            GD.Print($"Found ConnectionLayer for {Name}");
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
            
            // Check if mouse is within block bounds
            if (globalRect.HasPoint(mousePos))
            {
                GetViewport().SetInputAsHandled();
                if (!_inBlockLayer)
                {
                    GD.Print($"Block {Name} clicked in toolbar");
                    EmitSignal(SignalName.BlockPlaced, this);
                }
                else
                {
                    // Toggle drag state when clicked in ConnectionLayer
                    _isBeingDragged = !_isBeingDragged;
                    if (_isBeingDragged)
                    {
                        var gameManager = GameManager.Instance;
                        if (gameManager != null)
                        {
                            gameManager.HandleBlockDrag(this);
                        }
                    }
                    else
                    {
                        var gameManager = GameManager.Instance;
                        if (gameManager != null)
                        {
                            gameManager.HandleBlockDrop();
                        }
                    }
                }
            }
        }
    }

    public override void _Process(double delta)
    {
        if (_isProcessingToken && _processingToken != null)
        {
            _processingTime += (float)delta;
            if (_processingTime >= ProcessingDuration)
            {
                var token = _processingToken;
                _processingToken = null;
                _processingTime = 0f;
                _isProcessingToken = false;
                
                // Continue processing after delay
                OnProcessingComplete(token);
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

    public virtual void ProcessToken(Token token)
    {
        GD.Print($"Processing token in {GetType().Name}, value: {token.Value}");
        
        if (_isProcessingToken) 
        {
            GD.PrintErr($"Block {Name} is already processing a token!");
            return;
        }
        
        if (ConnectionLayer != null)
        {
            _isProcessingToken = true;
            _processingToken = token;
            _processingTime = 0f;
        }
        else
        {
            GD.PrintErr($"No ConnectionLayer found for {GetType().Name}, token processing failed");
            token.QueueFree();
        }
    }

    protected virtual void OnProcessingComplete(Token token)
    {
        GD.Print($"Block {Name} completed processing token with value: {token.Value}");
        
        if (ConnectionLayer != null)
        {
            ConnectionLayer.ProcessTokenThroughBlock(this, token);
        }
    }

    public virtual void ResetState()
    {
        Scale = Vector2.One * AnimConfig.Toolbar.BlockScale;
        Rotation = 0;
        ZIndex = AnimConfig.ZIndex.Block;
    }
}
