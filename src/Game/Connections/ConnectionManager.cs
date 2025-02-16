using F.Game.Tokens;
using F.Audio;
using F.UI.Animations;
using F.Game.Toolbar;
using F.Game.Blocks;
using ToolbarHoverAnimation = F.UI.Animations.UI.ToolbarHoverAnimation;

namespace F.Game.Connections;

public partial class ConnectionManager : Node2D
{
    private readonly Dictionary<IBlock, List<ConnectionPipe>> _blockToPipeMap = new();
    private readonly List<ConnectionPipe> _connections = new();
    private readonly ConnectionFactory _factory;
    private readonly ConnectionValidator _validator;
    private ConnectionPipe? _hoveredPipe;
    private IBlock? _inputBlock;
    private IBlock? _outputBlock;
    private ConnectionPipe? _temporaryConnection;
    private ToolbarHoverAnimation? _currentAnimation;
    private List<ConnectionPipe> _activePipes = new List<ConnectionPipe>();
    private ConnectionPipe? _currentHighlightedPipe = null;
    private bool _currentHoverState = false;
    private bool _processLogged = false;

    public ConnectionManager()
    {
        GD.Print("[ConnectionManager Debug] Constructor called.");
        _factory = new ConnectionFactory(this);
        var bounds = GetNodeOrNull<ColorRect>("Bounds");
        if (bounds == null) GD.PrintErr("Failed to get Bounds ColorRect!");
        _validator = new ConnectionValidator(bounds!);
    }

    public override void _Ready()
    {
        GD.Print("[ConnectionManager Debug] _Ready called.");
        base._Ready();
        ZIndex = ZIndexConfig.Layers.Pipes;

        // Retrieve the input and output blocks by manually casting
        _inputBlock = GetNode<F.Game.BlockLogic.Input>("Input");
        if (_inputBlock == null)
        {
            GD.PrintErr("Input block cannot be determined. Make sure that the 'Input' node is an instance of an IBlock.");
        }

        _outputBlock = GetNode<F.Game.BlockLogic.Output>("Output");
        if (_outputBlock == null)
        {
            GD.PrintErr("Output block cannot be determined. Make sure that the 'Output' node is an instance of an IBlock.");
        }

        GD.Print("Input Block Type: " + _inputBlock?.GetType());
        GD.Print("Output Block Type: " + _outputBlock?.GetType());

        // Defer initial connection creation
        CallDeferred(MethodName.CreateInitialConnection);

        // NOTE: Ensure that no node in your scene is instantiating ConnectionHelper since it is now a static utility class.
    }

    private void CreateInitialConnection()
    {
        if (_inputBlock == null || _outputBlock == null) return;

        GD.Print("Creating initial connection between Input and Output blocks...");
        var pipe = _factory.CreateConnection(_inputBlock, _outputBlock);
        if (pipe != null)
        {
            AddChild(pipe);
            _connections.Add(pipe);
            AddPipeToBlock(_inputBlock, pipe);
            AddPipeToBlock(_outputBlock, pipe);
            GD.Print("Initial connection created successfully!");
        }
    }

    private void AddPipeToBlock(IBlock block, ConnectionPipe pipe)
    {
        if (!_blockToPipeMap.ContainsKey(block))
        {
            _blockToPipeMap[block] = new List<ConnectionPipe>();
        }
        _blockToPipeMap[block].Add(pipe);
        
        // Update block's connection state if it's a BaseBlock
        if (block is BaseBlock baseBlock)
        {
            if (pipe.SourceBlock == block)
            {
                baseBlock.SetOutputConnected(true);
            }
            if (pipe.TargetBlock == block)
            {
                baseBlock.SetInputConnected(true);
            }
        }
    }

    private void RemovePipeFromBlock(IBlock block, ConnectionPipe pipe)
    {
        if (_blockToPipeMap.TryGetValue(block, out var pipes))
        {
            pipes.Remove(pipe);
            if (pipes.Count == 0)
            {
                _blockToPipeMap.Remove(block);
            }
            
            // Update block's connection state if it's a BaseBlock
            if (block is BaseBlock baseBlock)
            {
                if (pipe.SourceBlock == block)
                {
                    baseBlock.SetOutputConnected(false);
                }
                if (pipe.TargetBlock == block)
                {
                    baseBlock.SetInputConnected(false);
                }
            }
        }
    }

    public void RemoveConnection(ConnectionPipe pipe)
    {
        // Remove from collections
        if (pipe.SourceBlock != null)
            RemovePipeFromBlock(pipe.SourceBlock, pipe);
        if (pipe.TargetBlock != null)
            RemovePipeFromBlock(pipe.TargetBlock, pipe);
        
        _connections.Remove(pipe);
        _activePipes.Remove(pipe);

        // Remove from scene if still attached
        if (pipe.IsInsideTree())
        {
            pipe.GetParent()?.RemoveChild(pipe);
        }
        pipe.QueueFree();
    }

    public void ClearConnections()
    {
        var connectedBlocks = new List<IBlock>();
        foreach (var (block, pipes) in _blockToPipeMap)
        {
            foreach (var pipe in pipes)
            {
                pipe.QueueFree();
            }
            connectedBlocks.Add(block);
        }

        foreach (var block in connectedBlocks)
        {
            _blockToPipeMap.Remove(block);
        }
    }

    public ConnectionPipe? GetPipeAtPosition(Vector2 position)
    {
        float hoverDistance = PipeConfig.Interaction.HoverDistance;

        GD.Print($"[ConnectionManager Debug] Checking for pipe at position {position}");
        GD.Print($"[ConnectionManager Debug] Active pipes: {_activePipes.Count}, Connection pipes: {_connections.Count}");

        // First check active pipes (they take precedence)
        foreach (var pipe in _activePipes)
        {
            if (pipe.IsPointNearPipe(position))
            {
                GD.Print($"[ConnectionManager Debug] Found active pipe - Source: {pipe.SourceBlock?.GetType()}, Target: {pipe.TargetBlock?.GetType()}");
                return pipe;
            }
        }

        // Then check regular connections
        foreach (var pipes in _blockToPipeMap.Values)
        {
            foreach (var pipe in pipes)
            {
                if (pipe.IsPointNearPipe(position))
                {
                    GD.Print($"[ConnectionManager Debug] Found connection pipe - Source: {pipe.SourceBlock?.GetType()}, Target: {pipe.TargetBlock?.GetType()}");
                    return pipe;
                }
            }
        }

        GD.Print("[ConnectionManager Debug] No pipe found at position");
        return null;
    }

    public void ClearAllHighlights()
    {
        foreach (var pipes in _blockToPipeMap.Values)
        {
            foreach (var pipe in pipes)
            {
                pipe.SetHighlighted(false);
            }
        }
    }

    public bool HandleBlockConnection(IBlock block, Vector2 position)
    {
        GD.Print("[ConnectionManager Debug] HandleBlockConnection triggered for block: " + block.Name);
        var pipe = GetPipeAtPosition(position);
        if (pipe == null)
        {
            GD.Print("[ConnectionManager] No pipe close enough for connection.");
            return false;
        }

        // Verify pipe has valid blocks
        if (pipe.SourceBlock == null || pipe.TargetBlock == null)
        {
            GD.PrintErr("[ConnectionManager] Pipe has null blocks");
            return false;
        }

        // Log the types of blocks involved
        GD.Print($"[ConnectionManager] Attempting to insert {block.GetType().Name} between {pipe.SourceBlock.GetType().Name} and {pipe.TargetBlock.GetType().Name}");

        if (block is BaseBlock baseBlock) {
            bool hasConn = baseBlock.HasConnections();
            GD.Print($"[ConnectionManager Debug] Block {block.Name} HasConnections() returned: {hasConn}");
            if (hasConn) {
                GD.PrintErr("[ConnectionManager] Block already has connections.");
                return false;
            }
        }

        // Clear any existing highlights before attempting connection
        ClearAllHighlights();
        ClearInsertionHighlights();

        bool inserted = F.Game.Connections.Helpers.PipeRewiringHelper.InsertBlockIntoPipe(block, pipe, _factory, this);
        if (inserted)
        {
            // Remove the original pipe
            pipe.RemovePipe();
            _connections.Remove(pipe);
            RemovePipeFromBlock(pipe.SourceBlock, pipe);
            RemovePipeFromBlock(pipe.TargetBlock, pipe);

            // Create new Input -> Block and Block -> Output pipes
            var inputPipe = _factory.CreateConnection(pipe.SourceBlock, block);
            var outputPipe = _factory.CreateConnection(block, pipe.TargetBlock);
            
            if (inputPipe != null)
            {
                AddChild(inputPipe);
                _connections.Add(inputPipe);
                AddPipeToBlock(pipe.SourceBlock, inputPipe);
                AddPipeToBlock(block, inputPipe);
            }
            else
            {
                GD.PrintErr("Failed to create input pipe");
            }

            if (outputPipe != null)
            {
                AddChild(outputPipe);
                _connections.Add(outputPipe);
                AddPipeToBlock(block, outputPipe);
                AddPipeToBlock(pipe.TargetBlock, outputPipe);
            }
            else
            {
                GD.PrintErr("Failed to create output pipe");
            }
        }

        return inserted;
    }

    public override void _Process(double delta)
    {
        if (!_processLogged) {
            GD.Print("[ConnectionManager Debug] _Process running");
            _processLogged = true;
        }

        // Only process if we're still valid
        if (!IsInstanceValid(this) || !IsInsideTree()) return;

        // Calculate hover state based on mouse position relative to viewport
        var mousePos = GetViewport().GetMousePosition();
        var viewportSize = GetViewport().GetVisibleRect().Size;
        bool shouldShow = (mousePos.Y / viewportSize.Y) > 0.8;  // bottom 20% of screen

        // If the hover state has changed, trigger the animation
        if (shouldShow != _currentHoverState)
        {
            _currentHoverState = shouldShow;
            ToolbarHoverAnimation.Create(this, shouldShow);
        }
    }

    public bool ConnectBlocks(IBlock sourceBlock, IBlock targetBlock)
    {
        // Check if either block is already connected
        if (IsBlockConnected(sourceBlock))
        {
            DisconnectBlock(sourceBlock);
        }
        if (IsBlockConnected(targetBlock)) 
        {
            DisconnectBlock(targetBlock);
        }

        var pipe = _factory.CreateConnection(sourceBlock, targetBlock);
        if (pipe == null)
        {
            GD.PrintErr($"Failed to create connection between {sourceBlock} and {targetBlock}");
            return false;
        }

        AddChild(pipe);
        _connections.Add(pipe);
        AddPipeToBlock(sourceBlock, pipe);
        AddPipeToBlock(targetBlock, pipe);

        return true;
    }

    public void DisconnectBlock(IBlock block)
    {
        var connectionsToRemove = GetCurrentConnections(block);
        foreach (var pipe in connectionsToRemove)
        {
            RemoveConnection(pipe);
        }
    }

    public bool IsBlockConnected(IBlock block)
    {
        return _blockToPipeMap.ContainsKey(block) && _blockToPipeMap[block].Count > 0;
    }

    public List<ConnectionPipe> GetCurrentConnections(IBlock block)
    {
        if (_blockToPipeMap.TryGetValue(block, out var pipes))
        {
            return new List<ConnectionPipe>(pipes);
        }
        return new List<ConnectionPipe>();
    }

    public void AddPipe(ConnectionPipe pipe)
    {
        _activePipes.Add(pipe);
        AddChild(pipe);
    }

    public void RemovePipe(ConnectionPipe pipe)
    {
        if (_activePipes.Contains(pipe))
        {
            _activePipes.Remove(pipe);
            pipe.QueueFree();
        }
    }

    public (IBlock? nextBlock, ConnectionPipe? pipe) GetNextConnection()
    {
        if (_blockToPipeMap.Count == 0) return (null, null);
        var firstEntry = _blockToPipeMap.First();
        var block = firstEntry.Key;
        var pipe = firstEntry.Value.First();
        var nextBlock = pipe.GetOtherBlock(block);
        return (nextBlock, pipe);
    }

    public void SetHoveredPipe(ConnectionPipe? pipe)
    {
        if (_hoveredPipe == pipe) return;
        if (_hoveredPipe != null && IsInstanceValid(_hoveredPipe))
            _hoveredPipe.SetHighlighted(false);
        _hoveredPipe = pipe;
        if (_hoveredPipe != null && IsInstanceValid(_hoveredPipe))
            _hoveredPipe.SetHighlighted(true);
    }

    public void ClearInsertionHighlights()
    {
        foreach (var pipes in _blockToPipeMap.Values)
        {
            foreach (var pipe in pipes)
            {
                pipe.ClearInsertionHighlight();
            }
        }
    }

    public void SetConnection(IBlock block, ConnectionPipe pipe)
    {
        AddPipeToBlock(block, pipe);
        GD.Print($"[ConnectionManager] Set connection for block {block.Name} to pipe {pipe.Name}");
    }
}