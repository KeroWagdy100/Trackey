using System.Diagnostics.Tracing;
using LibVLCSharp.Shared;

namespace Trackey;

class AudioPlayer
{
    private LibVLC libvlc;
    private MediaPlayer player;
    private Media? currentMedia;
    
    public AudioPlayer()
    {
        Core.Initialize();
        libvlc = new LibVLC();
        player = new MediaPlayer(libvlc);
    }


    public int Volume => player.Volume;
    public PlaybackState State =>
        player.State switch
        {
            VLCState.Playing => PlaybackState.PLAYING,
            VLCState.Paused => PlaybackState.PAUSED,
            _ => PlaybackState.NONE,
        };

    public bool IsPlaying => State == PlaybackState.PLAYING;

    public enum PlaybackState
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

    public void Stop() => player.Stop();
    public void Pause() => player.Pause();
    public void Resume() => player.Pause();
    public void TogglePause()
    {
        if (player.IsPlaying) Pause();
        else                  Resume();
    }

    public void SetVolume(int volume)     => player.Volume = Math.Max(Math.Min(volume, 100), 0);
    public void IncreaseVolume(int incBy) => SetVolume(Volume + incBy);
    public void DecreaseVolume(int decBy) => SetVolume(Volume - decBy);
}