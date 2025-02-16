using F.Game.Tokens;
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

        string IBlock.Name
        {
            get => _blockName;
            set => _blockName = value;
        }

        public BlockState State { get; set; } = BlockState.InToolbar;

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

        // Change the abstract method to a virtual method with a default implementation
        public virtual void ProcessToken(Token token)
        {
            throw new NotImplementedException("ProcessToken must be implemented in derived block classes.");
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