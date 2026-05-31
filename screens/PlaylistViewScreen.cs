using Spectre.Console;
using Spectre.Console.Rendering;

namespace Trackey;

class PlaylistViewScreen : TrackListScreen
{
    private Guid playlistId;

    public PlaylistViewScreen(Application app, Guid playlistId) : base(app)
    {
        this.playlistId = playlistId;
        ReloadPlaylist();
    }

    public override void HandleInput(ConsoleKeyInfo key)
    {
        // Remove selected tracks from the playlist
        if (key.Key == ConsoleKey.R && key.Modifiers.HasFlag(ConsoleModifiers.Shift))
        {
        }

        // Remove hovered track from the playlist
        else if (key.Key == ConsoleKey.R)
        {
        }

        else
            base.HandleInput(key);
    }

    private void ReloadPlaylist()
    {
        Items.Clear();

        if (!app.Lib.TryGetPlaylist(playlistId, out var playlist))
            throw new ArgumentException($"Playlist with id {playlistId} not found");

        foreach (var trackId in playlist.TrackIds)
        {
            if (!app.Lib.TryGetTrack(trackId, out var track))
                throw new ArgumentException($"Track with id {trackId} not found");
            Items.Add(track);
        }
    }
}