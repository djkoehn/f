namespace F.Game.Connections;

public partial class ConnectionPipe : Node2D
{
    private Node2D? _fromSocket;
    private Node2D? _toSocket;
    private Line2D? _visuals;
    private bool _isHighlighted;
    private bool _isTemporary;
    private Vector2 _temporaryEndPoint;

    public Node2D? FromSocket => _fromSocket;
    public Node2D? ToSocket => _toSocket;
    public IBlock? SourceBlock { get; private set; }
    public IBlock? TargetBlock { get; private set; }

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
                ZIndex = ZIndexConfig.Layers.Pipes,
                ZAsRelative = false
            };
            AddChild(_visuals);
        }

        // Set pipe's own Z-index
        ZIndex = ZIndexConfig.Layers.Pipes;
        ZAsRelative = false;
    }

    public void Initialize(Node2D fromSocket, Node2D toSocket)
    {
        _fromSocket = fromSocket;
        _toSocket = toSocket;
        _isTemporary = false;

        // Try to retrieve the blocks by traversing up the hierarchy
        Node? fromBlock = fromSocket;
        Node? toBlock = toSocket;

        // Keep going up until we find an IBlock or hit null
        while (fromBlock != null && !(fromBlock is IBlock))
        {
            fromBlock = fromBlock.GetParent();
        }

        while (toBlock != null && !(toBlock is IBlock))
        {
            toBlock = toBlock.GetParent();
        }

        // Store the blocks as IBlock
        SourceBlock = fromBlock as IBlock;
        TargetBlock = toBlock as IBlock;

        GD.Print($"[ConnectionPipe] Socket hierarchy - FromSocket: {fromSocket.Name}, ToSocket: {toSocket.Name}");
        GD.Print($"[ConnectionPipe] Found blocks - Source: {SourceBlock?.GetType()}, Target: {TargetBlock?.GetType()}");
        
        if (SourceBlock == null || TargetBlock == null)
        {
            GD.PrintErr($"[ConnectionPipe] Failed to find blocks in hierarchy - FromSocket path: {GetNodePath(fromSocket)}, ToSocket path: {GetNodePath(toSocket)}");
            return;
        }

        UpdateVisuals();
    }

    private string GetNodePath(Node node)
    {
        string path = node.Name;
        Node? current = node;
        while (current.GetParent() != null)
        {
            current = current.GetParent();
            path = current.Name + "/" + path;
        }
        return path;
    }

    public void InitializeTemporary(IBlock startBlock)
    {
        var outputSocket = startBlock.GetOutputSocket() as Node2D;
        if (outputSocket == null)
        {
            GD.PrintErr("[ConnectionPipe] Failed to get output socket for temporary connection");
            return;
        }

        _fromSocket = outputSocket;
        _isTemporary = true;
        _temporaryEndPoint = startBlock.GlobalPosition;
        
        if (startBlock is BaseBlock baseBlock)
        {
            SourceBlock = baseBlock;
        }
        
        UpdateVisuals();
    }

    public void InitializeTemporary(IBlock from, Vector2 endPoint)
    {
        var outputSocket = from.GetOutputSocket() as Node2D;
        if (outputSocket == null)
        {
            GD.PrintErr("[ConnectionPipe] Failed to get output socket for temporary connection");
            return;
        }

        _fromSocket = outputSocket;
        _isTemporary = true;
        _temporaryEndPoint = endPoint;
        
        if (from is BaseBlock baseBlock)
        {
            SourceBlock = baseBlock;
        }
        
        UpdateVisuals();
    }

    public void UpdateTemporaryEndPoint(Vector2 endPoint)
    {
        if (!_isTemporary) return;
        _temporaryEndPoint = endPoint;
        UpdateVisuals();
    }

    public override void _Process(double delta)
    {
        if (_fromSocket == null || (_toSocket == null && !_isTemporary) || _visuals == null) return;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_fromSocket == null || (_toSocket == null && !_isTemporary) || _visuals == null) return;

        var startPos = ToLocal(_fromSocket.GlobalPosition);
        var endPos = _isTemporary ? ToLocal(_temporaryEndPoint) : ToLocal(_toSocket.GlobalPosition);

        // Use PipeCurveCalculator to compute curve points
        var curvePoints = F.Game.Connections.Helpers.PipeCurveCalculator.CalculateCurvePoints(startPos, endPos);

        _visuals.ClearPoints();
        foreach (var pt in curvePoints)
        {
            _visuals.AddPoint(pt);
        }

        // Update outline based on new visual points
        var outline = GetNode<Line2D>("Outline");
        if (outline != null)
        {
            outline.ClearPoints();
            foreach (var pt in _visuals.Points)
            {
                outline.AddPoint(pt);
            }
        }

        // Hide pipe if either end block is not visible
        if (SourceBlock is Node2D sourceNode && !sourceNode.Visible ||
            TargetBlock is Node2D targetNode && !targetNode.Visible)
        {
            HidePipe();
        }
        else
        {
            ShowPipe();
        }
    }

    public bool IsPointNearPipe(Vector2 point)
    {
        if (_fromSocket == null || _toSocket == null || _visuals == null) 
        {
            GD.PrintErr("[ConnectionPipe] Cannot check point near pipe - missing sockets or visuals");
            return false;
        }

        // Get the curve points in global coordinates
        var curvePoints = GetCurvePoints();
        if (curvePoints.Count == 0)
        {
            GD.PrintErr("[ConnectionPipe] No curve points available for detection");
            return false;
        }

        // Check distance to each curve segment
        float minDistance = float.MaxValue;
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            var start = curvePoints[i];
            var end = curvePoints[i + 1];
            
            // Calculate distance from point to line segment
            var distance = DistanceToLineSegment(point, start, end);
            minDistance = Mathf.Min(minDistance, distance);
        }

        bool isNear = minDistance <= PipeConfig.Interaction.HoverDistance;
        if (isNear)
        {
            GD.Print($"[ConnectionPipe] Point {point} is near pipe (distance: {minDistance})");
        }
        return isNear;
    }

    private float DistanceToLineSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        var line = end - start;
        float len = line.Length();
        if (len == 0) return point.DistanceTo(start);

        // Calculate projection
        float t = Mathf.Clamp(((point - start).Dot(line)) / (len * len), 0, 1);
        var projection = start + line * t;

        return point.DistanceTo(projection);
    }

    public (Node2D? From, Node2D? To) GetSockets()
    {
        return (_fromSocket, _toSocket);
    }

    public Vector2 GetPositionAlongCurve(float t)
    {
        if (_fromSocket == null || _toSocket == null) return Vector2.Zero;
        var startPos = _fromSocket.GlobalPosition;
        var endPos = _toSocket.GlobalPosition;
        return startPos.Lerp(endPos, t);
    }

    public IBlock? GetOtherBlock(IBlock currentBlock)
    {
        // First try to match against SourceBlock
        if (SourceBlock != null && !SourceBlock.Equals(currentBlock))
        {
            GD.Print($"[ConnectionPipe] GetOtherBlock returning SourceBlock: {SourceBlock.Name}");
            return SourceBlock;
        }

        // Then try TargetBlock
        if (TargetBlock != null && !TargetBlock.Equals(currentBlock))
        {
            GD.Print($"[ConnectionPipe] GetOtherBlock returning TargetBlock: {TargetBlock.Name}");
            return TargetBlock;
        }

        GD.Print($"[ConnectionPipe] GetOtherBlock found no matching block for {currentBlock.Name}");
        return null;
    }

    public void SetHighlighted(bool highlighted)
    {
        if (_visuals == null) return;
        _isHighlighted = highlighted;

        // Make pipe REALLY glow when highlighted
        _visuals.DefaultColor = highlighted
            ? new Color(0, 1, 1)  // BRIGHT CYAN WHEN HIGHLIGHTED
            : new Color(0.7f, 0.7f, 0.7f); // NORMAL GRAY

        // Make pipe thicker when highlighted
        _visuals.Width = highlighted ? 8.0f : 4.0f;
    }

    public void SetInsertionHighlight(bool highlighted)
    {
        if (_visuals != null)
        {
            _visuals.DefaultColor = highlighted ? new Color(0, 1, 0) : PipeConfig.Visual.LineColor;
        }
    }

    public void ClearInsertionHighlight()
    {
        SetInsertionHighlight(false);
    }

    public List<Vector2> GetCurvePoints()
    {
        List<Vector2> globalPoints = new List<Vector2>();
        if (_visuals != null)
        {
            foreach (var localPt in _visuals.Points)
            {
                Vector2 globalPt = _visuals.ToGlobal(localPt);
                globalPoints.Add(globalPt);
                GD.Print($"[ConnectionPipe Debug] global curve point from visuals: {globalPt}");
            }
            GD.Print("[ConnectionPipe Debug] Using _visuals points for curve.");
        }
        else
        {
            GD.PrintErr("[ConnectionPipe Debug] _visuals is null, cannot retrieve curve points.");
        }
        return globalPoints;
    }

    public void HidePipe()
    {
        if (_visuals != null)
        {
            _visuals.Visible = false;
        }

        var outline = GetNode<Line2D>("Outline");
        if (outline != null)
        {
            outline.Visible = false;
        }
    }

    public void ShowPipe()
    {
        if (_visuals != null)
        {
            _visuals.Visible = true;
        }

        var outline = GetNode<Line2D>("Outline");
        if (outline != null)
        {
            outline.Visible = true;
        }
    }

    public void RemovePipe()
    {
        if (IsInsideTree())
        {
            GetParent()?.RemoveChild(this);
        }
        QueueFree();
    }
}