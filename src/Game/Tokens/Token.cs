using F.Audio;
using F.Game.Connections;

namespace F.Game.Tokens;

public partial class Token : Node2D
{
    private ConnectionPipe? _currentPipe;
    private bool _isMoving;
    private IBlock? _targetBlock;
    private TokenVisuals? _visuals;
    public float Value { get; set; }

    public HashSet<IBlock> ProcessedBlocks { get; } = new();

    public IBlock? CurrentBlock { get; private set; }

    public bool IsMoving => !(_visuals?.IsMovementComplete ?? true);

    public void Initialize(IBlock startBlock, float initialValue = 0)
    {
        CurrentBlock = startBlock;
        Value = initialValue;
        GlobalPosition = startBlock.GetTokenPosition();
    }

    public override void _Ready()
    {
        base._Ready();
        _visuals = GetNode<TokenVisuals>("TokenVisuals");
        if (_visuals == null)
        {
            GD.PrintErr("TokenVisuals node not found!");
        }
        else
        {
            _visuals.Connect(TokenVisuals.SignalName.MovementComplete, new Callable(this, nameof(OnMovementComplete)));
            _visuals.Connect(TokenVisuals.SignalName.MovementStart, new Callable(this, nameof(OnMovementStart)));
        }
        
        // Set initial z-index
        ZIndexConfig.SetZIndex(this, ZIndexConfig.Layers.Token);
    }

    public void MoveTo(IBlock nextBlock, ConnectionPipe pipe)
    {
        if (nextBlock == null || CurrentBlock == null || _visuals == null) return;

        _targetBlock = nextBlock;
        _currentPipe = pipe;
        StartMovement(nextBlock);
        
        // Set processing z-index when moving between blocks
        ZIndexConfig.SetZIndex(this, ZIndexConfig.Layers.ProcessingToken);

        // Start pipe animation
        _currentPipe?.StartTokenMovement(this);
    }

    public void StartMovement(IBlock targetBlock)
    {
        if (_visuals == null) return;

        _isMoving = true;
        _targetBlock = targetBlock;
        var targetPosition = targetBlock.GetTokenPosition();
        _visuals.StartMovement(targetPosition);
    }

    public override void _Process(double delta)
    {
        if (!_isMoving || _visuals == null) return;

        // Update token's global position to match visuals
        GlobalPosition = _visuals.GlobalPosition;

        // Update pipe animation with current position
        _currentPipe?.UpdateTokenPosition(this);

        if (_visuals.IsMovementComplete)
        {
            CompleteMovement();
        }
    }

    private void CompleteMovement()
    {
        if (_targetBlock == null) return;

        _isMoving = false;
        CurrentBlock = _targetBlock;
        
        // End pipe animation
        _currentPipe?.EndTokenMovement(this);
        _currentPipe = null;

        // Return to normal z-index after processing
        ZIndexConfig.SetZIndex(this, ZIndexConfig.Layers.Token);
        
        _targetBlock.ProcessToken(this);
    }

    private void OnMovementComplete()
    {
        if (_targetBlock != null)
        {
            CompleteMovement();
        }
    }

    private void OnMovementStart()
    {
        _isMoving = true;
    }

    public void StopMovement()
    {
        _isMoving = false;
        if (_visuals != null)
        {
            _visuals.StopMovement();
        }
    }

    public void UpdateValue(float value)
    {
        Value = value;
        if (_visuals != null)
        {
            _visuals.UpdateValue(value);
        }
    }

    public float GetValue()
    {
        return Value;
    }

    public void TriggerHitEffect()
    {
        if (_visuals != null)
        {
            _visuals.TriggerHitEffect();
        }
    }

    public void TriggerAnimation()
    {
        if (_visuals != null)
        {
            _visuals.TriggerAnimation();
        }
    }

    public void MoveToBlock(IBlock nextBlock)
    {
        if (nextBlock == null || _visuals == null) return;

        _targetBlock = nextBlock;
        StartMovement(nextBlock);
    }
}