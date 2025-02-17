using F.Game.Tokens;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using F.Game.Connections;
using System.Dynamic;
using RoslynScript = Microsoft.CodeAnalysis.Scripting.Script;
using RoslynCSharpScript = Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript;
// for IBlock, if needed

namespace F.Game.BlockLogic
{
    // New enum to represent the state of a block

    // Marking the class as partial and implementing IBlock
    public partial class BaseBlock : Node2D, IBlock
    {
        // Explicit interface implementation for IBlock.Name to avoid conflict with Node.Name
        private string _blockName = "";
        private bool _isConnected;
        private bool _isInputConnected;
        private bool _isOutputConnected;

        private ConnectionManager _connectionManager;

        string IBlock.Name
        {
            get => _blockName;
            set => _blockName = value;
        }

        public BlockState State { get; set; } = BlockState.InToolbar;
        public BlockMetadata? Metadata { get; set; }

        public virtual void Initialize(BlockConfig config)
        {
            // Default initialization for draggable blocks
        }

        // Explicit implementation for IBlock.Initialize(object config)
        void IBlock.Initialize(object config)
        {
            if (config is BlockConfig bc)
                Initialize(bc);
            else
                GD.PrintErr("Invalid config passed to BaseBlock.Initialize");
        }

        // Declare the partial method for initializing dragging functionality; implementation provided in BaseBlock.Dragging.cs
        partial void InitializeDragging();

        public virtual void ProcessToken(Token token)
        {
            GD.Print($"[BaseBlock Debug] Processing token in block {Name}");
            
            // First run the block-specific processing from metadata
            if (Metadata != null && !string.IsNullOrEmpty(Metadata.ProcessTokenScript))
            {
                try
                {
                    // Create the script with a declared variable
                    string scriptWithVar = $@"
                        float Value = {token.Value};
                        Action<string> Print = Godot.GD.Print;
                        Action<string> PrintErr = Godot.GD.PrintErr;
                        {Metadata.ProcessTokenScript}
                        return Value;
                    ";

                    // Set up script options with necessary references and imports
                    var options = ScriptOptions.Default
                        .WithImports("System")
                        .WithReferences(typeof(GD).Assembly)
                        .WithOptimizationLevel(OptimizationLevel.Release);
                    
                    GD.Print($"[BaseBlock Debug] Executing script for block {Name} with script: {scriptWithVar}");
                    var result = RoslynCSharpScript.RunAsync<float>(scriptWithVar, options).Result;
                    
                    // Update token value from result
                    token.Value = result.ReturnValue;
                    
                    GD.Print($"[BaseBlock Debug] Script execution completed for block {Name}");
                }
                catch (Exception e)
                {
                    GD.PrintErr($"[BaseBlock Debug] Error processing token script in block {Name}: {e.Message}");
                    // Continue with common processing even if script fails
                }
            }

            // Then handle common token processing
            ProcessTokenCommon(token);
        }

        protected void ProcessTokenCommon(Token token)
        {
            GD.Print($"[BaseBlock Debug] Starting common token processing in block {Name}");
            
            // Add this block to the token's processed blocks list
            token.ProcessedBlocks.Add(this);
            GD.Print($"[BaseBlock Debug] Added block {Name} to token's processed blocks");

            // Handle token movement to next block or cleanup
            if (_connectionManager != null)
            {
                var (nextBlock, pipe) = _connectionManager.GetNextConnection(this);
                GD.Print($"[BaseBlock Debug] GetNextConnection returned nextBlock: {(nextBlock != null ? nextBlock.Name : "null")}, pipe: {(pipe != null ? "valid" : "null")}");
                
                if (nextBlock != null)
                {
                    GD.Print($"[BaseBlock Debug] Moving token from block {Name} to block {nextBlock.Name}");
                    token.MoveTo(nextBlock);
                }
                else
                {
                    GD.Print($"[BaseBlock Debug] No next block found, destroying token in block {Name}");
                    token.QueueFree();
                }
            }
            else
            {
                GD.PrintErr($"[BaseBlock Debug] ConnectionManager is null in block {Name}, destroying token");
                token.QueueFree();
            }
        }

        // Default virtual implementation for HasConnections; override if needed
        public virtual bool HasConnections()
        {
            return _isInputConnected || _isOutputConnected;
        }

        public bool HasInputConnection()
        {
            return _isInputConnected;
        }

        public bool HasOutputConnection()
        {
            return _isOutputConnected;
        }

        public void SetInputConnected(bool connected)
        {
            _isInputConnected = connected;
            _isConnected = _isInputConnected || _isOutputConnected;
        }

        public void SetOutputConnected(bool connected)
        {
            _isOutputConnected = connected;
            _isConnected = _isInputConnected || _isOutputConnected;
        }

        // Default virtual implementation for GetTokenPosition; returns GlobalPosition
        public virtual Vector2 GetTokenPosition()
        {
            return GlobalPosition;
        }

        // Default virtual implementation for GetInputSocket; override in dragging partial if necessary
        public virtual Node? GetInputSocket()
        {
            var socket = GetNodeOrNull<Node2D>("BlockInputSocket");
            if (socket == null)
            {
                GD.PrintErr($"[BaseBlock] Input socket not found for block {Name}");
            }
            return socket;
        }

        // Default virtual implementation for GetOutputSocket; override in dragging partial if necessary
        public virtual Node? GetOutputSocket()
        {
            var socket = GetNodeOrNull<Node2D>("BlockOutputSocket");
            if (socket == null)
            {
                GD.PrintErr($"[BaseBlock] Output socket not found for block {Name}");
            }
            return socket;
        }

        // Helper methods to manage block state
        public void SetDragging(bool isDragging)
        {
            var oldState = State;
            if (isDragging)
            {
                State = BlockState.Dragging;
            }
            else
            {
                if (State == BlockState.Dragging)
                    State = BlockState.Placed;
            }
            GD.Print($"[BaseBlock Debug] Block {Name} state changed: {oldState} -> {State} (SetDragging: {isDragging})");
        }

        public void SetPlaced(bool isPlaced)
        {
            var oldState = State;
            if (isPlaced)
                State = BlockState.Placed;
            else
            {
                if (State == BlockState.Placed)
                    State = BlockState.Dragging;
            }
            GD.Print($"[BaseBlock Debug] Block {Name} state changed: {oldState} -> {State} (SetPlaced: {isPlaced})");
        }

        public void CompleteConnection()
        {
            var oldState = State;
            State = BlockState.Connected;
            GD.Print($"[BaseBlock Debug] Block {Name} state changed: {oldState} -> {State} (CompleteConnection)");
        }

        public void SetInToolbar(bool value)
        {
            var oldState = State;
            if (value)
                State = BlockState.InToolbar;
            else if (State == BlockState.InToolbar)
                State = BlockState.Placed;
            GD.Print($"[BaseBlock Debug] Block {Name} state changed: {oldState} -> {State} (SetInToolbar: {value})");
        }

        public override void _Ready()
        {
            base._Ready();
            GD.Print($"[Debug BaseBlock] {Name} _Ready called, adding to Blocks group");
            AddToGroup("Blocks");
            InitializeDragging();
            // Enable processing so that _Process callback is active
            SetProcess(true);
            GD.Print($"[Debug BaseBlock] {Name} initialization complete, Position: {GlobalPosition}, State: {State}");
            _connectionManager = GetNode<ConnectionManager>("/root/Main/GameManager/BlockLayer");
        }

        public virtual void ResetConnections()
        {
            GD.Print($"[BaseBlock Debug] Block {Name} resetting connections. Current state: {State}");
            // Reset connection state flags
            _isConnected = false;
            _isInputConnected = false;
            _isOutputConnected = false;
            
            // Reset socket states by updating their properties directly
            var inputSocket = GetInputSocket();
            var outputSocket = GetOutputSocket();
            
            if (inputSocket != null)
            {
                // Reset any visual state of the input socket if needed
                // For now, we just ensure it's visible and at normal scale
                if (inputSocket is Node2D inputNode)
                {
                    inputNode.Visible = true;
                    inputNode.Scale = Vector2.One;
                    GD.Print($"[BaseBlock Debug] Block {Name} reset input socket");
                }
            }
            
            if (outputSocket != null)
            {
                // Reset any visual state of the output socket if needed
                // For now, we just ensure it's visible and at normal scale
                if (outputSocket is Node2D outputNode)
                {
                    outputNode.Visible = true;
                    outputNode.Scale = Vector2.One;
                    GD.Print($"[BaseBlock Debug] Block {Name} reset output socket");
                }
            }
            GD.Print($"[BaseBlock Debug] Block {Name} finished resetting connections");
        }
    }
} 