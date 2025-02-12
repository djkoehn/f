using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace F;

public partial class ConnectionLayer : Node2D
{
    private BaseBlock? _inputBlock;
    private BaseBlock? _outputBlock;
    private ConnectionPipe? _hoveredPipe;
    private List<ConnectionPipe> _connections = new();
    private PackedScene? _connectionScene;
    private Dictionary<BaseBlock, ConnectionPipe> _blockToPipeMap = new();
    
    public override void _Ready()
    {
        ZIndex = 1; // Ensure pipes are drawn above blocks
        GD.Print("Loading Connection scene...");
        _connectionScene = GD.Load<PackedScene>("res://scenes/Connection.tscn");
        if (_connectionScene == null)
        {
            GD.PrintErr("Failed to load Connection scene!");
            return;
        }
        GD.Print("Connection scene loaded successfully");

        _inputBlock = GetNode<Node2D>("Input") as BaseBlock;
        _outputBlock = GetNode<Node2D>("Output") as BaseBlock;
        
        if (_inputBlock == null || _outputBlock == null)
        {
            GD.PrintErr("Failed to get Input/Output blocks!");
            return;
        }

        CreateInitialConnection();
    }

    private void CreateInitialConnection()
    {
        if (_inputBlock == null || _outputBlock == null) return;

        GD.Print("Creating initial connection...");
        var fromSocket = _inputBlock.GetNodeOrNull<Node2D>("BlockOutputSocket");
        var toSocket = _outputBlock.GetNodeOrNull<Node2D>("BlockInputSocket");
        
        if (fromSocket != null && toSocket != null)
        {
            var connection = CreateConnection(fromSocket, toSocket);
            if (connection != null)
            {
                GD.Print("Initial connection created successfully");
            }
            else
            {
                GD.PrintErr("Failed to create initial connection");
            }
        }
        else
        {
            GD.PrintErr($"Failed to get sockets. FromSocket: {fromSocket != null}, ToSocket: {toSocket != null}");
        }
    }

    private ConnectionPipe? CreateConnection(Node2D fromSocket, Node2D toSocket)
    {
        if (_connectionScene == null)
        {
            GD.PrintErr("Cannot create connection: Connection scene is null");
            return null;
        }

        // Check if connection already exists
        var existingConnection = _connections.FirstOrDefault(pipe => {
            try
            {
                var (pipeFrom, pipeTo) = pipe.GetSockets();
                return (pipeFrom == fromSocket && pipeTo == toSocket) ||
                       (pipeFrom == toSocket && pipeTo == fromSocket);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        });

        if (existingConnection != null)
        {
            GD.Print("Connection already exists between these sockets");
            return null;
        }

        GD.Print("Creating new connection...");
        var connection = _connectionScene.Instantiate<ConnectionPipe>();
        AddChild(connection);
        connection.Initialize(fromSocket, toSocket);
        _connections.Add(connection);

        var fromBlock = fromSocket.GetParent<BaseBlock>();
        var toBlock = toSocket.GetParent<BaseBlock>();

        if (fromBlock != null) 
        {
            _blockToPipeMap[fromBlock] = connection;
        }

        GD.Print($"Connection created. FromBlock: {fromBlock?.Name}, ToBlock: {toBlock?.Name}");
        return connection;
    }

    public void HandleBlockDrag(BaseBlock block, Vector2 position)
    {
        // Only look for pipes if the block isn't already connected
        var hasConnections = _connections.Any(pipe => {
            try
            {
                var (from, to) = pipe.GetSockets();
                var fromParent = from.GetParent<BaseBlock>();
                var toParent = to.GetParent<BaseBlock>();
                return fromParent == block || toParent == block;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        });

        // If block is connected, don't look for new connections
        if (hasConnections)
        {
            if (_hoveredPipe != null)
            {
                _hoveredPipe.SetHovered(false);
                _hoveredPipe = null;
            }
            return;
        }

        // Find pipe under dragged block
        ConnectionPipe? nearestPipe = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var pipe in _connections)
        {
            if (pipe.IsPointNearPipe(position))
            {
                var dist = position.DistanceSquaredTo(pipe.GlobalPosition);
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    nearestPipe = pipe;
                }
            }
        }

        // Update hover states
        if (_hoveredPipe != nearestPipe)
        {
            if (_hoveredPipe != null)
            {
                _hoveredPipe.SetHovered(false);
            }
            _hoveredPipe = nearestPipe;
            if (_hoveredPipe != null)
            {
                _hoveredPipe.SetHovered(true);
            }
        }
    }

    public void HandleBlockDrop(BaseBlock block, Vector2 position)
    {
        // If block is being sent back to toolbar, just remove connections and clear hover
        if (position.Y > 1000)
        {
            if (_hoveredPipe != null)
            {
                _hoveredPipe.SetHovered(false);
                _hoveredPipe = null;
            }
            RemoveBlockConnections(block);
            return;
        }

        // Store currently hovered pipe before clearing state
        var hoveredPipe = _hoveredPipe;

        // Clear hover state
        if (_hoveredPipe != null)
        {
            _hoveredPipe.SetHovered(false);
            _hoveredPipe = null;
        }

        // If block is already connected or we're not hovering over a pipe, do nothing
        var hasConnections = _connections.Any(pipe => {
            try
            {
                var (from, to) = pipe.GetSockets();
                var fromParent = from.GetParent<BaseBlock>();
                var toParent = to.GetParent<BaseBlock>();
                return fromParent == block || toParent == block;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        });

        if (hasConnections || hoveredPipe == null) return;

        try
        {
            // Get the blocks we're connecting between
            var (fromSocket, toSocket) = hoveredPipe.GetSockets();
            var fromBlock = fromSocket.GetParent<BaseBlock>();
            var toBlock = toSocket.GetParent<BaseBlock>();
            
            if (fromBlock == null || toBlock == null) return;

            // Remove the existing connection
            _connections.Remove(hoveredPipe);
            hoveredPipe.QueueFree();

            // Create new connections
            var blockInput = block.GetNode<Node2D>("BlockInputSocket");
            var blockOutput = block.GetNode<Node2D>("BlockOutputSocket");
            
            if (blockInput != null && blockOutput != null)
            {
                // Connect from previous output to this block's input
                CreateConnection(fromSocket, blockInput);
                // Connect from this block's output to previous input
                CreateConnection(blockOutput, toSocket);
            }
        }
        catch (InvalidOperationException)
        {
            // Handle case where GetSockets fails
            GD.PrintErr("Failed to get sockets for connection");
        }
    }

    public void RemoveBlockConnections(BaseBlock block)
    {
        // Find all connections involving this block
        var connectionsToRemove = _connections
            .Where(pipe => {
                try
                {
                    var (from, to) = pipe.GetSockets();
                    var fromParent = from.GetParent<BaseBlock>();
                    var toParent = to.GetParent<BaseBlock>();
                    return fromParent == block || toParent == block;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            })
            .ToList();

        if (connectionsToRemove.Count == 2)
        {
            // Find the external sockets (the ones not on the block being removed)
            Node2D? externalFromSocket = null;
            Node2D? externalToSocket = null;

            foreach (var pipe in connectionsToRemove)
            {
                try
                {
                    var (from, to) = pipe.GetSockets();
                    var fromParent = from.GetParent<BaseBlock>();
                    var toParent = to.GetParent<BaseBlock>();

                    if (fromParent != block)
                    {
                        externalFromSocket = from;
                    }
                    else if (toParent != block)
                    {
                        externalToSocket = to;
                    }
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
            }

            // Remove the old connections
            foreach (var connection in connectionsToRemove)
            {
                _connections.Remove(connection);
                connection.QueueFree();
            }

            // Create new connection between the external sockets
            if (externalFromSocket != null && externalToSocket != null)
            {
                CreateConnection(externalFromSocket, externalToSocket);
            }
        }
        else
        {
            // Just remove the connections if there aren't exactly 2
            foreach (var connection in connectionsToRemove)
            {
                _connections.Remove(connection);
                connection.QueueFree();
            }
        }
    }

    public (BaseBlock? block, ConnectionPipe? pipe) GetNextBlockAndPipe(BaseBlock block)
    {
        if (_blockToPipeMap.TryGetValue(block, out var pipe))
        {
            try
            {
                var (_, toSocket) = pipe.GetSockets();
                var nextBlock = toSocket.GetParent<BaseBlock>();
                return (nextBlock, pipe);
            }
            catch (InvalidOperationException)
            {
                return (null, null);
            }
        }

        return (null, null);
    }
}
