using LibVLCSharp.Shared;

namespace Trackey;

class AudioPlayer
{
    /* Fields */
    private readonly LibVLC _libvlc;
    private readonly MediaPlayer _player;
    private Media? _currentMedia;

    /* Ctors */
    public AudioPlayer()
    {
        // if (OperatingSystem.IsWindows())
        //     Core.Initialize(@"C:\Program Files\VideoLAN\VLC");
        // else
        Core.Initialize();

        _libvlc = new LibVLC();
        _player = new MediaPlayer(_libvlc);
        _player.EndReached += (_, _) =>
        {
            TrackEnded?.Invoke(this, EventArgs.Empty);
        };
    }

    /* Events */
    public event EventHandler? TrackEnded;

    /* Properties */
    public int Volume => IsMuted ? 0 : _player.Volume;
    public PlayerState State =>
        _player.State switch
        {
            VLCState.Playing => PlayerState.PLAYING,
            VLCState.Paused => PlayerState.PAUSED,
            _ => PlayerState.NONE,
        };
    public bool IsPlaying => State == PlayerState.PLAYING;
    public long TimeMs => _player.Time;
    public long DurationMs => _player.Length;
    public bool IsMuted => _player.Mute;


    /* Methods */
    public void Init()
    {

    }
    public void Play(string filename)
    {
        _currentMedia?.Dispose();

        _currentMedia = new Media(_libvlc, filename, FromType.FromPath);
        _player.Media = _currentMedia;

        Logger.Log($"Play {filename} ({File.Exists(filename)})");
        _player.Play();
    }
    public void Stop() => _player.Stop();

    public void Pause() => _player.Pause();
    public void Resume() => _player.SetPause(false);

    public void TogglePause()
    {
        if (_player.IsPlaying) Pause();
        else Resume();
    }
    public void ToggleMute() => _player.ToggleMute();

    public void SetVolume(int volume) => _player.Volume = Math.Max(Math.Min(volume, 100), 0);
    public void IncreaseVolume(int incBy) => SetVolume(Volume + incBy);
    public void DecreaseVolume(int decBy) => SetVolume(Volume - decBy);

    /* Nested types */
    public enum PlayerState
    {
        NONE,
        PLAYING,
        PAUSED,
    }
}