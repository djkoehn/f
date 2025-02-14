namespace F.UI.Input;

using F.UI.Toolbar;
using F.Audio;

public partial class BlockConnectionHandler : Node
{
    private GameManager? _gameManager;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/Main/GameManager");
    }

    public bool TryConnectBlock(BaseBlock block, Vector2 position)
    {
        if (_gameManager?.ConnectionManager == null) return false;

        var connected = _gameManager.ConnectionManager.HandleBlockConnection(block, position);
        if (connected)
        {
            block.GetParent()?.RemoveChild(block);
            _gameManager.ConnectionManager.AddChild(block);
            AudioManager.Instance?.PlayBlockConnect();
            // NEW: Ensure the block is no longer dragged after connection.
            block.SetDragging(false);
            block.SetPlaced(true);
        }
        return connected;
    }

    public void HighlightPipeAtPosition(Vector2 position)
    {
        if (_gameManager?.ConnectionManager == null) return;

        _gameManager.ConnectionManager.ClearAllHighlights();
        var pipe = _gameManager.ConnectionManager.GetPipeAtPosition(position);
        if (pipe != null) pipe.SetHighlighted(true);
    }
}