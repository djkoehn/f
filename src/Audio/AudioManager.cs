using Godot;

namespace F;

public partial class AudioManager : Node
{
    private static AudioManager? _instance;
    public static AudioManager? Instance => _instance;

    private AudioStreamPlayer? _blockConnectPlayer;
    private AudioStreamPlayer? _blockHitPlayer;
    private AudioStreamPlayer? _tokenStartPlayer;
    private AudioStreamPlayer? _traversalCompletePlayer;

    public override void _Ready()
    {
        if (_instance != null)
        {
            QueueFree();
            return;
        }
        _instance = this;

        // Initialize audio players
        _blockConnectPlayer = new AudioStreamPlayer
        {
            Stream = GD.Load<AudioStream>("res://assets/audio/BlockConnect.wav"),
            Name = "BlockConnectPlayer"
        };
        AddChild(_blockConnectPlayer);

        _blockHitPlayer = new AudioStreamPlayer
        {
            Stream = GD.Load<AudioStream>("res://assets/audio/BlockHit.wav"),
            Name = "BlockHitPlayer"
        };
        AddChild(_blockHitPlayer);

        _tokenStartPlayer = new AudioStreamPlayer
        {
            Stream = GD.Load<AudioStream>("res://assets/audio/TokenStart.wav"),
            Name = "TokenStartPlayer"
        };
        AddChild(_tokenStartPlayer);

        _traversalCompletePlayer = new AudioStreamPlayer
        {
            Stream = GD.Load<AudioStream>("res://assets/audio/TraversalComplete.wav"),
            Name = "TraversalCompletePlayer"
        };
        AddChild(_traversalCompletePlayer);
    }

    public override void _ExitTree()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public void PlayBlockConnect()
    {
        _blockConnectPlayer?.Play();
    }

    public void PlayBlockHit()
    {
        _blockHitPlayer?.Play();
    }

    public void PlayTokenStart()
    {
        _tokenStartPlayer?.Play();
    }

    public void PlayTraversalComplete()
    {
        _traversalCompletePlayer?.Play();
    }
} 