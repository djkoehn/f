using F.Framework.Blocks;
using F.Framework.Connections.Interfaces;
using F.Framework.Core.Services;
using F.Framework.Logging;
using F.Game.Toolbar;
using F.Framework.Core.SceneTree;
using F.Game.Connections;
using F.UI.Animations.UI;

namespace F.Framework.Connections;

public partial class ConnectionManager : Node2D, IConnectionManager
{
    private readonly List<ConnectionPipe> _activePipes = new();
    private readonly Dictionary<IBlock, List<ConnectionPipe>> _blockConnections = new();
    private readonly ConnectionFactory _factory;
    private ConnectionValidator _validator;
    private ConnectionPipe? _currentHighlightedPipe;
    private bool _currentHoverState;
    private ConnectionPipe? _hoveredPipe;
    private IBlock? _inputBlock;
    private IBlock? _outputBlock;
    private bool _processLogged;
    private ConnectionPipe? _temporaryConnection;
    private ColorRect? _bounds;
    private const float RETRY_INTERVAL = 0.1f;
    private const int MAX_RETRIES = 10;
    private int _initRetryCount;

    public ConnectionManager()
    {
        Logger.Connection.Print("Constructor called.");
        _factory = new ConnectionFactory(this);
        _validator = new ConnectionValidator(null!);
    }

    public override void _Ready()
    {
        Logger.Connection.Print("Ready called.");
        base._Ready();
        ZIndex = ZIndexConfig.Layers.Pipes;

        InitializeManager();
    }

    private void InitializeManager()
    {
        // Get dependencies from Services
        _inputBlock = Services.Instance?.Game?.BlockLayer?.Input;
        _outputBlock = Services.Instance?.Game?.BlockLayer?.Output;

        // Get the bounds from the correct path
        _bounds = GetNode<ColorRect>("/root/Main/BlockLayer/Bounds/Background");
        if (_bounds == null)
        {
            Logger.Connection.Err("Failed to get Bounds ColorRect!");
        }

        // Create a new validator with the bounds
        _validator = new ConnectionValidator(_bounds ?? null!);

        if (_inputBlock?.Metadata == null || _outputBlock?.Metadata == null)
        {
            _initRetryCount++;
            if (_initRetryCount >= MAX_RETRIES)
            {
                Logger.Connection.Err($"Failed to initialize after {MAX_RETRIES} attempts");
                return;
            }

            Logger.Connection.Print($"Dependencies not ready, retry {_initRetryCount}/{MAX_RETRIES} in {RETRY_INTERVAL}s");
            var timer = new Timer
            {
                OneShot = true,
                WaitTime = RETRY_INTERVAL
            };
            AddChild(timer);
            timer.Timeout += () =>
            {
                timer.QueueFree();
                InitializeManager();
            };
            timer.Start();
            return;
        }

        Logger.Connection.Print($"Successfully initialized with Input block ({_inputBlock.Name}) and Output block ({_outputBlock.Name})");

        // Defer initial connection creation
        CallDeferred(MethodName.CreateInitialConnection);
    }

    public void DisconnectBlock(IBlock block)
    {
        if (block == null) return;

        Logger.Connection.Print($"Disconnecting block {block.Name}");

        // Get all connections for this block
        if (_blockConnections.TryGetValue(block, out var connections))
        {
            // Create a new list to avoid modification during enumeration
            var connectionsToRemove = connections.ToList();
            foreach (var pipe in connectionsToRemove) RemoveConnection(pipe);
        }

        // Clear the block's connection state
        if (block is BaseBlock baseBlock) baseBlock.ResetConnections();

        _blockConnections.Remove(block);
    }

    public ConnectionPipe? GetPipeAtPosition(Vector2 position)
    {
        return PipeSelector.GetPipeAtPosition(position, _activePipes);
    }

    public bool HandleBlockConnection(IBlock block, Vector2 position)
    {
        var pipe = GetPipeAtPosition(position);
        if (pipe?.SourceBlock == null || pipe.TargetBlock == null) return false;

        // Get block names for logging
        var blockName = block.Name ?? "unknown";
        var sourceName = pipe.SourceBlock.Name ?? "unknown";
        var targetName = pipe.TargetBlock.Name ?? "unknown";

        Logger.Connection.Print($"Attempting to connect block {blockName} between {sourceName} and {targetName}");

        // Prevent self-connections
        if (block == pipe.SourceBlock || block == pipe.TargetBlock)
        {
            Logger.Connection.Print($"Rejected self-connection attempt for block {blockName}");
            return false;
        }

        // Special handling for Input block connections
        var isSourceInput = pipe.SourceBlock.Metadata?.Id == "input";

        // If the block we're inserting has connections and we're not inserting after Input, reject
        if (block is BaseBlock baseBlock && baseBlock.HasConnections() && !isSourceInput)
        {
            Logger.Connection.Print($"Block {blockName} already has connections and not inserting after Input");
            return false;
        }

        // First ensure we disconnect any existing connections for the block
        DisconnectBlock(block);
        ClearAllHighlights();

        // Store the original blocks
        var sourceBlock = pipe.SourceBlock;
        var targetBlock = pipe.TargetBlock;

        // Reset the new block's connections if it's a BaseBlock
        if (block is BaseBlock newBlock) newBlock.ResetConnections();

        // Only remove the specific pipe we're replacing
        RemoveConnection(pipe);

        Logger.Connection.Print($"Creating new connections for block {blockName}");

        // Create new connections using CreatePipeForInsertion
        var inputPipe = ConnectionFactory.CreatePipeForInsertion(sourceBlock, block);
        if (inputPipe == null)
        {
            Logger.Connection.Err($"Failed to create input pipe from {sourceName} to {blockName}");
            RestoreConnection(sourceBlock, targetBlock);
            return false;
        }

        var outputPipe = ConnectionFactory.CreatePipeForInsertion(block, targetBlock);
        if (outputPipe == null)
        {
            Logger.Connection.Err($"Failed to create output pipe from {blockName} to {targetName}");
            inputPipe.QueueFree();
            RestoreConnection(sourceBlock, targetBlock);
            return false;
        }

        // Get the BlockLayerContent node
        var content = GetBlockLayerContent();
        if (content == null)
        {
            Logger.Connection.Err("BlockLayerContent not found");
            inputPipe.QueueFree();
            outputPipe.QueueFree();
            RestoreConnection(sourceBlock, targetBlock);
            return false;
        }

        // Add pipes to BlockLayerContent
        content.AddChild(inputPipe);
        content.AddChild(outputPipe);
        _activePipes.Add(inputPipe);
        _activePipes.Add(outputPipe);

        // Set up the connections for all blocks
        AddPipeToBlock(sourceBlock, inputPipe);
        AddPipeToBlock(block, inputPipe);
        AddPipeToBlock(block, outputPipe);
        AddPipeToBlock(targetBlock, outputPipe);

        // Set the block's state to connected
        if (block is BaseBlock connectedBlock)
        {
            connectedBlock.CompleteConnection();
            Logger.Connection.Print($"Successfully connected block {blockName} between {sourceName} and {targetName}");
        }

        return true;
    }

    public override void _Process(double delta)
    {
        if (!_processLogged)
        {
            Logger.Connection.Print("_Process running");
            _processLogged = true;
        }

        if (!IsInstanceValid(this) || !IsInsideTree()) return;

        var mousePos = GetViewport().GetMousePosition();
        var viewportSize = GetViewport().GetVisibleRect().Size;
        var shouldShow = mousePos.Y / viewportSize.Y > 0.8;

        if (shouldShow != _currentHoverState)
        {
            _currentHoverState = shouldShow;
            ToolbarHoverAnimation.Create(this, shouldShow);
        }
    }

    public void ConnectBlocks(IBlock sourceBlock, IBlock targetBlock)
    {
        if (sourceBlock == null || targetBlock == null)
        {
            Logger.Connection.Err("[ConnectionManager] Cannot connect null blocks");
            return;
        }

        Logger.Connection.Print($"Creating new connection between {sourceBlock.Name} -> {targetBlock.Name}");

        // First disconnect any existing connections
        if (sourceBlock is BaseBlock sourceBlockInstance) sourceBlockInstance.ResetConnections();
        if (targetBlock is BaseBlock targetBlockInstance) targetBlockInstance.ResetConnections();

        // Create the connection pipe
        var pipe = _factory.CreatePipe(sourceBlock, targetBlock);
        if (pipe != null)
        {
            // Add to our connection tracking
            if (!_blockConnections.ContainsKey(sourceBlock))
                _blockConnections[sourceBlock] = new List<ConnectionPipe>();
            if (!_blockConnections.ContainsKey(targetBlock))
                _blockConnections[targetBlock] = new List<ConnectionPipe>();

            _blockConnections[sourceBlock].Add(pipe);
            _blockConnections[targetBlock].Add(pipe);

            // Update block states
            if (sourceBlock is BaseBlock sourceBlockInst) sourceBlockInst.SetOutputConnected(true);
            if (targetBlock is BaseBlock targetBlockInst) targetBlockInst.SetInputConnected(true);

            Logger.Connection.Print($"Successfully connected blocks {sourceBlock.Name} -> {targetBlock.Name}");
        }
    }

    public bool IsBlockConnected(IBlock block)
    {
        return _blockConnections.ContainsKey(block) && _blockConnections[block].Count > 0;
    }

    public List<ConnectionPipe> GetCurrentConnections(IBlock block)
    {
        if (_blockConnections.TryGetValue(block, out var connections)) return new List<ConnectionPipe>(connections);
        return new List<ConnectionPipe>();
    }

    public (IBlock? nextBlock, ConnectionPipe? pipe) GetNextConnection(IBlock currentBlock)
    {
        // Find the pipes connected to the current block
        if (!_blockConnections.TryGetValue(currentBlock, out var pipes) || pipes.Count == 0)
            return (null, null);

        // Get the first pipe where current block is the source
        var pipe = pipes.FirstOrDefault(p => p.SourceBlock == currentBlock);
        if (pipe == null) return (null, null);

        // Return the target block of the pipe
        var nextBlock = pipe.TargetBlock;
        Logger.Connection.Print($"Found next block in chain from {currentBlock.Name}: {nextBlock?.Name}");
        return (nextBlock, pipe);
    }

    public void ClearAllHighlights()
    {
        foreach (var pipe in _activePipes)
        {
            pipe.SetHighlighted(false);
            pipe.ClearInsertionHighlight();
        }
    }

    private Node2D? GetBlockLayerContent()
    {
        return Services.Instance?.Game?.BlockLayer as Node2D;
    }

    private void CreateInitialConnection()
    {
        if (_inputBlock == null || _outputBlock == null) return;

        Logger.Connection.Print($"Creating initial connection between {_inputBlock.Name} and {_outputBlock.Name} blocks...");
        var pipe = ConnectionFactory.CreatePipeForInsertion(_inputBlock, _outputBlock);
        if (pipe != null)
        {
            // Get the BlockLayerContent node
            var content = GetBlockLayerContent();
            if (content != null)
            {
                // Add pipe to BlockLayerContent
                content.AddChild(pipe);
                // Set pipe z-index
                ZIndexConfig.SetZIndex(pipe, ZIndexConfig.Layers.Pipes);
                _activePipes.Add(pipe);
                AddPipeToBlock(_inputBlock, pipe);
                AddPipeToBlock(_outputBlock, pipe);
                Logger.Connection.Print("Successfully created initial connection in BlockLayerContent");
            }
            else
            {
                Logger.Connection.Err("BlockLayerContent not found, initial connection failed");
                pipe.QueueFree();
            }
        }
    }

    private void AddPipeToBlock(IBlock block, ConnectionPipe pipe)
    {
        if (!_blockConnections.ContainsKey(block)) _blockConnections[block] = new List<ConnectionPipe>();
        _blockConnections[block].Add(pipe);

        if (block is BaseBlock baseBlock)
        {
            if (pipe.SourceBlock == block) baseBlock.SetOutputConnected(true);
            if (pipe.TargetBlock == block) baseBlock.SetInputConnected(true);
            Logger.Connection.Print($"Set connection for block {block.Name} to pipe {pipe.Name}");
        }
    }

    private void RemovePipeFromBlock(IBlock block, ConnectionPipe pipe)
    {
        if (_blockConnections.TryGetValue(block, out var pipes))
        {
            pipes.Remove(pipe);
            if (pipes.Count == 0) _blockConnections.Remove(block);

            if (block is BaseBlock baseBlock)
            {
                if (pipe.SourceBlock == block) baseBlock.SetOutputConnected(false);
                if (pipe.TargetBlock == block) baseBlock.SetInputConnected(false);
            }
        }
    }

    public void RemoveConnection(ConnectionPipe pipe)
    {
        if (pipe == null) return;

        Logger.Connection.Print($"Removing connection pipe between {pipe.SourceBlock?.Name} and {pipe.TargetBlock?.Name}");

        // Remove from both blocks' connection lists
        if (pipe.SourceBlock != null)
            if (_blockConnections.TryGetValue(pipe.SourceBlock, out var sourceConnections))
            {
                sourceConnections.Remove(pipe);
                if (sourceConnections.Count == 0) _blockConnections.Remove(pipe.SourceBlock);
            }

        if (pipe.TargetBlock != null)
            if (_blockConnections.TryGetValue(pipe.TargetBlock, out var targetConnections))
            {
                targetConnections.Remove(pipe);
                if (targetConnections.Count == 0) _blockConnections.Remove(pipe.TargetBlock);
            }

        // Remove the pipe from the scene tree
        pipe.QueueFree();
    }

    private void RestoreConnection(IBlock sourceBlock, IBlock targetBlock)
    {
        var sourceName = sourceBlock.Name ?? "unknown";
        var targetName = targetBlock.Name ?? "unknown";
        Logger.Connection.Print($"Restoring connection between {sourceName} and {targetName}");

        var restoredPipe = ConnectionFactory.CreatePipeForInsertion(sourceBlock, targetBlock);
        if (restoredPipe != null)
        {
            var content = GetBlockLayerContent();
            if (content != null)
            {
                content.AddChild(restoredPipe);
                ZIndexConfig.SetZIndex(restoredPipe, ZIndexConfig.Layers.Pipes);
                _activePipes.Add(restoredPipe);
                AddPipeToBlock(sourceBlock, restoredPipe);
                AddPipeToBlock(targetBlock, restoredPipe);
                Logger.Connection.Print("Successfully restored connection in BlockLayerContent");
            }
            else
            {
                Logger.Connection.Err("BlockLayerContent not found, connection failed");
                restoredPipe.QueueFree();
            }
        }
    }

    // Keep the parameterless version for backward compatibility with Input block
    public (IBlock? nextBlock, ConnectionPipe? pipe) GetNextConnection()
    {
        // Get the Input block as our starting point
        var inputBlock = GetNode<BaseBlock>("../BlockLayer/Input");
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

    public void AddPipe(ConnectionPipe pipe)
    {
        _activePipes.Add(pipe);
        _activePipes.Add(pipe);
        var content = GetBlockLayerContent();
        if (content != null)
        {
            content.AddChild(pipe);
            ZIndexConfig.SetZIndex(pipe, ZIndexConfig.Layers.Pipes);
        }
        else
        {
            Logger.Connection.Err("[ConnectionManager] BlockLayerContent not found, pipe added to wrong node");
            AddChild(pipe);
        }
    }

    public void RemovePipe(ConnectionPipe pipe)
    {
        if (_activePipes.Contains(pipe))
        {
            _activePipes.Remove(pipe);
            _activePipes.Remove(pipe);
            pipe.QueueFree();
        }
    }

    public void SetConnection(IBlock block, ConnectionPipe pipe, bool isSource = true)
    {
        AddPipeToBlock(block, pipe);

        // Get the existing source and target blocks
        var currentSource = isSource ? block : pipe.SourceBlock;
        var currentTarget = isSource ? pipe.TargetBlock : block;

        // Re-initialize the pipe with the correct source and target
        if (currentSource != null && currentTarget != null)
        {
            var sourceSocket = currentSource.GetOutputSocket() as Node2D;
            var targetSocket = currentTarget.GetInputSocket() as Node2D;
            if (sourceSocket != null && targetSocket != null)
            {
                var content = GetBlockLayerContent();
                if (content != null && pipe.GetParent() != content)
                {
                    // If pipe is already in the tree but in the wrong place, reparent it
                    if (pipe.IsInsideTree()) pipe.GetParent()?.RemoveChild(pipe);
                    content.AddChild(pipe);
                    ZIndexConfig.SetZIndex(pipe, ZIndexConfig.Layers.Pipes);
                }

                pipe.Initialize(sourceSocket, targetSocket);

                // Set the connection state flags for both blocks
                currentSource.SetOutputConnected(true);
                currentTarget.SetInputConnected(true);

                Logger.Connection.Print($"Source block {currentSource.Name} output connected: {currentSource.HasOutputConnection()}");
                Logger.Connection.Print($"Target block {currentTarget.Name} input connected: {currentTarget.HasInputConnection()}");
            }
        }

        Logger.Connection.Print($"[ConnectionManager] Set {(isSource ? "source" : "target")} connection for block {block.Name} to pipe {pipe.Name}");
    }
}