using F.Config.Visual;
using F.UI.Animations;
using F.UI.Toolbar;

// ADD THIS FOR DICTIONARY!

namespace F.UI.Input;

public partial class BlockToolbarHandler : Node
{
    private readonly Dictionary<BaseBlock, BlockReturn> _activeAnimations = new(); // TRACK ANIMATIONS!
    private GameManager? _gameManager;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/Main/GameManager");
    }

    public void ReturnBlockToToolbar(BaseBlock block)
    {
        if (_gameManager == null) return;

        // EXTRA SAFETY CHECKS!
        if (block == null || !IsInstanceValid(block)) return;

        // IMMEDIATELY STOP ANY DRAGGING STATE AND TELL INTERACTION MANAGER!
        block.SetDragging(false);
        block.SetPlaced(false);

        var interactionManager = GetNode<BlockInteractionManager>("/root/Main/GameManager/BlockInteractionManager");
        interactionManager?.SetDraggedBlock(null); // TELL MANAGER TO STOP DRAGGING!

        // CLEAR ANY EXISTING ANIMATIONS
        if (_activeAnimations.TryGetValue(block, out var oldAnim))
        {
            if (IsInstanceValid(oldAnim)) oldAnim.QueueFree();
            _activeAnimations.Remove(block);
        }

        // BREAK PIPE CONNECTIONS
        if (_gameManager.ConnectionManager != null) _gameManager.ConnectionManager.RemoveConnection(block);

        // GET TOOLBAR AND START POSITION
        var toolbar = _gameManager.GetNodeOrNull<ToolbarBlockContainer>("Toolbar/BlockContainer");
        if (toolbar != null)
        {
            // PREPARE SPACE AND GET TARGET
            toolbar.PrepareSpaceForBlock();
            var targetPos = toolbar.GetNextBlockPosition();

            // MAKE ANIMATION!
            var animation = BlockReturn.Create(block, block.GlobalPosition, targetPos);
            animation.ReturnCompleted += OnBlockReturnCompleted;
            AddChild(animation);
            _activeAnimations[block] = animation;
        }
    }

    private void OnBlockReturnCompleted(BaseBlock block)
    {
        // EXTRA SAFETY CHECKS!
        if (block == null || !IsInstanceValid(block)) return;

        // MAKE DOUBLE SURE DRAGGING STOPPED
        block.SetDragging(false);
        block.SetPlaced(false);

        // CLEAN UP OLD ANIMATION
        if (_activeAnimations.TryGetValue(block, out var oldAnim))
        {
            if (IsInstanceValid(oldAnim)) oldAnim.QueueFree();
            _activeAnimations.Remove(block);
        }

        // PUT BLOCK TO BED IN TOOLBAR
        var toolbar = _gameManager?.GetNodeOrNull<ToolbarBlockContainer>("Toolbar/BlockContainer");
        if (toolbar != null && IsInstanceValid(toolbar))
        {
            toolbar.AddBlockWithoutAnimation(block); // USE THIS TO AVOID MORE ANIMATIONS!
            block.ZIndex = ZIndexConfig.Layers.ToolbarBlock; // SET FINAL Z-INDEX
        }
    }

    public void SendBlockHome(BaseBlock block)
    {
        block.SetDragging(false);
        block.SetPlaced(false);
        ReturnBlockToToolbar(block);
    }

    public void ReturnDraggedBlockToToolbar(BaseBlock block)
    {
        ReturnBlockToToolbar(block);
    }

    public bool IsHoveringToolbar(Vector2 position)
    {
        if (_gameManager == null) return false;
        var toolbar = _gameManager.GetNodeOrNull<ToolbarBlockContainer>("Toolbar/BlockContainer");
        if (toolbar == null) return false;

        return toolbar.GetGlobalRect().HasPoint(position);
    }
}