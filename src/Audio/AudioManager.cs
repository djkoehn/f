namespace F.Audio;

public partial class AudioManager : Node
{
    private BlockSoundPlayer? _blockSounds;
    private TokenSoundPlayer? _tokenSounds;
    public static AudioManager? Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;

        _blockSounds = GetNode<BlockSoundPlayer>("BlockSoundPlayer");
        _tokenSounds = GetNode<TokenSoundPlayer>("TokenSoundPlayer");

        if (_blockSounds == null || _tokenSounds == null) GD.PrintErr("Required sound players not found!");
    }

    public void PlayBlockConnect()
    {
        _blockSounds?.PlayConnect();
    }

    public void PlayBlockHit()
    {
        _blockSounds?.PlayHit();
    }

    public void PlayTokenStart()
    {
        _tokenSounds?.PlayStart();
    }

    public void PlayTraversalComplete()
    {
        _tokenSounds?.PlayTraversalComplete();
    }
}