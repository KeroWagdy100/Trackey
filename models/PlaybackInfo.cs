namespace Trackey;

record PlaybackInfo
(
    AudioPlayer.PlayerState playerState,
    int volume,
    Track? currentTrack,
    string? username,
    bool PlaybackControlsUnlocked,
    long CurrentTimeMs,
    long DurationMs
);