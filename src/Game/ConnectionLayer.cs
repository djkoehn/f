using Godot;
using System.Collections.Generic;

namespace F;

public partial class ConnectionLayer : Node2D
{
    private Input? _inputBlock;
    private Output? _outputBlock;
    private List<ConnectionPipe> _connections = new();
    
    public override void _Ready()
    {
        // Get input/output blocks
        _inputBlock = GetNode<Input>("Input");
        _outputBlock = GetNode<Output>("Output");
        
        if (_inputBlock == null || _outputBlock == null)
        {
            GD.PrintErr("Input or Output block not found in ConnectionLayer!");
            return;
        }
        
        // Connect output block signal
        _outputBlock.TokenProcessed += OnTokenProcessed;
    }
    
    private void OnTokenProcessed(float value)
    {
        // Handle processed token value
        GD.Print($"Token processed with final value: {value}");
        
        // Emit signal to GameManager
        EmitSignal(SignalName.TokenProcessed, value);
    }
    
    public void SetInputValue(float value)
    {
        _inputBlock?.SetValue(value);
    }
    
    public void ProcessToken()
    {
        if (_inputBlock == null || _outputBlock == null) return;
        
        var value = _inputBlock.GetValue();
        _outputBlock.ProcessValue(value);
    }
    
    [Signal]
    public delegate void TokenProcessedEventHandler(float value);
}
