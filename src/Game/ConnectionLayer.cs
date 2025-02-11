using Godot;
using System.Collections.Generic;
using System.Linq;
using F.Blocks;

namespace F;

public partial class ConnectionLayer : Node2D
{
    private BaseBlock? _inputBlock;
    private BaseBlock? _outputBlock;
    private BaseBlock? _hoveredPipeBlock;
    private ConnectionPipe? _hoveredPipe;
    private List<ConnectionPipe> _connections = new();
    private PackedScene? _connectionScene;

    // Add public properties to access the blocks
    public BaseBlock? InputBlock => _inputBlock;
    public BaseBlock? OutputBlock => _outputBlock;
    
    public override void _Ready()
    {
        ZIndex = AnimConfig.ZIndex.Pipe;  // Set pipe layer z-index

        // Load connection scene
        _connectionScene = GD.Load<PackedScene>("res://scenes/Connection.tscn");
        if (_connectionScene == null)
        {
            GD.PrintErr("Failed to load Connection scene!");
            return;
        }

        // Debug node tree
        GD.Print("ConnectionLayer children:");
        foreach (var child in GetChildren())
        {
            GD.Print($"Child: {child.Name}, Type: {child.GetType()}");
        }

        // Get nodes directly as their correct types
        _inputBlock = GetNode<Node2D>("Input") as BaseBlock;
        _outputBlock = GetNode<Node2D>("Output") as BaseBlock;
        
        if (_inputBlock == null || _outputBlock == null)
        {
            GD.PrintErr($"Failed to get blocks. Input: {GetNode<Node2D>("Input")?.GetType()}, Output: {GetNode<Node2D>("Output")?.GetType()}");
            return;
        }

        GD.Print($"Successfully got blocks. Input: {_inputBlock.GetType()}, Output: {_outputBlock.GetType()}");
        
        // Connect output block signal
        if (_outputBlock is Output output)
        {
            output.TokenProcessed += OnTokenProcessed;
        }

        // Create only the initial input-to-output connection
        CreateInitialConnection();
    }

    private void CreateInitialConnection()
    {
        if (_inputBlock == null || _outputBlock == null) return;

        var fromSocket = _inputBlock.GetNodeOrNull<Node2D>("BlockOutputSocket");
        var toSocket = _outputBlock.GetNodeOrNull<Node2D>("BlockInputSocket");
        
        if (fromSocket != null && toSocket != null)
        {
            CreateConnection(fromSocket, toSocket);
        }
        else
        {
            GD.PrintErr("Could not create initial connection - sockets not found!");
        }
    }

    private ConnectionPipe? CreateConnection(Node2D fromSocket, Node2D toSocket)
    {
        if (_connectionScene == null) return null;

        // Check if connection already exists
        var existingConnection = _connections.FirstOrDefault(pipe => {
            var (pipeFrom, pipeTo) = pipe.GetSockets();
            return (pipeFrom == fromSocket && pipeTo == toSocket) ||
                   (pipeFrom == toSocket && pipeTo == fromSocket);
        });

        if (existingConnection != null)
        {
            GD.Print($"Connection already exists between {fromSocket.GetParent().Name} and {toSocket.GetParent().Name}");
            return null;
        }

        var connection = _connectionScene.Instantiate<ConnectionPipe>();
        AddChild(connection);
        connection.Show();
        connection.ZIndex = 0;
        connection.Initialize(fromSocket, toSocket);
        _connections.Add(connection);
        
        GD.Print($"Created connection between {fromSocket.GetParent().Name} -> {toSocket.GetParent().Name}");
        
        return connection;
    }
    
    private void OnTokenProcessed(float value)
    {
        // Handle processed token value
        GD.Print($"Token processed with final value: {value}");
        
        // Emit signal to GameManager
        EmitSignal(SignalName.TokenProcessed, value);
    }
    
    public void SetInputValue(float value)
    {
        if (_inputBlock is Input input)
        {
            input.SetValue(value);
        }
    }
    
    public void ProcessToken()
    {
        if (_inputBlock == null || _outputBlock == null) return;
        
        if (_inputBlock is Input input && _outputBlock is Output output)
        {
            var value = input.GetValue();
            output.ProcessValue(value);
        }
    }
    
    [Signal]
    public delegate void TokenProcessedEventHandler(float value);

    public void HandleBlockDrag(BaseBlock block, Vector2 position)
    {
        // Find pipe under dragged block
        ConnectionPipe? nearestPipe = null;
        foreach (var pipe in _connections)
        {
            if (pipe.IsPointNearPipe(position))
            {
                nearestPipe = pipe;
                break;
            }
        }

        // Update hover states
        if (_hoveredPipe != nearestPipe)
        {
            _hoveredPipe?.SetHovered(false);
            _hoveredPipe = nearestPipe;
            _hoveredPipe?.SetHovered(true);
        }
    }

    public void HandleBlockDrop(BaseBlock block, Vector2 position)
    {
        if (_hoveredPipe == null) return;

        // Get existing connection
        var (fromSocket, toSocket) = _hoveredPipe.GetSockets();
        
        // Remove existing connection and get all connections with same endpoints
        var duplicateConnections = _connections
            .Where(pipe => {
                var (pipeFrom, pipeTo) = pipe.GetSockets();
                return (pipeFrom == fromSocket && pipeTo == toSocket) ||
                       (pipeFrom == toSocket && pipeTo == fromSocket);
            })
            .ToList();

        // Remove all duplicate connections
        foreach (var pipe in duplicateConnections)
        {
            _connections.Remove(pipe);
            pipe.QueueFree();
        }
        
        // Create two new connections
        var blockInput = block.GetNode<Node2D>("BlockInputSocket");
        var blockOutput = block.GetNode<Node2D>("BlockOutputSocket");
        
        if (blockInput != null && blockOutput != null)
        {
            CreateConnection(fromSocket, blockInput);
            CreateConnection(blockOutput, toSocket);
        }
        
        _hoveredPipe = null;
        
        // Debug connections
        GD.Print($"Total connections after drop: {_connections.Count}");
        foreach (var conn in _connections)
        {
            var (from, to) = conn.GetSockets();
            GD.Print($"Connection: {from.GetParent().Name} -> {to.GetParent().Name}");
        }
    }

    public void HandleBlockRemoved(BaseBlock block)
    {
        // Find ALL connections that involve this block
        var connectedPipes = _connections
            .Where(pipe => {
                var (from, to) = pipe.GetSockets();
                return from.GetParent() == block || to.GetParent() == block;
            })
            .ToList();

        GD.Print($"Found {connectedPipes.Count} connections for block {block.Name}");

        if (connectedPipes.Count >= 2)
        {
            // Find the external connections (ones not connected to the block being removed)
            var externalSockets = connectedPipes
                .SelectMany(pipe => {
                    var (from, to) = pipe.GetSockets();
                    return new[] { 
                        from.GetParent() == block ? null : from,
                        to.GetParent() == block ? null : to
                    };
                })
                .Where(socket => socket != null)
                .ToList();

            // Remove all connected pipes
            foreach (var pipe in connectedPipes)
            {
                _connections.Remove(pipe);
                pipe.QueueFree();
            }

            // If we found exactly two valid external sockets, reconnect them
            if (externalSockets.Count >= 2)
            {
                var fromSocket = externalSockets[0];
                var toSocket = externalSockets[1];
                
                if (fromSocket != null && toSocket != null)
                {
                    var newConnection = CreateConnection(fromSocket, toSocket);
                    if (newConnection != null)
                    {
                        newConnection.StartReconnectAnimation();
                    }
                }
            }
        }
        else
        {
            // Just remove any single connections
            foreach (var pipe in connectedPipes)
            {
                _connections.Remove(pipe);
                pipe.QueueFree();
            }
        }
    }

    public void ProcessTokenThroughBlock(BaseBlock currentBlock, Token token)
    {
        // Find the outgoing connection from this block
        var outgoingPipe = _connections.FirstOrDefault(pipe => {
            var (from, _) = pipe.GetSockets();
            return from.GetParent() == currentBlock;
        });

        if (outgoingPipe != null)
        {
            var (_, toSocket) = outgoingPipe.GetSockets();
            var nextBlock = toSocket.GetParent<BaseBlock>();
            if (nextBlock != null)
            {
                GD.Print($"Moving token from {currentBlock.Name} to {nextBlock.Name}");
                token.MoveTo(nextBlock, toSocket.GlobalPosition);
            }
            else
            {
                GD.PrintErr($"Invalid next block from socket {toSocket.Name}");
                token.QueueFree();
            }
        }
        else
        {
            // No more connections, token is done
            GD.Print($"No more connections from {currentBlock.Name}, destroying token");
            token.QueueFree();
        }
    }

    public void SpawnToken()
    {
        if (_inputBlock == null) return;

        var inventory = GetNode<Inventory>("../Inventory");
        var baseValue = inventory?.TokenBaseValue ?? 1.0f;
        
        var token = new Token { Value = baseValue };
        AddChild(token);
        
        // Position at input block's output socket
        var outputSocket = _inputBlock.GetNode<Node2D>("BlockOutputSocket");
        if (outputSocket != null)
        {
            token.GlobalPosition = outputSocket.GlobalPosition;
            ProcessTokenThroughBlock(_inputBlock, token);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("spawn_token"))  // Define this in project settings
        {
            SpawnToken();
            GetViewport().SetInputAsHandled();
        }
    }
}
