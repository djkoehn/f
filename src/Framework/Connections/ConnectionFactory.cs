using F.Framework.Blocks;
using F.Game.Toolbar;
using F.Framework.Logging;

namespace F.Framework.Connections;

public partial class ConnectionFactory : Node
{
    private readonly Node2D _blockLayer;
    private readonly PackedScene _pipeScene;

    public ConnectionFactory(Node2D blockLayer)
    {
        _pipeScene = GD.Load<PackedScene>("res://src/Framework/Connections/ConnectionPipe.tscn");
        _blockLayer = blockLayer;
    }

    public ConnectionPipe? CreatePipe(IBlock sourceBlock, IBlock targetBlock)
    {
        if (sourceBlock == null || targetBlock == null)
        {
            Logger.Connection.Err("Cannot create pipe with null blocks");
            return null;
        }

        Logger.Connection.Print($"Creating pipe between blocks: {sourceBlock.Name} -> {targetBlock.Name}");
        Logger.Connection.Print(
            $"From block type: {sourceBlock.GetType().Name}, To block type: {targetBlock.GetType().Name}");

        var fromSocket = sourceBlock.GetOutputSocket() as Node2D;
        var toSocket = targetBlock.GetInputSocket() as Node2D;

        if (fromSocket == null || toSocket == null)
        {
            Logger.Connection.Err("One or both sockets are null");
            return null;
        }

        // Enforce that a block can have only one connection per socket.
        if (sourceBlock.HasConnections() || targetBlock.HasConnections())
        {
            Logger.Connection.Err(
                $"One or both blocks already have a connection! ({sourceBlock.Name} or {targetBlock.Name})");
            return null;
        }

        if (sourceBlock.GetParent() is ToolbarBlockContainer || targetBlock.GetParent() is ToolbarBlockContainer)
            return null;

        Logger.Connection.Print(
            $"From socket type: {fromSocket.GetType()}, To socket type: {toSocket.GetType()}");

        var pipe = _pipeScene.Instantiate<ConnectionPipe>();
        if (pipe != null)
        {
            pipe.Name = $"Pipe_{sourceBlock.Name}_{targetBlock.Name}";
            pipe.Initialize(fromSocket, toSocket);
            Logger.Connection.Print(
                $"Initialized pipe {pipe.Name} between {sourceBlock.Name} and {targetBlock.Name}");

            _blockLayer.AddChild(pipe);
            return pipe;
        }

        Logger.Connection.Err("Failed to create pipe");
        return null;
    }

    public ConnectionPipe CreateTemporaryConnection(IBlock from, Vector2 endPoint)
    {
        var pipe = new ConnectionPipe();
        // Remove deferred add since parent will handle it
        pipe.InitializeTemporary(from, endPoint);
        return pipe;
    }

    // New static method to create a pipe for insertion bypassing the connection check
    public static ConnectionPipe? CreatePipeForInsertion(IBlock from, IBlock to)
    {
        var fromName = from?.Name ?? "unknown";
        var toName = to?.Name ?? "unknown";
        Logger.Connection.Print($"Creating pipe between blocks: {fromName} -> {toName}");

        var fromSocket = from?.GetOutputSocket() as Node2D ?? from as Node2D;
        var toSocket = to?.GetInputSocket() as Node2D ?? to as Node2D;

        if (fromSocket == null || toSocket == null)
        {
            Logger.Connection.Err($"Could not find required sockets for blocks {fromName} -> {toName}");
            return null;
        }

        if (from?.GetParent() is ToolbarBlockContainer || to?.GetParent() is ToolbarBlockContainer)
            return null;

        // Print the actual types for debugging
        Logger.Connection.Print($"From block type: {from?.GetType()}, To block type: {to?.GetType()}");
        Logger.Connection.Print(
            $"From socket type: {fromSocket.GetType()}, To socket type: {toSocket.GetType()}");

        var pipe = new ConnectionPipe();
        pipe.Initialize(fromSocket, toSocket);
        return pipe;
    }

    public bool CanConnect(IBlock block, ConnectionPipe pipe)
    {
        if (block == pipe.SourceBlock)
            // Trying to connect to the source, check if block already has an output
            return !block.HasOutputConnection();

        if (block == pipe.TargetBlock)
            // Trying to connect to the target, check if block already has an input
            return !block.HasInputConnection();

        // Not connecting to either end of the pipe
        return false;
    }
}