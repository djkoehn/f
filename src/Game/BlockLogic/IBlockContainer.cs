namespace F.Game.BlockLogic;

public interface IBlockContainer
{
    /// <summary>
    ///     Updates the positions of all blocks within the container.
    /// </summary>
    void UpdateBlockPositions();

    /// <summary>
    ///     Gets the next available position for a block in the container.
    /// </summary>
    /// <returns>The next available position in the container's coordinate space.</returns>
    Vector2 GetNextBlockPosition();
}