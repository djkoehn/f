using F.Framework.Blocks;
using F.Game.Tokens;
using F.Framework.Logging;

namespace F.Framework.Connections;

public partial class ConnectionPipe : Node2D
{
    private bool _hasActiveToken;
    private bool _isHighlighted;
    private bool _isInsertionHighlighted;
    private bool _isTemporary;
    private PipeVisuals? _pipeVisuals;
    private Vector2 _temporaryEndPoint;
    private Line2D? _visuals;

    public Node2D? FromSocket { get; private set; }

    public Node2D? ToSocket { get; private set; }

    public IBlock? SourceBlock { get; private set; }
    public IBlock? TargetBlock { get; private set; }

    public override void _Ready()
    {
        Logger.Connection.Print($"_Ready called on {Name}");

        // Set proper z-index for the pipe
        ZIndexConfig.SetZIndex(this, ZIndexConfig.Layers.Pipes);

        // Try to get existing nodes first
        var outline = GetNodeOrNull<Line2D>("Outline");
        _pipeVisuals = GetNodeOrNull<PipeVisuals>("PipeVisuals");

        Logger.Connection.Print($"Found nodes: Outline: {outline != null}, PipeVisuals: {_pipeVisuals != null}");

        // Create outline if it doesn't exist
        if (outline == null)
        {
            outline = new Line2D
            {
                Name = "Outline",
                DefaultColor = Colors.Black,
                Width = 8.0f,
                BeginCapMode = Line2D.LineCapMode.Round,
                EndCapMode = Line2D.LineCapMode.Round,
                JointMode = Line2D.LineJointMode.Round,
                ZIndex = -1,
                ZAsRelative = true,
                Antialiased = true
            };
            AddChild(outline);
        }

        // Create PipeVisuals if it doesn't exist
        if (_pipeVisuals == null)
        {
            // Load the Connection scene
            var connectionScene = GD.Load<PackedScene>("res://scenes/UI/Connection.tscn");
            if (connectionScene != null)
            {
                // Instance the scene
                var instance = connectionScene.Instantiate<Node2D>();
                if (instance != null)
                {
                    // Get PipeVisuals and its VisualPipe
                    _pipeVisuals = instance.GetNode<PipeVisuals>("PipeVisuals");
                    var visualPipe = _pipeVisuals?.GetNode<Line2D>("VisualPipe");
                    var bulgeEffect = _pipeVisuals?.GetNode<Line2D>("BulgeEffect");

                    if (_pipeVisuals != null && visualPipe != null && bulgeEffect != null)
                    {
                        // Store the original material
                        var originalMaterial = bulgeEffect.Material as ShaderMaterial;

                        // Clear ownership before moving nodes
                        _pipeVisuals.Owner = null;
                        visualPipe.Owner = null;
                        bulgeEffect.Owner = null;

                        // Remove from original parent and add to this node
                        instance.RemoveChild(_pipeVisuals);
                        AddChild(_pipeVisuals);

                        // Set new ownership
                        _pipeVisuals.Owner = this;
                        visualPipe.Owner = this;
                        bulgeEffect.Owner = this;

                        // Set z-indices
                        _pipeVisuals.ZIndex = 0;
                        _pipeVisuals.ZAsRelative = true;
                        visualPipe.ZIndex = 0;
                        visualPipe.ZAsRelative = true;
                        bulgeEffect.ZIndex = 1;
                        bulgeEffect.ZAsRelative = true;

                        // Ensure material is preserved
                        if (originalMaterial != null)
                        {
                            // Create a duplicate of the material to avoid sharing
                            var newMaterial = originalMaterial.Duplicate() as ShaderMaterial;
                            if (newMaterial != null)
                            {
                                bulgeEffect.Material = newMaterial;
                                Logger.Connection.Print("Created new shader material");
                            }
                        }

                        Logger.Connection.Print("Successfully created PipeVisuals from scene");
                    }

                    // Clean up the temporary instance
                    instance.QueueFree();
                }
            }
            else
            {
                Logger.Connection.Err("Failed to load Connection scene!");
                _pipeVisuals = new PipeVisuals { Name = "PipeVisuals" };
                AddChild(_pipeVisuals);
                _pipeVisuals.Owner = this;
            }
        }

        // Get the VisualPipe reference after PipeVisuals is ready
        _visuals = _pipeVisuals?.GetNode<Line2D>("VisualPipe");
        if (_visuals == null)
        {
            Logger.Connection.Err($"Failed to get VisualPipe node from {_pipeVisuals?.Name}!");
            var children = _pipeVisuals?.GetChildren().Select(c => c.Name.ToString()).ToList() ?? new List<string>();
            Logger.Connection.Print($"PipeVisuals children: {string.Join(", ", children)}");
            return;
        }

        Logger.Connection.Print("Successfully got VisualPipe node");
    }

    public void Initialize(Node2D fromSocket, Node2D toSocket)
    {
        FromSocket = fromSocket;
        ToSocket = toSocket;
        _isTemporary = false;

        // Find blocks in hierarchy
        Node? fromBlock = fromSocket;
        Node? toBlock = toSocket;

        while (fromBlock != null && !(fromBlock is IBlock))
            fromBlock = fromBlock.GetParent();

        while (toBlock != null && !(toBlock is IBlock))
            toBlock = toBlock.GetParent();

        SourceBlock = fromBlock as IBlock;
        TargetBlock = toBlock as IBlock;

        if (SourceBlock == null || TargetBlock == null)
        {
            Logger.Connection.Err("Failed to find blocks in hierarchy");
            return;
        }

        var sourceName = SourceBlock.Name ?? "unknown";
        var targetName = TargetBlock.Name ?? "unknown";

        // Set a unique name for the pipe based on the connected blocks
        Name = $"Pipe_{sourceName}_{targetName}";
        Logger.Connection.Print($"Initialized pipe {Name} between {sourceName} and {targetName}");

        UpdateVisuals();
    }

    public void InitializeTemporary(IBlock startBlock)
    {
        var outputSocket = startBlock.GetOutputSocket() as Node2D;
        if (outputSocket == null)
        {
            Logger.Connection.Err("Failed to get output socket for temporary connection");
            return;
        }

        FromSocket = outputSocket;
        _isTemporary = true;
        _temporaryEndPoint = startBlock.GlobalPosition;
        SourceBlock = startBlock;
        Name = $"TempPipe_{startBlock.Name}";
        Logger.Connection.Print($"Initialized temporary pipe {Name} from {startBlock.Name}");
        UpdateVisuals();
    }

    public void InitializeTemporary(IBlock from, Vector2 endPoint)
    {
        var outputSocket = from.GetOutputSocket() as Node2D;
        if (outputSocket == null)
        {
            Logger.Connection.Err("Failed to get output socket for temporary connection");
            return;
        }

        FromSocket = outputSocket;
        _isTemporary = true;
        _temporaryEndPoint = endPoint;
        SourceBlock = from;
        Name = $"TempPipe_{from.Name}";
        Logger.Connection.Print($"Initialized temporary pipe {Name} from {from.Name}");
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
        if (FromSocket == null || (ToSocket == null && !_isTemporary) || _visuals == null) return;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (FromSocket == null || (ToSocket == null && !_isTemporary) || _visuals == null) return;

        var startPos = ToLocal(FromSocket.GlobalPosition);
        var endPos = _isTemporary ? ToLocal(_temporaryEndPoint) : ToLocal(ToSocket.GlobalPosition);

        var curvePoints = PipeCurveCalculator.CalculateCurvePoints(startPos, endPos);

        _visuals.ClearPoints();
        foreach (var pt in curvePoints) _visuals.AddPoint(pt);

        var outline = GetNode<Line2D>("Outline");
        if (outline != null)
        {
            outline.ClearPoints();
            foreach (var pt in curvePoints) outline.AddPoint(pt);
        }

        // Update shader parameters if material exists
        if (_visuals.Material is ShaderMaterial material)
        {
            var points = new Vector2[50]; // Match shader's fixed size array
            for (var i = 0; i < curvePoints.Count && i < 50; i++) points[i] = ToGlobal(curvePoints[i]);
            material.SetShaderParameter("curve_points", points);
            material.SetShaderParameter("point_count", Mathf.Min(curvePoints.Count, 50));
        }

        // Hide pipe if either end block is not visible
        if ((SourceBlock is Node2D sourceNode && !sourceNode.Visible) ||
            (TargetBlock is Node2D targetNode && !targetNode.Visible))
            HidePipe();
        else
            ShowPipe();
    }

    public bool IsPointNearPipe(Vector2 point)
    {
        if (FromSocket == null || ToSocket == null || _visuals == null) return false;

        var curvePoints = GetCurvePoints();
        if (curvePoints.Count == 0) return false;

        var minDistance = float.MaxValue;
        for (var i = 0; i < curvePoints.Count - 1; i++)
        {
            var distance = DistanceToLineSegment(point, curvePoints[i], curvePoints[i + 1]);
            minDistance = Mathf.Min(minDistance, distance);
        }

        return minDistance <= PipeConfig.Interaction.HoverDistance;
    }

    private float DistanceToLineSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        var line = end - start;
        var len = line.Length();
        if (len == 0) return point.DistanceTo(start);

        var t = Mathf.Clamp((point - start).Dot(line) / (len * len), 0, 1);
        var projection = start + line * t;

        return point.DistanceTo(projection);
    }

    public IBlock? GetOtherBlock(IBlock currentBlock)
    {
        if (SourceBlock != null && !SourceBlock.Equals(currentBlock)) return SourceBlock;
        if (TargetBlock != null && !TargetBlock.Equals(currentBlock)) return TargetBlock;
        return null;
    }

    public (Node2D? From, Node2D? To) GetSockets()
    {
        return (FromSocket, ToSocket);
    }

    public void SetHighlighted(bool highlighted)
    {
        if (_visuals == null) return;
        _isHighlighted = highlighted;
        _visuals.DefaultColor = highlighted ? new Color(0, 1, 1) : new Color(0.7f, 0.7f, 0.7f);
        _visuals.Width = highlighted ? 8.0f : 4.0f;
    }

    public void SetInsertionHighlight(bool highlighted)
    {
        if (_visuals != null && _isInsertionHighlighted != highlighted)
        {
            _isInsertionHighlighted = highlighted;
            _visuals.DefaultColor = highlighted ? new Color(0, 1, 0) : PipeConfig.Visual.LineColor;

            var outline = GetNode<Line2D>("Outline");
            if (outline != null) outline.Width = highlighted ? 10.0f : 8.0f;
        }
    }

    public void ClearInsertionHighlight()
    {
        SetInsertionHighlight(false);
    }

    public bool IsHighlighted()
    {
        return _isInsertionHighlighted || _isHighlighted;
    }

    public List<Vector2> GetCurvePoints()
    {
        List<Vector2> globalPoints = new();
        if (_visuals != null)
            foreach (var localPt in _visuals.Points)
                globalPoints.Add(_visuals.ToGlobal(localPt));

        return globalPoints;
    }

    private void HidePipe()
    {
        if (_visuals != null) _visuals.Visible = false;
        var outline = GetNode<Line2D>("Outline");
        if (outline != null) outline.Visible = false;
    }

    private void ShowPipe()
    {
        if (_visuals != null) _visuals.Visible = true;
        var outline = GetNode<Line2D>("Outline");
        if (outline != null) outline.Visible = true;
    }

    public void RemovePipe()
    {
        if (IsInsideTree()) GetParent()?.RemoveChild(this);
        QueueFree();
    }

    public void StartTokenMovement(Token token)
    {
        _hasActiveToken = true;
        _pipeVisuals?.StartTokenMovement();
    }

    public void UpdateTokenPosition(Token token)
    {
        if (!_hasActiveToken) return;
        _pipeVisuals?.UpdateTokenPosition(token.GlobalPosition);
    }

    public void EndTokenMovement(Token token)
    {
        _hasActiveToken = false;
        _pipeVisuals?.EndTokenMovement();
    }
}