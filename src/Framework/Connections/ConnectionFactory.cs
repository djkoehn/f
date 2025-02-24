using Godot;
using F.Framework.Blocks;
using F.Game.Toolbar;

namespace F.Framework.Connections;

public partial class ConnectionFactory : Node
{
    private readonly PackedScene _pipeScene;
    private readonly Node2D _blockLayer;

    public ConnectionFactory(Node2D blockLayer)
    {
        _pipeScene = GD.Load<PackedScene>("res://src/Framework/Connections/ConnectionPipe.tscn");
        _blockLayer = blockLayer;
    }

    public ConnectionPipe? CreatePipe(IBlock sourceBlock, IBlock targetBlock)
    {
        if (sourceBlock == null || targetBlock == null)
        {
            GD.PrintErr("[ConnectionFactory] Cannot create pipe with null blocks");
            return null;
        }

        GD.Print($"[ConnectionFactory] Creating pipe between blocks: {sourceBlock.Name} -> {targetBlock.Name}");
        GD.Print($"[ConnectionFactory] From block type: {sourceBlock.GetType()}, To block type: {targetBlock.GetType()}");

        var sourceSocket = sourceBlock.GetOutputSocket() as Node2D;
        var targetSocket = targetBlock.GetInputSocket() as Node2D;

        if (sourceSocket == null || targetSocket == null)
        {
            GD.PrintErr("[ConnectionFactory] One or both sockets are null");
            return null;
        }

        // Enforce that a block can have only one connection per socket.
        if (sourceBlock.HasConnections() || targetBlock.HasConnections())
        {
            GD.PrintErr($"[ConnectionFactory] One or both blocks already have a connection! ({sourceBlock.Name} or {targetBlock.Name})");
            return null;
        }

        if (sourceBlock.GetParent() is ToolbarBlockContainer || targetBlock.GetParent() is ToolbarBlockContainer)
            return null;

        GD.Print($"[ConnectionFactory] From socket type: {sourceSocket.GetType()}, To socket type: {targetSocket.GetType()}");

        var pipe = _pipeScene.Instantiate<ConnectionPipe>();
        if (pipe != null)
        {
            pipe.Name = $"Pipe_{sourceBlock.Name}_{targetBlock.Name}";
            pipe.Initialize(sourceSocket, targetSocket);
            GD.Print($"[ConnectionPipe] Initialized pipe {pipe.Name} between {sourceBlock.Name} and {targetBlock.Name}");

            _blockLayer.AddChild(pipe);
            return pipe;
        }

        GD.PrintErr("[ConnectionFactory] Failed to create pipe");
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
        string fromName = from?.Name ?? "unknown";
        string toName = to?.Name ?? "unknown";
        GD.Print($"[ConnectionFactory] Creating pipe between blocks: {fromName} -> {toName}");

        var outputSocket = from?.GetOutputSocket() as Node2D ?? (from as Node2D);
        var inputSocket = to?.GetInputSocket() as Node2D ?? (to as Node2D);

        if (outputSocket == null || inputSocket == null)
        {
            GD.PrintErr($"[ConnectionFactory] Could not find required sockets for blocks {fromName} -> {toName}");
            return null;
        }

        if (from?.GetParent() is ToolbarBlockContainer || to?.GetParent() is ToolbarBlockContainer)
            return null;

        // Print the actual types for debugging
        GD.Print($"[ConnectionFactory] From block type: {from?.GetType()}, To block type: {to?.GetType()}");
        GD.Print($"[ConnectionFactory] From socket type: {outputSocket.GetType()}, To socket type: {inputSocket.GetType()}");

        var pipe = new ConnectionPipe();
        pipe.Initialize(outputSocket, inputSocket);
        return pipe;
    }

    public bool CanConnect(IBlock block, ConnectionPipe pipe)
    {
        if (block == pipe.SourceBlock)
        {
            // Trying to connect to the source, check if block already has an output
            return !block.HasOutputConnection();
        }
        else if (block == pipe.TargetBlock)
        {
            // Trying to connect to the target, check if block already has an input
            return !block.HasInputConnection();
        }
        else
        {
            // Not connecting to either end of the pipe
            return false;
        }
    }
}