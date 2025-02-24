using Godot;
using F.Framework.Blocks;
using F.Framework.Core;
using F.Game.Connections;
using F.Game.Core;
using F.UI.Animations;
using F.UI.Animations.UI;
using F.Framework.Core.SceneTree;

namespace F.Framework.Connections;

public partial class ConnectionManager : Node2D, IConnectionManager
{
    private readonly Dictionary<IBlock, List<ConnectionPipe>> _blockConnections = new();
    private readonly ConnectionFactory _factory;
    private readonly ConnectionValidator _validator;
    private ConnectionPipe? _hoveredPipe;
    private IBlock? _inputBlock;
    private IBlock? _outputBlock;
    private ConnectionPipe? _temporaryConnection;
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

    private Node2D? GetBlockLayerContent()
    {
        return GetNode<Node2D>("../BlockLayer");
    }

    public override void _Ready()
    {
        GD.Print("[ConnectionManager Debug] _Ready called.");
        base._Ready();
        ZIndex = ZIndexConfig.Layers.Pipes;

        // Retrieve the input and output blocks from the viewport content
        _inputBlock = GetNode<BaseBlock>("../BlockLayer/Input");
        _outputBlock = GetNode<BaseBlock>("../BlockLayer/Output");

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

        GD.Print($"Creating initial connection between {_inputBlock.Name} and {_outputBlock.Name} blocks...");
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
                GD.Print($"[ConnectionManager] Successfully created initial connection in BlockLayerContent");
            }
            else
            {
                GD.PrintErr("[ConnectionManager] BlockLayerContent not found, initial connection failed");
                pipe.QueueFree();
            }
        }
    }

    private void AddPipeToBlock(IBlock block, ConnectionPipe pipe)
    {
        if (!_blockConnections.ContainsKey(block))
        {
            _blockConnections[block] = new List<ConnectionPipe>();
        }
        _blockConnections[block].Add(pipe);

        if (block is BaseBlock baseBlock)
        {
            if (pipe.SourceBlock == block) baseBlock.SetOutputConnected(true);
            if (pipe.TargetBlock == block) baseBlock.SetInputConnected(true);
            GD.Print($"[ConnectionManager] Set connection for block {block.Name} to pipe {pipe.Name}");
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

        GD.Print($"[ConnectionManager] Removing connection pipe between {pipe.SourceBlock?.Name} and {pipe.TargetBlock?.Name}");

        // Remove from both blocks' connection lists
        if (pipe.SourceBlock != null)
        {
            if (_blockConnections.TryGetValue(pipe.SourceBlock, out var sourceConnections))
            {
                sourceConnections.Remove(pipe);
                if (sourceConnections.Count == 0)
                {
                    _blockConnections.Remove(pipe.SourceBlock);
                }
            }
        }

        if (pipe.TargetBlock != null)
        {
            if (_blockConnections.TryGetValue(pipe.TargetBlock, out var targetConnections))
            {
                targetConnections.Remove(pipe);
                if (targetConnections.Count == 0)
                {
                    _blockConnections.Remove(pipe.TargetBlock);
                }
            }
        }

        // Remove the pipe from the scene tree
        pipe.QueueFree();
    }

    public void DisconnectBlock(IBlock block)
    {
        if (block == null) return;

        GD.Print($"[ConnectionManager Debug] Disconnecting block {block.Name}");

        // Get all connections for this block
        if (_blockConnections.TryGetValue(block, out var connections))
        {
            // Create a new list to avoid modification during enumeration
            var connectionsToRemove = connections.ToList();
            foreach (var pipe in connectionsToRemove)
            {
                RemoveConnection(pipe);
            }
        }

        // Clear the block's connection state
        if (block is BaseBlock baseBlock)
        {
            baseBlock.ResetConnections();
        }

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
        string blockName = block.Name ?? "unknown";
        string sourceName = pipe.SourceBlock.Name ?? "unknown";
        string targetName = pipe.TargetBlock.Name ?? "unknown";

        GD.Print($"[ConnectionManager Debug] Attempting to connect block {blockName} between {sourceName} and {targetName}");

        // Prevent self-connections
        if (block == pipe.SourceBlock || block == pipe.TargetBlock)
        {
            GD.Print($"[ConnectionManager Debug] Rejected self-connection attempt for block {blockName}");
            return false;
        }

        // Special handling for Input block connections
        bool isSourceInput = pipe.SourceBlock.Metadata?.Id == "input";

        // If the block we're inserting has connections and we're not inserting after Input, reject
        if (block is BaseBlock baseBlock && baseBlock.HasConnections() && !isSourceInput)
        {
            GD.Print($"[ConnectionManager Debug] Block {blockName} already has connections and not inserting after Input");
            return false;
        }

        // First ensure we disconnect any existing connections for the block
        DisconnectBlock(block);
        ClearAllHighlights();

        // Store the original blocks
        var sourceBlock = pipe.SourceBlock;
        var targetBlock = pipe.TargetBlock;

        // Reset the new block's connections if it's a BaseBlock
        if (block is BaseBlock newBlock)
        {
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

        // Get the BlockLayerContent node
        var content = GetBlockLayerContent();
        if (content == null)
        {
            GD.PrintErr("[ConnectionManager Debug] BlockLayerContent not found");
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
            GD.Print($"[ConnectionManager] Successfully connected block {blockName} between {sourceName} and {targetName}");
        }

        return true;
    }

    private void RestoreConnection(IBlock sourceBlock, IBlock targetBlock)
    {
        string sourceName = sourceBlock.Name ?? "unknown";
        string targetName = targetBlock.Name ?? "unknown";
        GD.Print($"[ConnectionManager] Restoring connection between {sourceName} and {targetName}");

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
                GD.Print($"[ConnectionManager] Successfully restored connection in BlockLayerContent");
            }
            else
            {
                GD.PrintErr("[ConnectionManager] BlockLayerContent not found, connection failed");
                restoredPipe.QueueFree();
            }
        }
    }

    public override void _Process(double delta)
    {
        if (!_processLogged)
        {
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

    public void ConnectBlocks(IBlock sourceBlock, IBlock targetBlock)
    {
        if (sourceBlock == null || targetBlock == null)
        {
            GD.PrintErr("[ConnectionManager] Cannot connect null blocks");
            return;
        }

        GD.Print($"[ConnectionManager Debug] Creating new connection between {sourceBlock.Name} -> {targetBlock.Name}");

        // First disconnect any existing connections
        if (sourceBlock is BaseBlock sourceBlockInstance)
        {
            sourceBlockInstance.ResetConnections();
        }
        if (targetBlock is BaseBlock targetBlockInstance)
        {
            targetBlockInstance.ResetConnections();
        }

        // Create the connection pipe
        var pipe = _factory.CreatePipe(sourceBlock, targetBlock);
        if (pipe != null)
        {
            // Add to our connection tracking
            if (!_blockConnections.ContainsKey(sourceBlock))
            {
                _blockConnections[sourceBlock] = new List<ConnectionPipe>();
            }
            if (!_blockConnections.ContainsKey(targetBlock))
            {
                _blockConnections[targetBlock] = new List<ConnectionPipe>();
            }

            _blockConnections[sourceBlock].Add(pipe);
            _blockConnections[targetBlock].Add(pipe);

            // Update block states
            if (sourceBlock is BaseBlock sourceBlockInst)
            {
                sourceBlockInst.SetOutputConnected(true);
            }
            if (targetBlock is BaseBlock targetBlockInst)
            {
                targetBlockInst.SetInputConnected(true);
            }

            GD.Print($"[ConnectionManager] Successfully connected blocks {sourceBlock.Name} -> {targetBlock.Name}");
        }
    }

    public bool IsBlockConnected(IBlock block) =>
        _blockConnections.ContainsKey(block) && _blockConnections[block].Count > 0;

    public List<ConnectionPipe> GetCurrentConnections(IBlock block)
    {
        if (_blockConnections.TryGetValue(block, out var connections))
        {
            return new List<ConnectionPipe>(connections);
        }
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
        GD.Print($"[ConnectionManager Debug] Found next block in chain from {currentBlock.Name}: {nextBlock?.Name}");
        return (nextBlock, pipe);
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

    public void ClearAllHighlights()
    {
        foreach (var pipe in _activePipes)
        {
            pipe.SetHighlighted(false);
            pipe.ClearInsertionHighlight();
        }
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
            GD.PrintErr("[ConnectionManager] BlockLayerContent not found, pipe added to wrong node");
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
                    if (pipe.IsInsideTree())
                    {
                        pipe.GetParent()?.RemoveChild(pipe);
                    }
                    content.AddChild(pipe);
                    ZIndexConfig.SetZIndex(pipe, ZIndexConfig.Layers.Pipes);
                }

                pipe.Initialize(sourceSocket, targetSocket);

                // Set the connection state flags for both blocks
                currentSource.SetOutputConnected(true);
                currentTarget.SetInputConnected(true);

                GD.Print($"[ConnectionManager] Source block {currentSource.Name} output connected: {currentSource.HasOutputConnection()}");
                GD.Print($"[ConnectionManager] Target block {currentTarget.Name} input connected: {currentTarget.HasInputConnection()}");
            }
        }

        GD.Print($"[ConnectionManager] Set {(isSource ? "source" : "target")} connection for block {block.Name} to pipe {pipe.Name}");
    }
}