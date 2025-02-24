using Chickensoft.GodotNodeInterfaces;

namespace F.Framework.Input.Interfaces;

public interface IInputManager : INode
{
    void HandleInput(InputEvent @event);
    void HandleMouseMotion(InputEventMouseMotion mouseMotion);
    void HandleLeftClick(InputEventMouseButton mouseEvent);
    void HandleRightClick(InputEventMouseButton mouseEvent);
    void HandleSpacePress();
}