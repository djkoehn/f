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
        string fromName = from?.Name ?? "unknown";
        string toName = to?.Name ?? "unknown";
        GD.Print($"Creating pipe between blocks: {fromName} -> {toName}");
        
        // Try to retrieve sockets; if null, use the block itself if possible
        var outputSocket = from?.GetOutputSocket() as Node2D ?? (from as Node2D);
        var inputSocket = to?.GetInputSocket() as Node2D ?? (to as Node2D);

        if (outputSocket == null || inputSocket == null)
        {
            GD.PrintErr($"Could not find required sockets! Output: {outputSocket != null}, Input: {inputSocket != null}");
            return null;
        }
        
        // Enforce that a block can have only one connection per socket.
        if (ConnectionHelper.HasOutputConnection(from) || 
            ConnectionHelper.HasInputConnection(to))
        {
            GD.PrintErr($"One or both blocks already have a connection! ({fromName} or {toName})");
            return null;
        }
        
        if (from?.GetParent() is ToolbarBlockContainer || to?.GetParent() is ToolbarBlockContainer)
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
        string fromName = from?.Name ?? "unknown";
        string toName = to?.Name ?? "unknown";
        GD.Print($"(static) Creating pipe between blocks: {fromName} -> {toName}");
        
        var outputSocket = from?.GetOutputSocket() as Node2D ?? (from as Node2D);
        var inputSocket = to?.GetInputSocket() as Node2D ?? (to as Node2D);
        
        if (outputSocket == null || inputSocket == null)
        {
            GD.PrintErr($"Could not find required sockets! Output: {outputSocket != null}, Input: {inputSocket != null}");
            return null;
        }
        
        // Enforce that a block can have only one connection per socket.
        if (from?.HasConnections() == true || to?.HasConnections() == true)
        {
            GD.PrintErr($"One or both blocks already have a connection! ({fromName} or {toName})");
            return null;
        }
        
        if (from?.GetParent() is ToolbarBlockContainer || to?.GetParent() is ToolbarBlockContainer)
            return null;

        var pipe = new ConnectionPipe();
        pipe.Initialize(outputSocket, inputSocket);
        return pipe;
    }

    // New static method to create a pipe for insertion bypassing the connection check
    public static ConnectionPipe CreatePipeForInsertion(IBlock from, IBlock to)
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
            return !ConnectionHelper.HasOutputConnection(block);
        }
        else if (block == pipe.TargetBlock)
        {
            // Trying to connect to the target, check if block already has an input
            return !ConnectionHelper.HasInputConnection(block);  
        }
        else
        {
            // Not connecting to either end of the pipe
            return false;
        }
    }
}