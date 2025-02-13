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
    private ColorRect? _bounds;
    
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

        // Get the bounds reference - now directly under ConnectionLayer
        _bounds = GetNodeOrNull<ColorRect>("Bounds");
        if (_bounds == null)
        {
            GD.PrintErr("Failed to get Bounds ColorRect from ConnectionLayer!");
        }
        else
        {
            GD.Print("Successfully found Bounds ColorRect");
        }

        CreateInitialConnection();
    }

    private Rect2? GetBoundsRect()
    {
        if (_bounds == null || !IsInstanceValid(_bounds))
        {
            // Try to get bounds directly from ConnectionLayer
            _bounds = GetNodeOrNull<ColorRect>("Bounds");
        }
        
        if (_bounds != null && IsInstanceValid(_bounds))
        {
            return _bounds.GetGlobalRect();
        }
        
        GD.PrintErr("Could not get valid bounds rect!");
        return null;
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

        // Play connection sound
        AudioManager.Instance?.PlayBlockConnect();

        GD.Print($"Connection created. FromBlock: {fromBlock?.Name}, ToBlock: {toBlock?.Name}");
        return connection;
    }

    private Vector2 GetNonCollidingPosition(BaseBlock block, Vector2 position, Rect2? bounds, ConnectionPipe? hoveredPipe)
    {
        if (!bounds.HasValue) return position;

        var blockSize = 128f;
        var padding = 10f;
        var halfSize = blockSize / 2;
        
        // First clamp the position to bounds
        var newPos = new Vector2(
            Mathf.Clamp(position.X, bounds.Value.Position.X + halfSize, bounds.Value.Position.X + bounds.Value.Size.X - halfSize),
            Mathf.Clamp(position.Y, bounds.Value.Position.Y + halfSize, bounds.Value.Position.Y + bounds.Value.Size.Y - halfSize)
        );
        
        // Get all blocks in the layer except the one being placed
        var blocks = GetChildren()
            .OfType<BaseBlock>()
            .Where(b => b != block && b.IsInBlockLayer())
            .ToList();

        // Add blocks connected by the hovered pipe
        if (hoveredPipe != null)
        {
            try
            {
                var (fromSocket, toSocket) = hoveredPipe.GetSockets();
                var fromBlock = fromSocket?.GetParent<BaseBlock>();
                var toBlock = toSocket?.GetParent<BaseBlock>();
                
                if (fromBlock != null && !blocks.Contains(fromBlock))
                    blocks.Add(fromBlock);
                if (toBlock != null && !blocks.Contains(toBlock))
                    blocks.Add(toBlock);
            }
            catch (InvalidOperationException)
            {
                // Ignore invalid pipe sockets
            }
        }

        bool hasCollision;
        int maxIterations = 10;
        int currentIteration = 0;

        do
        {
            hasCollision = false;
            var currentRect = new Rect2(
                newPos - new Vector2(blockSize/2, blockSize/2),
                new Vector2(blockSize, blockSize)
            );

            foreach (var otherBlock in blocks)
            {
                var otherRect = new Rect2(
                    otherBlock.GlobalPosition - new Vector2(blockSize/2, blockSize/2),
                    new Vector2(blockSize, blockSize)
                );
                var paddedRect = new Rect2(
                    otherRect.Position - new Vector2(padding, padding),
                    otherRect.Size + new Vector2(padding * 2, padding * 2)
                );

                if (currentRect.Intersects(paddedRect))
                {
                    hasCollision = true;
                    var blockCenter = currentRect.GetCenter();
                    var otherCenter = paddedRect.GetCenter();
                    var direction = blockCenter - otherCenter;
                    
                    // Try horizontal shift first
                    if (Mathf.Abs(direction.X) >= Mathf.Abs(direction.Y))
                    {
                        float shiftX = direction.X > 0 ? 
                            paddedRect.End.X - currentRect.Position.X :
                            paddedRect.Position.X - currentRect.End.X;
                            
                        // Check if horizontal shift would keep us in bounds
                        float newX = newPos.X + shiftX;
                        if (newX >= bounds.Value.Position.X + halfSize && 
                            newX <= bounds.Value.Position.X + bounds.Value.Size.X - halfSize)
                        {
                            newPos.X = newX;
                            continue;
                        }
                    }
                    
                    // Try vertical shift if horizontal isn't possible or would put us out of bounds
                    float shiftY = direction.Y > 0 ? 
                        paddedRect.End.Y - currentRect.Position.Y :
                        paddedRect.Position.Y - currentRect.End.Y;
                        
                    float newY = newPos.Y + shiftY;
                    if (newY >= bounds.Value.Position.Y + halfSize && 
                        newY <= bounds.Value.Position.Y + bounds.Value.Size.Y - halfSize)
                    {
                        newPos.Y = newY;
                    }
                    else
                    {
                        // If we can't shift either direction within bounds, 
                        // just keep the clamped position and break
                        hasCollision = false;
                    }
                    break;
                }
            }

            currentIteration++;
        } while (hasCollision && currentIteration < maxIterations);

        return newPos;
    }

    private List<BlockMovement> CalculateBlockSpread(BaseBlock newBlock, Vector2 targetPosition, List<BaseBlock> existingBlocks, Rect2? bounds)
    {
        if (!bounds.HasValue) return new List<BlockMovement>();

        float padding = 10f;
        var blockSize = 128f;
        var movements = existingBlocks.Select(b => new BlockMovement(b, b.GlobalPosition)).ToList();
        var newBlockRect = new Rect2(
            targetPosition - new Vector2(blockSize/2, blockSize/2),
            new Vector2(blockSize, blockSize)
        );

        bool needsAnotherPass;
        int maxPasses = 20;
        int currentPass = 0;

        // First, check if any blocks need to move at all
        bool anyCollisions = false;
        foreach (var movement in movements)
        {
            var currentRect = new Rect2(
                movement.NewPosition - new Vector2(blockSize/2, blockSize/2),
                new Vector2(blockSize, blockSize)
            );
            
            var paddedCurrentRect = new Rect2(
                currentRect.Position - new Vector2(padding, padding),
                currentRect.Size + new Vector2(padding * 2, padding * 2)
            );
            
            // Check against new block
            if (paddedCurrentRect.Intersects(newBlockRect))
            {
                anyCollisions = true;
                break;
            }

            // Check against other blocks
            foreach (var otherMovement in movements)
            {
                if (otherMovement.Block == movement.Block) continue;

                var otherRect = new Rect2(
                    otherMovement.NewPosition - new Vector2(blockSize/2, blockSize/2),
                    new Vector2(blockSize, blockSize)
                );
                
                var paddedOtherRect = new Rect2(
                    otherRect.Position - new Vector2(padding, padding),
                    otherRect.Size + new Vector2(padding * 2, padding * 2)
                );

                if (paddedCurrentRect.Intersects(paddedOtherRect))
                {
                    anyCollisions = true;
                    break;
                }
            }
            
            if (anyCollisions) break;
        }

        // If no collisions, return movements without changes
        if (!anyCollisions) return movements;

        do
        {
            needsAnotherPass = false;
            currentPass++;

            foreach (var movement in movements)
            {
                var currentRect = new Rect2(
                    movement.NewPosition - new Vector2(blockSize/2, blockSize/2),
                    new Vector2(blockSize, blockSize)
                );

                var totalMoveDirection = Vector2.Zero;
                var needsMove = false;
                
                var paddedCurrentRect = new Rect2(
                    currentRect.Position - new Vector2(padding, padding),
                    currentRect.Size + new Vector2(padding * 2, padding * 2)
                );
                
                // Check against new block
                if (paddedCurrentRect.Intersects(newBlockRect))
                {
                    needsMove = true;
                    var moveDir = (currentRect.GetCenter() - newBlockRect.GetCenter()).Normalized();
                    totalMoveDirection += moveDir;
                }

                // Check against other blocks
                foreach (var otherMovement in movements)
                {
                    if (otherMovement.Block == movement.Block) continue;

                    var otherRect = new Rect2(
                        otherMovement.NewPosition - new Vector2(blockSize/2, blockSize/2),
                        new Vector2(blockSize, blockSize)
                    );
                    
                    var paddedOtherRect = new Rect2(
                        otherRect.Position - new Vector2(padding, padding),
                        otherRect.Size + new Vector2(padding * 2, padding * 2)
                    );

                    if (paddedCurrentRect.Intersects(paddedOtherRect))
                    {
                        needsMove = true;
                        var moveDir = (currentRect.GetCenter() - otherRect.GetCenter()).Normalized();
                        totalMoveDirection += moveDir;
                    }
                }

                if (needsMove && totalMoveDirection != Vector2.Zero)
                {
                    totalMoveDirection = totalMoveDirection.Normalized();
                    var shiftAmount = (blockSize + padding) * 0.6f; // Adjusted shift amount for smoother movement
                    var proposedNewPos = movement.NewPosition + totalMoveDirection * shiftAmount;

                    // Clamp to bounds
                    var halfSize = blockSize / 2;
                    var clampedPos = new Vector2(
                        Mathf.Clamp(proposedNewPos.X, bounds.Value.Position.X + halfSize, bounds.Value.Position.X + bounds.Value.Size.X - halfSize),
                        Mathf.Clamp(proposedNewPos.Y, bounds.Value.Position.Y + halfSize, bounds.Value.Position.Y + bounds.Value.Size.Y - halfSize)
                    );

                    if (clampedPos != movement.NewPosition)
                    {
                        movement.NewPosition = clampedPos;
                        movement.HasMoved = true;
                        needsAnotherPass = true;
                    }
                }
            }
        } while (needsAnotherPass && currentPass < maxPasses);

        return movements;
    }

    private bool WouldCollideWithBlocks(BaseBlock block, Vector2 position, List<BaseBlock> blocks, float padding)
    {
        var blockSize = 128f;
        var blockRect = new Rect2(
            position - new Vector2(blockSize/2, blockSize/2),
            new Vector2(blockSize, blockSize)
        );

        foreach (var otherBlock in blocks)
        {
            var otherRect = new Rect2(
                otherBlock.GlobalPosition - new Vector2(blockSize/2, blockSize/2),
                new Vector2(blockSize, blockSize)
            );
            var paddedRect = new Rect2(
                otherRect.Position - new Vector2(padding, padding),
                otherRect.Size + new Vector2(padding * 2, padding * 2)
            );

            if (blockRect.Intersects(paddedRect))
            {
                return true;
            }
        }

        return false;
    }

    private ConnectionPipe? FindNearestPipe(Vector2 position)
    {
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
        
        return nearestPipe;
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
        // If block is being sent back to toolbar, handle it separately from normal placement
        if (position.Y > 1000)
        {
            if (_hoveredPipe != null)
            {
                _hoveredPipe.SetHovered(false);
                _hoveredPipe = null;
            }
            
            // Remove all connections before returning to toolbar
            RemoveBlockConnections(block);
            
            // Get the toolbar and handle the return
            var toolbar = GetNode<Toolbar>("../Toolbar");
            if (toolbar != null)
            {
                // Remove from connection layer immediately to prevent any further processing
                RemoveChild(block);
                toolbar.ReturnBlockToToolbar(block);
            }
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

        // First clamp the position to bounds and store original position
        var boundsRect = GetBoundsRect();
        var blockSize = 128f;
        var halfSize = blockSize / 2;
        var originalPos = position; // Store the original position for animation
        var newPos = position;
        
        if (boundsRect.HasValue)
        {
            newPos = new Vector2(
                Mathf.Clamp(position.X, boundsRect.Value.Position.X + halfSize, boundsRect.Value.Position.X + boundsRect.Value.Size.X - halfSize),
                Mathf.Clamp(position.Y, boundsRect.Value.Position.Y + halfSize, boundsRect.Value.Position.Y + boundsRect.Value.Size.Y - halfSize)
            );
        }

        // Set the block's position to the original position first
        block.GlobalPosition = originalPos;

        // Get all existing blocks including the newly placed one
        var existingBlocks = GetChildren()
            .Cast<Node>()
            .Where(n => n is BaseBlock)
            .Cast<BaseBlock>()
            .Where(b => b.IsInBlockLayer() && b != block) // Exclude the block being placed
            .ToList();

        // Find the nearest pipe at the original position
        var nearestPipe = FindNearestPipe(newPos);
        var finalPos = newPos;
        
        if (nearestPipe != null)
        {
            // Check if we're close enough to snap to the pipe
            var distanceToNearestPipe = newPos.DistanceTo(nearestPipe.GlobalPosition);
            if (distanceToNearestPipe < blockSize / 2)
            {
                finalPos = nearestPipe.GlobalPosition;
            }
        }

        // Calculate all block movements needed for proper spacing
        var blockMovements = CalculateBlockSpread(block, finalPos, existingBlocks, boundsRect);

        // Animate from original position to final position
        var snapTween = CreateTween();
        snapTween.SetTrans(Tween.TransitionType.Spring);
        snapTween.SetEase(Tween.EaseType.Out);
        snapTween.TweenProperty(block, "global_position", finalPos, 0.5f);

        // Animate all other blocks to their new positions
        foreach (var movement in blockMovements)
        {
            if (movement.HasMoved) // Skip blocks that haven't moved
            {
                var tween = CreateTween();
                tween.SetTrans(Tween.TransitionType.Spring);
                tween.SetEase(Tween.EaseType.Out);
                tween.TweenProperty(movement.Block, "global_position", movement.NewPosition, 0.5f);
            }
        }

        // Check for existing connections on the block being placed
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

        if (hasConnections) return;

        // Try to connect to the nearest pipe if we found one
        if (nearestPipe == null) return;

        try
        {
            // Get the blocks we're connecting between
            var (fromSocket, toSocket) = nearestPipe.GetSockets();
            var fromBlock = fromSocket?.GetParent<BaseBlock>();
            var toBlock = toSocket?.GetParent<BaseBlock>();
            
            if (fromBlock == null || toBlock == null) return;

            // Remove the existing connection
            _connections.Remove(nearestPipe);
            nearestPipe.QueueFree();

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
        Node2D? fromSocket = null;
        Node2D? toSocket = null;

        // Find the block's connections and store its from/to sockets
        var connectedPipes = _connections.Where(pipe => {
            try
            {
                var (from, to) = pipe.GetSockets();
                var fromParent = from?.GetParent<BaseBlock>();
                var toParent = to?.GetParent<BaseBlock>();
                
                if (fromParent == block)
                {
                    toSocket = to; // Store the destination socket
                }
                else if (toParent == block)
                {
                    fromSocket = from; // Store the source socket
                }
                
                return fromParent == block || toParent == block;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }).ToList();

        // Remove only this block's pipes
        foreach (var pipe in connectedPipes)
        {
            _connections.Remove(pipe);
            pipe.QueueFree();
        }

        // If we have both a from and to socket that aren't on the block being removed,
        // create a new connection between them
        if (fromSocket != null && toSocket != null)
        {
            var fromBlock = fromSocket.GetParent<BaseBlock>();
            var toBlock = toSocket.GetParent<BaseBlock>();
            
            // Only connect if neither socket belongs to the block being removed
            if (fromBlock != block && toBlock != block)
            {
                CreateConnection(fromSocket, toSocket);
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

    private class BlockMovement
    {
        public BaseBlock Block { get; set; }
        public Vector2 NewPosition { get; set; }
        public bool HasMoved { get; set; }

        public BlockMovement(BaseBlock block, Vector2 position)
        {
            Block = block;
            NewPosition = position;
            HasMoved = false;
        }
    }

    private void AnimateBlockMovements(List<BlockMovement> movements)
    {
        foreach (var movement in movements)
        {
            if (!movement.HasMoved) continue;

            var snapTween = CreateTween();
            snapTween.SetTrans(Tween.TransitionType.Spring);
            snapTween.SetEase(Tween.EaseType.Out);
            snapTween.TweenProperty(movement.Block, "global_position", movement.NewPosition, 0.5f);
        }
    }
}
