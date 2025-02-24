namespace F.Framework.Core;

public interface IBlockMetadata
{
    string Id { get; }
    string Scene { get; }
    string SpawnHotkey { get; }
    bool HasInput { get; }
    bool HasOutput { get; }
    bool IsToolbarBlock { get; }
}