namespace F.UI.Toolbar;

public partial class ToolbarVisuals : Control
{
    private const float CONTAINER_ANIMATION_DURATION = 0.5f;
    private ColorRect? _backgroundContainer;
    private HBoxContainer? _blockContainer;
    private float _containerAnimTime;
    private float _containerWidth;
    private float _targetContainerWidth;

    public override void _Ready()
    {
        base._Ready();
        SetupToolbarControl();

        _backgroundContainer = GetNode<ColorRect>("Background");
        if (_backgroundContainer == null) GD.PrintErr("Background not found!");

        // Get the reference to the BlockContainer from parent
        _blockContainer = GetParent().GetNode<HBoxContainer>("BlockContainer");
        if (_blockContainer == null) GD.PrintErr("BlockContainer not found in parent!");

        Position = new Vector2(0, 256); // Start hidden
    }

    private void SetupToolbarControl()
    {
        CustomMinimumSize = new Vector2(1920, 256);

        // Use full width anchors
        AnchorLeft = 0;
        AnchorRight = 1;
        AnchorTop = 0.5f;
        AnchorBottom = 0.5f;
        GrowHorizontal = GrowDirection.Both;
        GrowVertical = GrowDirection.Both;
    }

    public void UpdatePosition(float targetOffset)
    {
        // Create a new tween for the parent node (Toolbar)
        var tween = GetParent().CreateTween();
        tween.TweenProperty(GetParent(), "position:y", targetOffset, 0.3f)
            .SetTrans(Tween.TransitionType.Spring)
            .SetEase(Tween.EaseType.Out);
    }

    public void UpdateBlockPositions()
    {
        if (_blockContainer == null) return;

        float totalWidth = 0;
        var children = _blockContainer.GetChildren();

        if (children.Count == 0)
        {
            _targetContainerWidth = 200; // Minimum width
            _containerAnimTime = 0;
            return;
        }

        totalWidth = children.Count * 200; // Fixed width per block slot
        totalWidth += (children.Count - 1) * 40; // Spacing
        totalWidth += 80; // Padding

        _targetContainerWidth = totalWidth;
        _containerAnimTime = 0;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!(Mathf.Abs(_containerWidth - _targetContainerWidth) > 0.1f)) return;
        _containerAnimTime += (float)delta;
        var t = Mathf.Min(_containerAnimTime / CONTAINER_ANIMATION_DURATION, 1.0f);
        _containerWidth = Mathf.Lerp(_containerWidth, _targetContainerWidth, t);

        if (_blockContainer == null) return;
        _blockContainer.CustomMinimumSize = new Vector2(_containerWidth, _blockContainer.CustomMinimumSize.Y);
        _blockContainer.Size = _blockContainer.CustomMinimumSize;
        _blockContainer.ForceUpdateTransform();
    }
}