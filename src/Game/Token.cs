using System.Collections.Generic;
using Godot;
using F.UI;

namespace F;

public partial class Token : Node2D
{
    public float Value { get; set; }
    private bool _isMoving;
    private BaseBlock? _targetBlock;
    public BaseBlock? CurrentBlock { get; set; }
    private ConnectionPipe? _currentPipe;
    private float _progress = 0f;
    private float _speed = 300f; // Increased speed for better feel
    private float _totalDistance;

    public HashSet<BaseBlock> ProcessedBlocks { get; } = new();

    public override void _Ready()
    {
        ZIndex = AnimConfig.ZIndex.Token;
    }

    public void MoveTo(BaseBlock nextBlock)
    {
        if (nextBlock == null || CurrentBlock == null) return;

        _targetBlock = nextBlock;
        _isMoving = true;

        if (CurrentBlock.ConnectionLayer != null)
        {
            var (_, pipe) = CurrentBlock.ConnectionLayer.GetNextBlockAndPipe(CurrentBlock);
            if (pipe != null)
            {
                _currentPipe = pipe;
                Position = CurrentBlock.GetTokenPosition(); // Set initial position
                _progress = 0f;
                _totalDistance = (_targetBlock.GetTokenPosition() - Position).Length();
            }
        }
    }

    public override void _Process(double delta)
    {
        if (!_isMoving || _currentPipe == null || _targetBlock == null) return;

        _progress = Mathf.Min(_progress + ((float)delta * _speed) / _totalDistance, 1.0f);
        
        // Update position along the curve
        Position = _currentPipe.GetPositionAlongCurve(_progress);
        
        // Update pipe bulge effect
        _currentPipe.UpdateTokenPosition(Position, _progress);

        if (_progress >= 1.0f)
        {
            CompleteMovement();
        }
    }

    private void CompleteMovement()
    {
        if (_targetBlock == null) return;

        // Clear the bulge effect from the pipe before nulling it
        if (_currentPipe != null)
        {
            _currentPipe.ClearBulgeEffect();
        }

        _isMoving = false;
        _currentPipe = null;
        _progress = 0f;
        CurrentBlock = _targetBlock;
        _targetBlock = null;
        
        // Play block hit sound and trigger animation
        AudioManager.Instance?.PlayBlockHit();
        CurrentBlock.TriggerAnimation(this);
        
        ProcessedBlocks.Add(CurrentBlock);
        CurrentBlock.ProcessToken(this);
    }

    // Compatibility method for old code
    public void MoveToBlock(BaseBlock nextBlock)
    {
        MoveTo(nextBlock);
    }
}
