using Godot;

namespace F.UI;

public partial class TokenValueDisplay : Label
{
    private Token? _trackedToken;
    
    public override void _Process(double delta)
    {
        if (_trackedToken != null)
        {
            Text = $"Token Value: {_trackedToken.Value:F1}";
            GlobalPosition = _trackedToken.GlobalPosition + new Vector2(0, -30);
        }
    }

    public void TrackToken(Token token)
    {
        _trackedToken = token;
        Show();
    }

    public void StopTracking()
    {
        _trackedToken = null;
        Hide();
    }
}
