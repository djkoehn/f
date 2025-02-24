namespace F.Framework.Input;

public static class InputActions
{
    public const string Interact = "interact";
    public const string ReturnBlock = "return_block";

    public static void ConfigureInputActions()
    {
        // Add "interact" action (left click)
        if (!InputMap.HasAction(Interact))
        {
            InputMap.AddAction(Interact);
            var mouseButtonEvent = new InputEventMouseButton
            {
                ButtonIndex = MouseButton.Left
            };
            InputMap.ActionAddEvent(Interact, mouseButtonEvent);
        }

        // Add "return_block" action (right click)
        if (!InputMap.HasAction(ReturnBlock))
        {
            InputMap.AddAction(ReturnBlock);
            var mouseButtonEvent = new InputEventMouseButton
            {
                ButtonIndex = MouseButton.Right
            };
            InputMap.ActionAddEvent(ReturnBlock, mouseButtonEvent);
        }
    }
}