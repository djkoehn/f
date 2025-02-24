using Chickensoft.GodotNodeInterfaces;
using F.Framework.Core.Interfaces;
using Godot;

namespace F.Framework.Blocks.Interfaces;

public interface IBlockService : INode
{
    bool IsDragging { get; }
    BaseBlock? DraggedBlock { get; }

    BaseBlock? GetBlockAtPosition(Vector2 position);
    BaseBlock? CreateBlock(IBlockMetadata metadata, Node parent);
    void StartDrag(BaseBlock block, Vector2 position);
    void UpdateDrag(Vector2 position);
    void EndDrag();
    void SetHoveredBlock(BaseBlock? block);
    void ReturnBlockToToolbar(BaseBlock block);
}