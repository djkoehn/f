namespace F.Audio;

public partial class TokenSoundPlayer : Node
{
    private AudioStreamPlayer? _startPlayer;
    private AudioStreamPlayer? _traversalCompletePlayer;

    public override void _Ready()
    {
        _startPlayer = new AudioStreamPlayer
        {
            Stream = GD.Load<AudioStream>("res://assets/audio/TokenStart.wav"),
            Name = "TokenStartPlayer"
        };
        AddChild(_startPlayer);

        _traversalCompletePlayer = new AudioStreamPlayer
        {
            Stream = GD.Load<AudioStream>("res://assets/audio/TraversalComplete.wav"),
            Name = "TraversalCompletePlayer"
        };
        AddChild(_traversalCompletePlayer);
    }

    public void PlayStart()
    {
        _startPlayer?.Play();
    }

    public void PlayTraversalComplete()
    {
        _traversalCompletePlayer?.Play();
    }
}