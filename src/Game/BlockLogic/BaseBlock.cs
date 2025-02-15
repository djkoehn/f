using Godot;
using F.Config;
using F.Game.Tokens;
using F.Game.BlockLogic; // for IBlock, if needed
using System;

namespace F.Game.BlockLogic
{
    // New enum to represent the state of a block
    public enum BlockState
    {
        InToolbar,
        Dragging,
        Placed,
        Connected
    }

    // Marking the class as partial and implementing IBlock
    public partial class BaseBlock : Node2D, IBlock
    {
        // Explicit interface implementation for IBlock.Name to avoid conflict with Node.Name
        private string _blockName = "";
        string IBlock.Name
        {
            get => _blockName;
            set => _blockName = value;
        }

        public BlockState State { get; set; } = BlockState.Placed;

        [Signal]
        public delegate void BlockClickedEventHandler(BaseBlock block);

        public static class BlockSignals {
            public const string BlockClicked = "block_clicked";
        }

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
            return false;
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
            if (isDragging)
            {
                State = BlockState.Dragging;
            }
            else
            {
                if (State == BlockState.Dragging)
                    State = BlockState.Placed;
            }
        }

        public void SetPlaced(bool isPlaced)
        {
            if (isPlaced)
                State = BlockState.Placed;
            else
            {
                if (State == BlockState.Placed)
                    State = BlockState.Dragging;
            }
        }

        public void CompleteConnection()
        {
            State = BlockState.Connected;
            GD.Print($"[BaseBlock Debug] Block {((IBlock)this).Name} set to Connected. Current state: {State}");
        }

        public void SetInToolbar(bool value)
        {
            if (value)
                State = BlockState.InToolbar;
            else if (State == BlockState.InToolbar)
                State = BlockState.Placed;
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

        public override void _Input(InputEvent @event)
        {
            // Intentionally empty to allow InputHelper to handle input globally.
        }
    }
} 