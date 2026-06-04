using LibVLCSharp.Shared;

namespace Trackey;

class AudioPlayer
{
    /* Fields */
    private LibVLC _libvlc = null!;
    private MediaPlayer _player = null!;
    private Media? _currentMedia;

    /* Ctors */
    public AudioPlayer()
    {
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
        if (!OperatingSystem.IsWindows())
            Core.Initialize();
        else
        {
            string[] VlcPaths =
            [
                @"C:\Program Files\VideoLAN\VLC",
                @"C:\Program Files (x86)\VideoLAN\VLC"
            ];
            foreach (string path in VlcPaths)
            {
                if (Directory.Exists(path))
                {
                    Core.Initialize(path);
                    Logger.Log($"VLC found at '{path}'");
                    break;
                }
            }
        }

        _libvlc = new LibVLC();
        _player = new MediaPlayer(_libvlc);
        _player.EndReached += (_, _) =>
        {
            TrackEnded?.Invoke(this, EventArgs.Empty);
        };

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

    public void MoveForward(int milliseconds)
        => _player.Time = Math.Clamp(_player.Time + milliseconds, 0, _player.Length);
    public void MoveBackward(int milliseconds)
        => _player.Time = Math.Clamp(_player.Time - milliseconds, 0, _player.Length);

    /* Nested types */
    public enum PlayerState
    {
        NONE,
        PLAYING,
        PAUSED,
    }
}