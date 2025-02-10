using Godot;

namespace F;

public partial class ConnectionPipe : Node2D
{
    private Node2D? _fromSocket;
    private Node2D? _toSocket;
    private ColorRect? _pipe;
    private ShaderMaterial? _material;
    
    public Node2D? FromSocket => _fromSocket;
    public Node2D? ToSocket => _toSocket;
    
    public override void _Ready()
    {
        _pipe = GetNode<ColorRect>("Pipe");
        _material = _pipe?.Material as ShaderMaterial;
    }
    
    public void Initialize(Node2D fromSocket, Node2D toSocket)
    {
        _fromSocket = fromSocket;
        _toSocket = toSocket;
    }
    
    public override void _Process(double delta)
    {
        if (_fromSocket == null || _toSocket == null || _material == null) return;
        
        // Update shader parameters
        _material.SetShaderParameter("start_point", _fromSocket.GlobalPosition);
        _material.SetShaderParameter("end_point", _toSocket.GlobalPosition);
        
        // Update pipe position and size to cover the connection area
        if (_pipe != null)
        {
            var startPos = _fromSocket.GlobalPosition;
            var endPos = _toSocket.GlobalPosition;
            var rect = new Rect2(startPos, endPos - startPos);
            rect = rect.Abs();
            
            // Add padding for curved lines
            rect = rect.Grow(100);
            
            _pipe.GlobalPosition = rect.Position;
            _pipe.Size = rect.Size;
        }
    }
}
