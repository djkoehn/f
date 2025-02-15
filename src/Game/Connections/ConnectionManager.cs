using Godot;
using F.Game.BlockLogic;
using F.Game.Tokens;
using F.Audio;
using F.Config.Visual;
using F.UI.Animations;
using F.Game.Toolbar;
using F.Game.Blocks;
using System.Linq;

namespace F.Game.Connections;

public partial class ConnectionManager : Node2D
{
    private readonly Dictionary<IBlock, ConnectionPipe> _blockToPipeMap = new();
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
            _blockToPipeMap[_inputBlock] = pipe;
            _blockToPipeMap[_outputBlock] = pipe;
            GD.Print("Initial connection created successfully!");
        }
    }

    public void RemoveConnection(IBlock block)
    {
        if (_blockToPipeMap.TryGetValue(block, out var pipe))
        {
            pipe.QueueFree();
            _blockToPipeMap.Remove(block);
        }
    }

    public void ClearConnections()
    {
        var connectedBlocks = new List<IBlock>();
        foreach (var (block, pipe) in _blockToPipeMap)
        {
            pipe.QueueFree();
            connectedBlocks.Add(block);
        }

        foreach (var block in connectedBlocks)
        {
            _blockToPipeMap.Remove(block);
        }
    }

    public ConnectionPipe? GetPipeAtPosition(Vector2 position)
    {
        float hoverDistance = F.Config.Connection.PipeConfig.Interaction.HoverDistance;

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
        foreach (var pipe in _connections)
        {
            if (pipe.IsPointNearPipe(position))
            {
                GD.Print($"[ConnectionManager Debug] Found connection pipe - Source: {pipe.SourceBlock?.GetType()}, Target: {pipe.TargetBlock?.GetType()}");
                return pipe;
            }
        }

        GD.Print("[ConnectionManager Debug] No pipe found at position");
        return null;
    }

    public void ClearAllHighlights()
    {
        foreach (var pipe in _connections) pipe.SetHighlighted(false);
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

        return F.Game.Connections.Helpers.PipeRewiringHelper.InsertBlockIntoPipe(block, pipe, _factory, this);
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

    public void DisconnectBlock(IBlock block)
    {
        if (_blockToPipeMap.TryGetValue(block, out var pipe))
        {
            var otherBlock = pipe.GetOtherBlock(block);
            if (otherBlock != null)
            {
                _blockToPipeMap.Remove(otherBlock);
            }

            pipe.QueueFree();
            _blockToPipeMap.Remove(block);
        }
    }

    public bool IsBlockConnected(IBlock block)
    {
        return _blockToPipeMap.ContainsKey(block);
    }

    public List<ConnectionPipe> GetCurrentConnections(IBlock block)
    {
        var connections = new List<ConnectionPipe>();
        if (_blockToPipeMap.TryGetValue(block, out var pipe))
        {
            connections.Add(pipe);
        }
        return connections;
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
        var pipe = firstEntry.Value;
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
        foreach (var pipe in _activePipes)
        {
            pipe.ClearInsertionHighlight();
        }
    }

    public void SetConnection(IBlock block, ConnectionPipe pipe)
    {
        _blockToPipeMap[block] = pipe;
        GD.Print($"[ConnectionManager] Set connection for block {block.Name} to pipe {pipe.Name}");
    }
}