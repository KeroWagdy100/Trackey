using System.Diagnostics.Tracing;
using System.Security.Cryptography.X509Certificates;
using LibVLCSharp.Shared;

namespace Trackey;

class AudioPlayer
{
    private LibVLC libvlc;
    private MediaPlayer player;
    private Media? currentMedia;

    public event EventHandler? TrackEnded;

    public AudioPlayer()
    {
        Core.Initialize();
        libvlc = new LibVLC();
        player = new MediaPlayer(libvlc);
        player.EndReached += (_, _) =>
        {
            TrackEnded?.Invoke(this, EventArgs.Empty);
        };
    }



    public int Volume => IsMuted ? 0 : player.Volume;
    public PlayerState State =>
        player.State switch
        {
            VLCState.Playing => PlayerState.PLAYING,
            VLCState.Paused => PlayerState.PAUSED,
            _ => PlayerState.NONE,
        };

    public bool IsPlaying => State == PlayerState.PLAYING;

    public enum PlayerState
    {
        NONE,
        PLAYING,
        PAUSED,
    }

    public void Play(string filename)
    {
        currentMedia?.Dispose();

        currentMedia = new Media(libvlc, filename, FromType.FromPath);
        player.Media = currentMedia;

        player.Play();
    }

    public long TimeMs => player.Time;
    public long DurationMs => player.Length;

    public void Stop() => player.Stop();
    public void Pause() => player.Pause();
    public void Resume() => player.SetPause(false);
    public void TogglePause()
    {
        if (player.IsPlaying) Pause();
        else Resume();
    }

    public void ToggleMute() => player.ToggleMute();
    public bool IsMuted => player.Mute;

    public void SetVolume(int volume) => player.Volume = Math.Max(Math.Min(volume, 100), 0);
    public void IncreaseVolume(int incBy) => SetVolume(Volume + incBy);
    public void DecreaseVolume(int decBy) => SetVolume(Volume - decBy);
}