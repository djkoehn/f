using F.Game.Toolbar;
using F.Game.Core;
using HelperFunnel = F.Utils.HelperFunnel;

namespace F.Game.BlockLogic;

public partial class BlockInteractionManager : Node
{
    private GameManager? _gameManager;
    private HelperFunnel? _helperFunnel;  // HelperFunnel for other helpers

    public override void _Ready()
    {
        base._Ready();
        _gameManager = GetNode<GameManager>("/root/Main/GameManager");
        _helperFunnel = HelperFunnel.GetInstance();
        ProcessMode = ProcessModeEnum.Always;

        if (_gameManager == null) GD.PrintErr("Failed to find GameManager!");
        if (_helperFunnel == null) GD.PrintErr("Failed to find HelperFunnel!");
    }

    public override void _Process(double delta)
    {
        if (_gameManager?.ConnectionManager == null) return;

        // Removing hover effects related to dragging as dragging is now removed
        _gameManager.ConnectionManager.SetHoveredPipe(null);
    }

    // Public method to get a block at a given position
    public BaseBlock? GetBlockAtPosition(Vector2 position)
    {
        BaseBlock? closestBlock = null;
        float closestDistance = 50.0f; // picking threshold in pixels
        foreach (Node node in GetTree().GetNodesInGroup("Blocks"))
        {
            if (node is BaseBlock block)
            {
                float distance = block.GlobalPosition.DistanceTo(position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBlock = block;
                }
            }
        }
        return closestBlock;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.IsReleased() && (int)mouseEvent.ButtonIndex == (int)MouseButton.Left)
        {
            var gameManager = GetNode<GameManager>("/root/Main/GameManager");
            if (gameManager?.ConnectionManager != null)
            {
                Vector2 mousePos = GetViewport().GetMousePosition();
                BaseBlock? block = GetBlockAtPosition(mousePos);
                if (block != null && block.State == BlockState.Dragging)
                {
                    bool result = gameManager.ConnectionManager.HandleBlockConnection(block, mousePos);
                    if (result)
                    {
                        GD.Print($"[BlockInteractionManager Debug] Block {block.Name} connection established. State: {block.State}");
                    }
                    else
                    {
                        GD.Print($"[BlockInteractionManager Debug] Block {block.Name} connection attempt failed.");
                    }
                }
            }
        }
    }
}