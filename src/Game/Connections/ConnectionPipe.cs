using F.Config.Connection;
using F.Config.Visual;

// ADD THIS!

namespace F.Game.Connections;

public partial class ConnectionPipe : Node2D
{
    private Vector2 _endPoint;
    private BaseBlock? _fromBlock;
    private bool _isTemporary;
    private Vector2 _temporaryEndPoint;
    private Line2D? _visuals;

    public Node2D? FromSocket { get; private set; }

    public Node2D? ToSocket { get; private set; }

    public BaseBlock? StartBlock { get; private set; }

    public BaseBlock? EndBlock { get; private set; }

    public override void _Ready()
    {
        // Create Line2D for outline
        var outline = new Line2D
        {
            Name = "Outline",
            DefaultColor = Colors.Black,
            Width = 8.0f,
            BeginCapMode = Line2D.LineCapMode.Round,
            EndCapMode = Line2D.LineCapMode.Round,
            ZIndex = ZIndexConfig.Layers.Pipes - 1,
            ZAsRelative = false
        };
        AddChild(outline);

        // Create Line2D if it doesn't exist
        _visuals = GetNodeOrNull<Line2D>("VisualPipe");
        if (_visuals == null)
        {
            _visuals = new Line2D
            {
                Name = "VisualPipe",
                DefaultColor = Colors.White,
                Width = 4.0f,
                BeginCapMode = Line2D.LineCapMode.Round,
                EndCapMode = Line2D.LineCapMode.Round,
                Points = new[] { Vector2.Zero, Vector2.Zero },
                ZIndex = ZIndexConfig.Layers.Pipes, // ADD THIS!
                ZAsRelative = false // ADD THIS TOO!
            };
            AddChild(_visuals);
        }
        else if (_visuals == null)
        {
            GD.PrintErr("VisualPipe node not found!");
        }

        // Also set pipe's own Z-index
        ZIndex = ZIndexConfig.Layers.Pipes;
        ZAsRelative = false;
    }

    public void Initialize(Node2D fromSocket, Node2D toSocket)
    {
        FromSocket = fromSocket;
        ToSocket = toSocket;
        _fromBlock = fromSocket?.GetParent() as BaseBlock;
        EndBlock = toSocket?.GetParent() as BaseBlock;
        StartBlock = _fromBlock; // ADD THIS LINE - SET START BLOCK!

        GD.Print($"Initializing pipe - From: {_fromBlock?.Name}, To: {EndBlock?.Name}");

        // Make sure both blocks AND sockets exist
        if (_fromBlock == null || EndBlock == null || FromSocket == null || ToSocket == null)
        {
            GD.PrintErr(
                $"Cannot create connection - Blocks: {_fromBlock != null}, {EndBlock != null}, Sockets: {FromSocket != null}, {ToSocket != null}");
            return;
        }

        _isTemporary = false;
        UpdateVisuals();
    }

    public void Initialize(BaseBlock from, BaseBlock to) // NEW METHOD!
    {
        // Find sockets on blocks
        var fromSocket = from.GetNearestSocket(to.GlobalPosition);
        var toSocket = to.GetNearestSocket(from.GlobalPosition);

        if (fromSocket == null || toSocket == null)
        {
            GD.PrintErr("Could not find sockets on blocks!");
            return;
        }

        FromSocket = fromSocket;
        ToSocket = toSocket;
        _fromBlock = from;
        EndBlock = to;

        GD.Print($"Connected blocks with sockets: {fromSocket.Name} -> {toSocket.Name}");
        UpdateVisuals();
    }

    public void InitializeTemporary(BaseBlock startBlock)
    {
        // Initialize the pipe for a temporary connection starting from startBlock
        StartBlock = startBlock;
        _isTemporary = true;
        _endPoint = startBlock.GlobalPosition;
        UpdateVisuals();
    }

    public void InitializeTemporary(BaseBlock from, Vector2 endPoint)
    {
        _fromBlock = from;
        EndBlock = null;
        _isTemporary = true;
        _temporaryEndPoint = endPoint;

        UpdateVisuals();
    }

    public override void _Process(double delta)
    {
        if (FromSocket == null || ToSocket == null || _visuals == null) return;

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (FromSocket == null || ToSocket == null || _visuals == null) return;

        // Always use global positions for consistency
        var startPos = FromSocket.GlobalPosition;
        var endPos = _isTemporary ? _temporaryEndPoint : ToSocket.GlobalPosition;

        // Convert positions to local space only for drawing
        startPos = ToLocal(startPos);
        endPos = ToLocal(endPos);

        // Calculate control points for Bezier curve
        var distance = startPos.DistanceTo(endPos);
        var controlPointOffset = distance * 0.5f; // Adjust this value to control curve amount

        var control1 = startPos + new Vector2(controlPointOffset, 0);
        var control2 = endPos - new Vector2(controlPointOffset, 0);

        // Create curve points
        _visuals.ClearPoints();
        const int numPoints = 20; // More points = smoother curve

        for (float t = 0; t <= 1; t += 1f / numPoints)
        {
            var point = CubicBezier(startPos, control1, control2, endPos, t);
            _visuals.AddPoint(point);
        }

        _visuals.AddPoint(endPos); // Make sure we end exactly at the end point

        // Update outline points
        var outline = GetNode<Line2D>("Outline");
        if (outline != null)
        {
            outline.ClearPoints();
            foreach (var point in _visuals.Points)
            {
                outline.AddPoint(point);
            }
        }
    }

    private Vector2 CubicBezier(Vector2 start, Vector2 control1, Vector2 control2, Vector2 end, float t)
    {
        var tt = t * t;
        var ttt = tt * t;
        var u = 1 - t;
        var uu = u * u;
        var uuu = uu * u;

        return uuu * start +
               3 * uu * t * control1 +
               3 * u * tt * control2 +
               ttt * end;
    }

    public void SetHovered(bool isHovered)
    {
        // Check if this node is still valid
        if (!IsInstanceValid(this) || !IsInsideTree()) return;

        // Get and validate visual components
        _visuals = GetNodeOrNull<Line2D>("VisualPipe");
        var outline = GetNodeOrNull<Line2D>("Outline");
        
        if (_visuals == null || outline == null) return;

        // Update colors only if nodes are valid
        if (IsInstanceValid(_visuals))
        {
            _visuals.DefaultColor = isHovered
                ? new Color(0, 1, 1)  // BRIGHT CYAN WHEN HIGHLIGHTED
                : new Color(0.7f, 0.7f, 0.7f); // NORMAL GRAY
        }

        if (IsInstanceValid(outline))
        {
            outline.DefaultColor = isHovered ? new Color(0, 0.5f, 0.5f) : Colors.Black;
        }
    }

    public bool IsPointNearPipe(Vector2 point)
    {
        if (FromSocket == null || ToSocket == null) return false;

        // Create detection area between sockets
        var rect = new Rect2(FromSocket.GlobalPosition, Vector2.Zero);
        rect = rect.Expand(ToSocket.GlobalPosition);
        rect = rect.Grow(PipeConfig.Interaction.HoverDistance * 2); // Make detection area bigger
        
        return rect.HasPoint(point);
    }

    public (Node2D? From, Node2D? To) GetSockets()
    {
        return (FromSocket, ToSocket);
    }

    public void StartReconnectAnimation(Vector2[] oldPoints)
    {
        // Removed this method as it was not implemented for Line2D
    }

    public void ClearBulgeEffect()
    {
        // Removed this method as it was not implemented for Line2D
    }

    public void UpdateTemporaryEndPoint(Vector2 endPoint)
    {
        if (!_isTemporary) return;
        _temporaryEndPoint = endPoint;
        UpdateVisuals();
    }

    public void UpdateTokenPosition(Vector2 position)
    {
        // Removed this method as it was not implemented for Line2D
    }

    public Vector2 GetPositionAlongCurve(float t)
    {
        if (FromSocket == null || ToSocket == null) return Vector2.Zero;
        var startPos = FromSocket.GlobalPosition;
        var endPos = ToSocket.GlobalPosition;
        return startPos.Lerp(endPos, t);
    }

    public BaseBlock? GetOtherBlock(BaseBlock currentBlock)
    {
        if (FromSocket?.GetParent() is BaseBlock fromBlock && fromBlock != currentBlock)
            return fromBlock;
        if (ToSocket?.GetParent() is BaseBlock toBlock && toBlock != currentBlock)
            return toBlock;
        return null;
    }

    public void SetHighlighted(bool highlighted)
    {
        if (_visuals == null) return;

        // Make pipe REALLY glow when highlighted
        _visuals.DefaultColor = highlighted
            ? new Color(0, 1, 1)
            : // BRIGHT CYAN WHEN HIGHLIGHTED
            new Color(0.7f, 0.7f, 0.7f); // NORMAL GRAY

        // Make pipe thicker when highlighted
        _visuals.Width = highlighted ? 8.0f : 4.0f;
    }

    public bool TryConnectBlock(BaseBlock block)
    {
        GD.Print("=== TRYING TO CONNECT BLOCK ===");
        GD.Print($"Block position: {block.GlobalPosition}");
        GD.Print($"Current pipe from: {FromSocket?.Name}, to: {ToSocket?.Name}");

        if (FromSocket == null || ToSocket == null)
        {
            GD.PrintErr("Pipe has no sockets!");
            return false;
        }

        // Find input and output sockets on block
        var blockSockets = block.GetChildren()
            .OfType<Node2D>()
            .Where(n => n.Name.ToString().Contains("Socket"))
            .ToList();

        GD.Print($"Found {blockSockets.Count} sockets on block:");
        foreach (var socket in blockSockets) GD.Print($"- {socket.Name}");

        var inputSocket = blockSockets.FirstOrDefault(n => n.Name.ToString().Contains("Input"));
        var outputSocket = blockSockets.FirstOrDefault(n => n.Name.ToString().Contains("Output"));

        if (inputSocket == null || outputSocket == null)
        {
            GD.PrintErr(
                $"Block {block.Name} missing sockets! Input: {inputSocket != null}, Output: {outputSocket != null}");
            return false;
        }

        // Position block at center of pipe
        var oldPos = block.GlobalPosition;
        var newPos = GetPositionOnPipe(block);
        block.GlobalPosition = newPos;
        GD.Print($"Moved block from {oldPos} to {newPos}");

        // Connect both sockets
        FromSocket = outputSocket;
        ToSocket = inputSocket;
        _fromBlock = block;
        EndBlock = block;

        GD.Print($"Connected block {block.Name} with sockets: {outputSocket.Name} -> {inputSocket.Name}");
        UpdateVisuals();
        return true;
    }

    public Vector2 GetPositionOnPipe(BaseBlock block)
    {
        // Return the nearest point on the pipe to snap the block to
        if (FromSocket == null || ToSocket == null) return block.GlobalPosition;

        var pipeStart = FromSocket.GlobalPosition;
        var pipeEnd = ToSocket.GlobalPosition;

        // Calculate nearest point on line segment
        var vec = pipeEnd - pipeStart;
        var len = vec.Length();
        if (len == 0) return pipeStart;

        vec = vec / len;
        var v = block.GlobalPosition - pipeStart;
        var d = v.Dot(vec);
        d = Mathf.Clamp(d, 0, len);

        return pipeStart + vec * d;
    }
}