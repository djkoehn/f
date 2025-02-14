using F.UI.Toolbar;

namespace F.Game.Connections;

public sealed class ConnectionFactory
{
    private readonly Node _parent;

    public ConnectionFactory(Node parent)
    {
        _parent = parent;
    }

    public ConnectionPipe CreateConnection(BaseBlock from, BaseBlock to)
    {
        GD.Print($"Creating pipe between blocks: {from.Name} -> {to.Name}");

        var fromSocketName = from.GetType().Name.Contains("Input") ? "Output" : "Output";
        var toSocketName = to.GetType().Name.Contains("Output") ? "Input" : "Input";

        if (from.GetParent() is ToolbarBlockContainer ||
            to.GetParent() is ToolbarBlockContainer)
            return new ConnectionPipe();

        var pipe = new ConnectionPipe();
        // Remove deferred add since parent will handle it

        var outputSocket = GetSocket(from, fromSocketName);
        var inputSocket = GetSocket(to, toSocketName);

        if (outputSocket == null || inputSocket == null)
        {
            GD.PrintErr($"Could not find sockets! Output: {outputSocket != null}, Input: {inputSocket != null}");
            return pipe;
        }

        pipe.Initialize(outputSocket, inputSocket);
        return pipe;
    }

    private Node2D? GetSocket(BaseBlock block, string socketName)
    {
        var socket = block.GetChildren()
            .OfType<Node2D>()
            .FirstOrDefault(n => n.Name.ToString().Contains(socketName));

        if (socket == null && block is Node2D blockNode &&
            blockNode.Name.ToString().Contains(socketName))
            return blockNode;

        return socket;
    }

    public ConnectionPipe CreateTemporaryConnection(BaseBlock from, Vector2 endPoint)
    {
        var pipe = new ConnectionPipe();
        // Remove deferred add since parent will handle it
        pipe.InitializeTemporary(from, endPoint);
        return pipe;
    }
}