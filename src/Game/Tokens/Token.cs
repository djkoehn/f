using F.Audio;
using F.Game.Connections;

namespace F.Game.Tokens;

public partial class Token : Node2D
{
    private ConnectionPipe? _currentPipe;
    private bool _isMoving;
    private BaseBlock? _targetBlock;
    private TokenVisuals? _visuals;
    public float Value { get; set; }

    public HashSet<BaseBlock> ProcessedBlocks { get; } = new();

    public BaseBlock? CurrentBlock { get; private set; }

    public bool IsMoving => !(_visuals?.IsMovementComplete ?? true);

    public void Initialize(BaseBlock startBlock, float initialValue = 0)
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
    }

    public void MoveTo(BaseBlock nextBlock)
    {
        if (nextBlock == null || CurrentBlock == null || _visuals == null) return;

        _targetBlock = nextBlock;
        StartMovement(nextBlock);
    }

    public void StartMovement(BaseBlock targetBlock)
    {
        if (_visuals == null || CurrentBlock == null) return;

        _isMoving = true;
        var startPos = CurrentBlock.GetTokenPosition();
        var endPos = targetBlock.GetTokenPosition();
        var connectionManager = GameManager.Instance?.ConnectionManager;
        if (connectionManager == null) return;
        var (nextBlock, pipe) = connectionManager.GetNextConnection();
        if (pipe != null)
        {
            _currentPipe = pipe;
            _visuals.StartMovement(pipe, startPos, endPos);
        }
    }

    public override void _Process(double delta)
    {
        if (_visuals == null) return;

        if (!_visuals.IsMovementComplete && CurrentBlock != null) GlobalPosition = CurrentBlock.GetTokenPosition();
    }

    private void CompleteMovement()
    {
        if (_targetBlock == null || _visuals == null) return;

        StopMovement();
        _currentPipe = null;
        CurrentBlock = _targetBlock;
        _targetBlock = null;

        // Play block hit sound and trigger animation
        AudioManager.Instance?.PlayBlockHit();
        TriggerHitEffect();
        CurrentBlock.TriggerAnimation(this);

        ProcessedBlocks.Add(CurrentBlock);
        CurrentBlock.ProcessToken(this);
    }

    private void OnMovementComplete()
    {
        _isMoving = false;
        if (CurrentBlock != null) CurrentBlock.ProcessToken(this);
    }

    private void OnMovementStart()
    {
        _isMoving = true;
    }

    public void StopMovement()
    {
        _isMoving = false;
        _visuals?.CompleteMovement();
    }

    public void UpdateValue(float value)
    {
        Value = value;
        _visuals?.UpdateValue(value);
    }

    public float GetValue()
    {
        return _visuals?.GetValue() ?? 0f;
    }

    public void TriggerHitEffect()
    {
        _visuals?.TriggerHitEffect();
    }

    public void TriggerAnimation()
    {
        _visuals?.TriggerAnimation();
    }

    // Compatibility method for old code
    public void MoveToBlock(BaseBlock nextBlock)
    {
        MoveTo(nextBlock);
    }
}