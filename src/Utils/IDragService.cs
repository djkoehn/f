namespace F.Utils
{
    public interface IDragService
    {
        // Initiates dragging for a given block using the click position
        void StartDrag(BaseBlock block, Vector2 clickPosition);

        // Updates the block's position during the drag
        void UpdateDrag(BaseBlock block, Vector2 position);

        // Ends the drag operation for the block
        void EndDrag(BaseBlock block);
    }
} 