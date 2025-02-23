using Friflo.Engine.ECS;
using Godot;
using F.Game.BlockLogic;
using F.Game.Connections;
using F.Game.Tokens;
using System.Collections.Generic;

namespace F.ECS.Components;

// Holds the token's current value
public struct TokenValueComponent : IComponent
{
    public float Value;
}

// Tracks which blocks the token is connected to
public struct TokenBlockComponent : IComponent
{
    public IBlock? CurrentBlock;
    public IBlock? TargetBlock;
    public HashSet<IBlock> ProcessedBlocks;

    public TokenBlockComponent()
    {
        CurrentBlock = null;
        TargetBlock = null;
        ProcessedBlocks = new HashSet<IBlock>();
    }
}

// Handles token movement state
public struct TokenMovementComponent : IComponent
{
    public Vector2 Position;
    public Vector2 TargetPosition;
    public bool IsMoving;
    public ConnectionPipe? CurrentPipe;
}

// Links the ECS entity to its visual representation
public struct TokenVisualComponent : IComponent
{
    public Token VisualToken;
} 