using Godot;
using F.Game.BlockLogic;
using F.Game.Toolbar;
namespace F.Game.Connections;

public sealed class ConnectionFactory
{
    private readonly Node2D _parent;

    public ConnectionFactory(Node2D parent)
    {
        _parent = parent;
    }

    public ConnectionPipe CreateConnection(IBlock from, IBlock to)
    {
        GD.Print($"Creating pipe between blocks: {from.Name} -> {to.Name}");
        
        // Try to retrieve sockets; if null, use the block itself if possible
        var outputSocket = from.GetOutputSocket() as Node2D ?? (from as Node2D);
        var inputSocket = to.GetInputSocket() as Node2D ?? (to as Node2D);

        if (outputSocket == null || inputSocket == null)
        {
            GD.PrintErr($"Could not find required sockets! Output: {outputSocket != null}, Input: {inputSocket != null}");
            return null;
        }
        
        // Enforce that a block can have only one connection per socket.
        if (from.HasConnections() || to.HasConnections())
        {
            GD.PrintErr($"One or both blocks already have a connection! ({from.Name} or {to.Name})");
            return null;
        }
        
        if (from.GetParent() is ToolbarBlockContainer || to.GetParent() is ToolbarBlockContainer)
            return null;

        var pipe = new ConnectionPipe();
        pipe.Initialize(outputSocket, inputSocket);
        return pipe;
    }

    public ConnectionPipe CreateTemporaryConnection(IBlock from, Vector2 endPoint)
    {
        var pipe = new ConnectionPipe();
        // Remove deferred add since parent will handle it
        pipe.InitializeTemporary(from, endPoint);
        return pipe;
    }

    // Added a static method to create a connection pipe between two blocks
    public static ConnectionPipe CreatePipe(IBlock from, IBlock to)
    {
        GD.Print($"(static) Creating pipe between blocks: {from.Name} -> {to.Name}");
        
        var outputSocket = from.GetOutputSocket() as Node2D ?? (from as Node2D);
        var inputSocket = to.GetInputSocket() as Node2D ?? (to as Node2D);
        
        if (outputSocket == null || inputSocket == null)
        {
            GD.PrintErr($"Could not find required sockets! Output: {outputSocket != null}, Input: {inputSocket != null}");
            return null;
        }
        
        // Enforce that a block can have only one connection per socket.
        if (from.HasConnections() || to.HasConnections())
        {
            GD.PrintErr($"One or both blocks already have a connection! ({from.Name} or {to.Name})");
            return null;
        }
        
        if (from.GetParent() is ToolbarBlockContainer || to.GetParent() is ToolbarBlockContainer)
            return null;

        var pipe = new ConnectionPipe();
        pipe.Initialize(outputSocket, inputSocket);
        return pipe;
    }

    // New static method to create a pipe for insertion bypassing the connection check
    public static ConnectionPipe CreatePipeForInsertion(IBlock from, IBlock to)
    {
        GD.Print($"(static insertion) Creating pipe between blocks: {from.Name} -> {to.Name}");
        var outputSocket = from.GetOutputSocket() as Node2D ?? (from as Node2D);
        var inputSocket = to.GetInputSocket() as Node2D ?? (to as Node2D);
        if (outputSocket == null || inputSocket == null)
        {
            GD.PrintErr($"Could not find required sockets! Output: {outputSocket != null}, Input: {inputSocket != null}");
            return null;
        }
        if (from.GetParent() is ToolbarBlockContainer || to.GetParent() is ToolbarBlockContainer)
            return null;

        // Print the actual types for debugging
        GD.Print($"From block type: {from.GetType()}, To block type: {to.GetType()}");
        GD.Print($"From socket type: {outputSocket.GetType()}, To socket type: {inputSocket.GetType()}");

        var pipe = new ConnectionPipe();
        pipe.Initialize(outputSocket, inputSocket);
        return pipe;
    }
}