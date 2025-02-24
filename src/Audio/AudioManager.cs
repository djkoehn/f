using F.Framework.Logging;
using Godot;

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

        _blockSounds = GetNode<BlockSoundPlayer>("BlockSounds");
        _tokenSounds = GetNode<TokenSoundPlayer>("TokenSounds");

        if (_blockSounds == null || _tokenSounds == null)
        {
            Logger.Game.Err("Required sound players not found!");
            return;
        }
    }

    public void PlayBlockConnect()
    {
        _blockSounds?.PlayConnect();
        Logger.Game.Print("Playing block connect sound");
    }

    public void PlayBlockHit()
    {
        _blockSounds?.PlayHit();
        Logger.Game.Print("Playing block hit sound");
    }

    public void PlayTokenStart()
    {
        _tokenSounds?.PlayStart();
        Logger.Game.Print("Playing token start sound");
    }

    public void PlayTokenComplete()
    {
        _tokenSounds?.PlayComplete();
        Logger.Game.Print("Playing token complete sound");
    }
}