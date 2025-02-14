namespace F.Audio;

public partial class BlockSoundPlayer : Node
{
    private AudioStreamPlayer? _connectPlayer;
    private AudioStreamPlayer? _hitPlayer;

    public override void _Ready()
    {
        _connectPlayer = new AudioStreamPlayer
        {
            Stream = GD.Load<AudioStream>("res://assets/audio/BlockConnect.wav"),
            Name = "BlockConnectPlayer"
        };
        AddChild(_connectPlayer);

        _hitPlayer = new AudioStreamPlayer
        {
            Stream = GD.Load<AudioStream>("res://assets/audio/BlockHit.wav"),
            Name = "BlockHitPlayer"
        };
        AddChild(_hitPlayer);
    }

    public void PlayConnect()
    {
        _connectPlayer?.Play();
    }

    public void PlayHit()
    {
        _hitPlayer?.Play();
    }
}