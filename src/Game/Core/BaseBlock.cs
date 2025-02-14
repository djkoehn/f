using F.Config.Visual;
using F.Game.Connections;
using F.Game.Tokens;
using F.Config;

namespace F.Game.Core;

public partial class BaseBlock : Node2D
{
    [Signal]
    public delegate void BlockClickedEventHandler(BaseBlock block);

    [Signal]
    public delegate void BlockPlacedEventHandler(BaseBlock block);

    [Signal]
    public delegate void BlockRightClickedEventHandler(BaseBlock block); // ADD RIGHT CLICK SIGNAL!

    private bool _canReceiveInput = true; // ADD INPUT FLAG
    private Area2D? _collisionArea;
    protected ConnectionManager? _connectionManager; // Change access level to protected
    private Vector2 _dragOffset;
    protected bool _inToolbar;
    protected BlockConfig? _metadata;
    private Vector2 _originalPosition;
    private float _originalRotation;
    private Vector2 _originalScale;
    private Tween? _scaleTween;
    private Sprite2D? _shadowSprite;
    private Tween? _shadowTween;
    public bool InToolbar => _inToolbar;

    public bool IsBeingDragged { get; private set; }

    public bool IsPlaced { get; private set; }

    public virtual Vector2 Size => new(100, 100); // Remove 'new' keyword

    public virtual Vector2 GetTokenPosition()
    {
        var tokenSocket = GetNodeOrNull<Node2D>("TokenSocket");
        return tokenSocket?.GlobalPosition ?? GlobalPosition;
    }

    public bool HasConnections()
    {
        if (_connectionManager == null) return false;
        var pipe = _connectionManager.GetPipeAtPosition(GlobalPosition);
        return pipe != null;
    }

    public override void _Ready()
    {
        base._Ready();
        CreateCollisionArea();
        CreateShadowSprite();

        // Store original transform
        _originalPosition = Position;
        _originalScale = Scale;
        _originalRotation = Rotation;
    }

    private void CreateShadowSprite()
    {
        _shadowSprite = new Sprite2D
        {
            ZIndex = -1, // Shadow should be behind the block
            Modulate = new Color(0, 0, 0, 0.3f),
            Visible = false
        };
        AddChild(_shadowSprite);
    }

    private void CreateCollisionArea()
    {
        var collisionArea = new Area2D { Name = "Area2D" };
        var collisionShape = new CollisionShape2D();
        var shape = new RectangleShape2D();
        shape.Size = Size;
        collisionShape.Shape = shape;
        collisionArea.AddChild(collisionShape);
        AddChild(collisionArea);
    }

    public virtual void OnDragStart(Vector2 clickPosition)
    {
        GD.Print($"Dragging started for block at position: {GlobalPosition}"); // Debug print
        IsBeingDragged = true;

        // If block is placed, we need to unset that state
        if (IsPlaced) SetPlaced(false);

        _dragOffset = GlobalPosition - clickPosition;

        // Start drag animation
        if (_scaleTween != null && _scaleTween.IsValid()) _scaleTween.Kill();

        _scaleTween = CreateTween();
        _scaleTween.TweenProperty(this, "scale", _originalScale * 1.1f, 0.1f);

        // Show shadow
        if (_shadowSprite != null)
        {
            _shadowSprite.Visible = true;
            if (_shadowTween != null && _shadowTween.IsValid()) _shadowTween.Kill();
            _shadowTween = CreateTween();
            _shadowTween.TweenProperty(_shadowSprite, "modulate:a", 0.3f, 0.1f);
        }
    }

    public virtual void OnDragEnd()
    {
        GD.Print($"Dragging ended for block at position: {GlobalPosition}"); // Debug print
        IsBeingDragged = false;

        // End drag animation
        if (_scaleTween != null && _scaleTween.IsValid()) _scaleTween.Kill();

        _scaleTween = CreateTween();
        _scaleTween.TweenProperty(this, "scale", _originalScale, 0.1f);

        // Hide shadow
        if (_shadowSprite != null)
        {
            if (_shadowTween != null && _shadowTween.IsValid()) _shadowTween.Kill();
            _shadowTween = CreateTween();
            _shadowTween.TweenProperty(_shadowSprite, "modulate:a", 0f, 0.1f);
            _shadowTween.TweenCallback(Callable.From(() => _shadowSprite.Visible = false));
        }
    }

    public virtual void OnPlaced()
    {
        IsPlaced = true;
        EmitSignal(SignalName.BlockPlaced, this);
    }

    public virtual void OnRemoved()
    {
        IsPlaced = false;
    }

    public virtual void Initialize(BlockConfig metadata)
    {
        _metadata = metadata;
        Name = metadata.Name;
    }

    public virtual void ProcessToken(Token token)
    {
        // Default implementation
    }

    public void TriggerAnimation(Token token)
    {
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Elastic);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(this, "scale", Vector2.One * 1.2f, 0.3f);
        tween.TweenProperty(this, "scale", Vector2.One, 0.3f);
    }

    public void SetDragging(bool isDragging)
    {
        IsBeingDragged = isDragging;
        if (isDragging)
            OnDragStart(GlobalPosition);
        else
            OnDragEnd();
    }

    public void SetPlaced(bool isPlaced)
    {
        IsPlaced = isPlaced;

        // Enable/disable collision based on placement
        var collision = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (collision != null) collision.Disabled = !isPlaced;

        // Enable/disable socket areas
        var socketAreas = GetChildren().OfType<Area2D>();
        foreach (var area in socketAreas)
        {
            area.Monitorable = isPlaced;
            area.Monitoring = isPlaced;
        }
    }

    public bool IsDragging()
    {
        return IsBeingDragged;
    }

    public Node2D? GetNearestSocket(Vector2 point)
    {
        return GetChildren()
            .OfType<Node2D>()
            .Where(n => n.Name.ToString().Contains("Socket"))
            .OrderBy(s => s.GlobalPosition.DistanceTo(point))
            .FirstOrDefault();
    }

    public override void _Input(InputEvent @event)
    {
        if (!_canReceiveInput) return;

        if (@event is InputEventMouseButton mouseButton)
        {
            var mousePos = GetViewport().GetMousePosition();
            var blockBounds = new Rect2(GlobalPosition - new Vector2(48, 48), new Vector2(96, 96));

            if (blockBounds.HasPoint(mousePos))
                if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed)
                {
                    GD.Print($"Block {Name} wants to go home!");
                    EmitSignal(SignalName.BlockRightClicked, this);
                    GetViewport().SetInputAsHandled();
                }
        }
    }

    public void SetCanReceiveInput(bool canReceive)
    {
        _canReceiveInput = canReceive;
    }

    public bool CanReceiveInput()
    {
        return _canReceiveInput;
    }

    public void SetInToolbar(bool value)
    {
        _inToolbar = value;
        if (_inToolbar)
        {
            // When in toolbar, make sure other states cleared
            SetDragging(false);
            SetPlaced(false);
            ZIndex = ZIndexConfig.Layers.ToolbarBlock;
        }
    }
}