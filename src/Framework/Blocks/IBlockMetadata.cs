namespace F.Framework.Blocks;

public interface IBlockMetadata
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string Scene { get; }
    bool SpawnOnSpace { get; }
    bool IsToolbarBlock { get; }
    bool IsTemporary { get; }
    bool IsStartBlock { get; }
    bool IsEndBlock { get; }
    bool HasInput { get; }
    bool HasOutput { get; }
}