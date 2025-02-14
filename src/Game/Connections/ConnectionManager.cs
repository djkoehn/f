using F.Audio;
using F.Config.Visual;
using F.UI.Animations;
using F.UI.Toolbar;

namespace F.Game.Connections;

public partial class ConnectionManager : Node2D
{
    private readonly Dictionary<BaseBlock, ConnectionPipe> _blockToPipeMap = new();
    private readonly List<ConnectionPipe> _connections = new();
    private ConnectionFactory? _factory;
    private ConnectionPipe? _hoveredPipe;
    private BaseBlock? _inputBlock;
    private BaseBlock? _outputBlock;
    private ConnectionPipe? _temporaryConnection;
    private ConnectionValidator? _validator;
    private ToolbarHoverAnimation? _currentAnimation;

    public override void _Ready()
    {
        base._Ready();
        ZIndex = ZIndexConfig.Layers.Pipes; // Ensure pipes are drawn above blocks

        var bounds = GetNodeOrNull<ColorRect>("Bounds");
        if (bounds == null) GD.PrintErr("Failed to get Bounds ColorRect!");

        _validator = new ConnectionValidator(bounds!);

        _inputBlock = GetNode<BaseBlock>("Input");
        _outputBlock = GetNode<BaseBlock>("Output");

        if (_inputBlock == null || _outputBlock == null)
        {
            GD.PrintErr("Failed to get Input/Output blocks!");
            return;
        }

        GD.Print("Input Block Type: " + _inputBlock?.GetType());
        GD.Print("Output Block Type: " + _outputBlock?.GetType());

        // Create factory directly since it's a script
        _factory = new ConnectionFactory(GetParent());

        // Defer initial connection creation
        CallDeferred(MethodName.CreateInitialConnection);
    }

    private void CreateInitialConnection()
    {
        if (_inputBlock == null || _outputBlock == null || _factory == null) return;

        GD.Print("Creating initial connection between Input and Output blocks...");
        var pipe = _factory.CreateConnection(_inputBlock, _outputBlock);
        if (pipe != null)
        {
            AddChild(pipe); // Add pipe directly to BlockLayer
            _connections.Add(pipe);
            _blockToPipeMap[_inputBlock] = pipe;
            _blockToPipeMap[_outputBlock] = pipe;
            GD.Print("Initial connection created successfully!");
        }
    }

    public void RemoveConnection(BaseBlock block)
    {
        // Get all connected pipes
        var connectedPipes = _connections
            .Where(p => p.StartBlock == block || p.EndBlock == block)
            .ToList();

        if (!connectedPipes.Any()) return;

        // Find the outermost blocks (skip the block being removed)
        var connectedBlocks = new List<BaseBlock>();
        foreach (var pipe in connectedPipes)
        {
            if (pipe.StartBlock != block)
                connectedBlocks.Add(pipe.StartBlock!);
            if (pipe.EndBlock != block)
                connectedBlocks.Add(pipe.EndBlock!);
        }

        // Find the input-most and output-most blocks
        var inputBlock = connectedBlocks.FirstOrDefault(b => b?.GetType().Name.Contains("Input") ?? false) 
            ?? connectedBlocks.FirstOrDefault();
        var outputBlock = connectedBlocks.LastOrDefault(b => b?.GetType().Name.Contains("Output") ?? false) 
            ?? connectedBlocks.LastOrDefault();

        // Create new connection between input and output blocks if valid
        if (inputBlock != null && outputBlock != null && inputBlock != outputBlock)
        {
            var newPipe = _factory?.CreateConnection(inputBlock, outputBlock);
            if (newPipe != null)
            {
                AddChild(newPipe);
                _connections.Add(newPipe);
                _blockToPipeMap[inputBlock] = newPipe;
                _blockToPipeMap[outputBlock] = newPipe;
                GD.Print($"Created new connection between {inputBlock.Name} and {outputBlock.Name}");
            }
        }

        // Now remove old pipes
        foreach (var pipe in connectedPipes)
        {
            _connections.Remove(pipe);
            pipe.QueueFree();
        }

        _blockToPipeMap.Remove(block);
        GD.Print($"Removed {block.Name} from pipe connections");
    }

    public ConnectionPipe? GetPipeAtPosition(Vector2 position)
    {
        return _connections.FirstOrDefault(p => p.IsPointNearPipe(position));
    }

    public void ClearAllHighlights()
    {
        foreach (var pipe in _connections) pipe.SetHighlighted(false);
    }

    public bool HandleBlockConnection(BaseBlock block, Vector2 position)
    {
        // Get pipe and blocks
        var pipe = GetPipeAtPosition(position);
        if (pipe == null || _factory == null) return false;

        var oldFromBlock = pipe.StartBlock;
        var oldToBlock = pipe.EndBlock;
        if (oldFromBlock == null || oldToBlock == null) return false;

        // Remove old pipe first
        _connections.Remove(pipe);
        pipe.QueueFree();

        // Create and add new pipes
        var pipe1 = _factory.CreateConnection(oldFromBlock, block);
        var pipe2 = _factory.CreateConnection(block, oldToBlock);

        if (pipe1 != null && pipe2 != null)
        {
            AddChild(pipe1);
            AddChild(pipe2);
            _connections.Add(pipe1);
            _connections.Add(pipe2);
            
            // Update mappings
            _blockToPipeMap[oldFromBlock] = pipe1;
            _blockToPipeMap[block] = pipe2;
            _blockToPipeMap[oldToBlock] = pipe2;
            
            return true;
        }

        return false;
    }

    public override void _Process(double delta)
    {
        // Only process if we're still valid
        if (!IsInstanceValid(this) || !IsInsideTree()) return;
    }

    public void DisconnectBlock(BaseBlock block)
    {
        if (block == null) return;

        GD.Print($"Disconnecting block {block.Name} from all pipes");

        // Get all pipes connected to block
        var connectedPipes = _connections
            .Where(p => p.StartBlock == block || p.EndBlock == block)
            .ToList();

        // Remove all pipes
        foreach (var pipe in connectedPipes)
        {
            _connections.Remove(pipe);
            pipe.QueueFree();
        }

        // Clean up block mapping
        _blockToPipeMap.Remove(block);

        GD.Print($"Disconnected {connectedPipes.Count} pipes from block");
    }

    public BaseBlock? GetBlockAtPosition(Vector2 position)
    {
        // Search through all blocks in the BlockLayer
        foreach (var child in GetChildren().OfType<BaseBlock>())
        {
            // Check if position is within block's bounds (adjust radius as needed)
            var distance = (child.GlobalPosition - position).Length();
            if (distance < 50) // 50 pixel radius for connection detection
            {
                GD.Print($"Found block {child.Name} at distance {distance}");
                return child;
            }
        }

        return null;
    }

    public (BaseBlock? nextBlock, ConnectionPipe? pipe) GetNextConnection()
    {
        if (_connections.Count == 0) return (null, null);
        var pipe = _connections.FirstOrDefault();
        if (pipe == null) return (null, null);
        return (pipe.EndBlock, pipe);
    }

    public void StartHoverAnimation(bool show)
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "position:y", show ? -128 : 0, 0.3f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Expo);
    }

    public void SetHoveredPipe(ConnectionPipe? pipe)
    {
        if (_hoveredPipe == pipe) return;
        
        if (_hoveredPipe != null && IsInstanceValid(_hoveredPipe))
            _hoveredPipe.SetHovered(false);
            
        _hoveredPipe = pipe;
        
        if (_hoveredPipe != null && IsInstanceValid(_hoveredPipe))
            _hoveredPipe.SetHovered(true);
    }

    public bool IsBlockConnected(BaseBlock block)
    {
        return _blockToPipeMap.ContainsKey(block);
    }

    public List<ConnectionPipe> GetCurrentConnections(BaseBlock block)
    {
        return _connections
            .Where(p => p.StartBlock == block || p.EndBlock == block)
            .ToList();
    }
}