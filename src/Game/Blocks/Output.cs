using F.Audio;
using F.Game.Tokens;

namespace F.Game.Blocks;

public partial class Output : BaseBlock
{
    private float _currentValue;
    private Label? _valueLabel;

    public override void _Ready()
    {
        base._Ready();
        _valueLabel = GetNode<Label>("Value");
        UpdateValueDisplay();
    }

    public override void ProcessToken(Token token)
    {
        if (token == null) return;

        _currentValue = token.Value;
        UpdateValueDisplay();
        AudioManager.Instance?.PlayTraversalComplete();
        token.QueueFree();
    }

    private void UpdateValueDisplay()
    {
        if (_valueLabel != null) _valueLabel.Text = _currentValue.ToString("F1");
    }
}