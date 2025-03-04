using F.Game.Tokens;
using F.Audio;
using F.UI.Animations;
using F.Game.Toolbar;
using F.Game.BlockLogic;
using F.Game.Connections.Helpers;
using F.Utils;
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

        // Retrieve the input and output blocks
        _inputBlock = GetNode<F.Game.BlockLogic.Input>("Input");
        _outputBlock = GetNode<F.Game.BlockLogic.Output>("Output");

        if (_inputBlock == null || _outputBlock == null)
        {
            GD.PrintErr("Input or Output block cannot be determined.");
            return;
        }

        // Defer initial connection creation
        CallDeferred(MethodName.CreateInitialConnection);
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
        }
    }

    private void AddPipeToBlock(IBlock block, ConnectionPipe pipe)
    {
        if (!_blockToPipeMap.ContainsKey(block))
        {
            _blockToPipeMap[block] = new List<ConnectionPipe>();
        }
        _blockToPipeMap[block].Add(pipe);
        
        if (block is BaseBlock baseBlock)
        {
            if (pipe.SourceBlock == block) baseBlock.SetOutputConnected(true);
            if (pipe.TargetBlock == block) baseBlock.SetInputConnected(true);
        }
    }

    private void RemovePipeFromBlock(IBlock block, ConnectionPipe pipe)
    {
        if (_blockToPipeMap.TryGetValue(block, out var pipes))
        {
            pipes.Remove(pipe);
            if (pipes.Count == 0) _blockToPipeMap.Remove(block);
            
            if (block is BaseBlock baseBlock)
            {
                if (pipe.SourceBlock == block) baseBlock.SetOutputConnected(false);
                if (pipe.TargetBlock == block) baseBlock.SetInputConnected(false);
            }
        }
    }

    public void RemoveConnection(ConnectionPipe pipe)
    {
        if (pipe.SourceBlock != null) RemovePipeFromBlock(pipe.SourceBlock, pipe);
        if (pipe.TargetBlock != null) RemovePipeFromBlock(pipe.TargetBlock, pipe);
        
        _connections.Remove(pipe);
        _activePipes.Remove(pipe);

        if (pipe.IsInsideTree()) pipe.GetParent()?.RemoveChild(pipe);
        pipe.QueueFree();
    }

    public void ClearConnections()
    {
        var connectedBlocks = new List<IBlock>(_blockToPipeMap.Keys);
        foreach (var block in connectedBlocks)
        {
            DisconnectBlock(block);
        }
    }

    public ConnectionPipe? GetPipeAtPosition(Vector2 position)
    {
        return PipeSelector.GetPipeAtPosition(position, _connections);
    }

    public bool HandleBlockConnection(IBlock block, Vector2 position)
    {
        var pipe = GetPipeAtPosition(position);
        if (pipe?.SourceBlock == null || pipe.TargetBlock == null) return false;

        // Get block names for logging
        string blockName = block.Name ?? block.GetType().Name;
        string sourceName = pipe.SourceBlock.Name ?? pipe.SourceBlock.GetType().Name;
        string targetName = pipe.TargetBlock.Name ?? pipe.TargetBlock.GetType().Name;

        GD.Print($"[ConnectionManager Debug] Attempting to connect block {blockName} between {sourceName} and {targetName}");

        // Special handling for Input block connections
        bool isSourceInput = pipe.SourceBlock is F.Game.BlockLogic.Input;
        
        // If the block we're inserting has connections and we're not inserting after Input, reject
        if (block is BaseBlock baseBlock && baseBlock.HasConnections() && !isSourceInput)
        {
            GD.Print($"[ConnectionManager Debug] Block {blockName} already has connections and not inserting after Input");
            return false;
        }

        ClearAllHighlights();

        // Store the original blocks
        var sourceBlock = pipe.SourceBlock;
        var targetBlock = pipe.TargetBlock;

        // First, reset the new block's connections
        if (block is BaseBlock newBlock)
        {
            DisconnectBlock(block);
            newBlock.ResetConnections();
        }

        // Only remove the specific pipe we're replacing
        RemoveConnection(pipe);
        
        GD.Print($"[ConnectionManager Debug] Creating new connections for block {blockName}");
        
        // Create new connections using CreatePipeForInsertion
        var inputPipe = ConnectionFactory.CreatePipeForInsertion(sourceBlock, block);
        if (inputPipe == null)
        {
            GD.PrintErr($"[ConnectionManager Debug] Failed to create input pipe from {sourceName} to {blockName}");
            RestoreConnection(sourceBlock, targetBlock);
            return false;
        }

        var outputPipe = ConnectionFactory.CreatePipeForInsertion(block, targetBlock);
        if (outputPipe == null)
        {
            GD.PrintErr($"[ConnectionManager Debug] Failed to create output pipe from {blockName} to {targetName}");
            inputPipe.QueueFree();
            RestoreConnection(sourceBlock, targetBlock);
            return false;
        }

        // If both pipes were created successfully, add them
        AddChild(inputPipe);
        AddChild(outputPipe);
        _connections.Add(inputPipe);
        _connections.Add(outputPipe);
        
        // Set up the connections for all blocks
        AddPipeToBlock(sourceBlock, inputPipe);
        AddPipeToBlock(block, inputPipe);
        AddPipeToBlock(block, outputPipe);
        AddPipeToBlock(targetBlock, outputPipe);

        // Set the block's state to connected
        if (block is BaseBlock connectedBlock)
        {
            connectedBlock.CompleteConnection();
            GD.Print($"[ConnectionManager] Successfully connected block {blockName} between {sourceName} and {targetName}");
        }
        
        return true;
    }

    private void RestoreConnection(IBlock sourceBlock, IBlock targetBlock)
    {
        GD.Print($"[ConnectionManager] Restoring connection between {sourceBlock.Name} and {targetBlock.Name}");
        // Use CreatePipeForInsertion here too since we're restoring a connection
        var restoredPipe = ConnectionFactory.CreatePipeForInsertion(sourceBlock, targetBlock);
        if (restoredPipe != null)
        {
            AddChild(restoredPipe);
            _connections.Add(restoredPipe);
            AddPipeToBlock(sourceBlock, restoredPipe);
            AddPipeToBlock(targetBlock, restoredPipe);
        }
    }

    public override void _Process(double delta)
    {
        if (!_processLogged) {
            GD.Print("[ConnectionManager Debug] _Process running");
            _processLogged = true;
        }

        if (!IsInstanceValid(this) || !IsInsideTree()) return;

        var mousePos = GetViewport().GetMousePosition();
        var viewportSize = GetViewport().GetVisibleRect().Size;
        bool shouldShow = (mousePos.Y / viewportSize.Y) > 0.8;

        if (shouldShow != _currentHoverState)
        {
            _currentHoverState = shouldShow;
            ToolbarHoverAnimation.Create(this, shouldShow);
        }
    }

    public bool ConnectBlocks(IBlock sourceBlock, IBlock targetBlock)
    {
        if (IsBlockConnected(sourceBlock)) DisconnectBlock(sourceBlock);
        if (IsBlockConnected(targetBlock)) DisconnectBlock(targetBlock);

        var pipe = _factory.CreateConnection(sourceBlock, targetBlock);
        if (pipe == null) return false;

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

    public bool IsBlockConnected(IBlock block) => 
        _blockToPipeMap.ContainsKey(block) && _blockToPipeMap[block].Count > 0;

    public List<ConnectionPipe> GetCurrentConnections(IBlock block) =>
        _blockToPipeMap.TryGetValue(block, out var pipes) ? new List<ConnectionPipe>(pipes) : new List<ConnectionPipe>();

    public (IBlock? nextBlock, ConnectionPipe? pipe) GetNextConnection(IBlock currentBlock)
    {
        // Find the pipes connected to the current block
        if (!_blockToPipeMap.TryGetValue(currentBlock, out var pipes) || pipes.Count == 0)
            return (null, null);

        // Get the first pipe where current block is the source
        var pipe = pipes.FirstOrDefault(p => p.SourceBlock == currentBlock);
        if (pipe == null) return (null, null);

        // Return the target block of the pipe
        var nextBlock = pipe.TargetBlock;
        GD.Print($"[ConnectionManager Debug] Found next block in chain from {currentBlock.Name}: {nextBlock?.Name}");
        return (nextBlock, pipe);
    }

    // Keep the parameterless version for backward compatibility with Input block
    public (IBlock? nextBlock, ConnectionPipe? pipe) GetNextConnection()
    {
        // Get the Input block as our starting point
        var inputBlock = GetNode<F.Game.BlockLogic.Input>("Input");
        if (inputBlock == null) return (null, null);

        return GetNextConnection(inputBlock);
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

    public void ClearAllHighlights()
    {
        foreach (var pipe in _connections)
        {
            pipe.SetHighlighted(false);
            pipe.ClearInsertionHighlight();
        }
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

    public void SetConnection(IBlock block, ConnectionPipe pipe)
    {
        AddPipeToBlock(block, pipe);
        GD.Print($"[ConnectionManager] Set connection for block {block.Name} to pipe {pipe.Name}");
    }
}