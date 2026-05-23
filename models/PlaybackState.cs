namespace Trackey;

struct PlaybackInfo
{
    public AudioPlayer.PlayerState playerState;
    public int volume;
    public Track? currentTrack;

    public PlaybackInfo(AudioPlayer.PlayerState playerState, int volume, Track? currentTrack)
    {
        this.playerState = playerState;
        this.volume = volume;
        this.currentTrack = currentTrack;
    }
}