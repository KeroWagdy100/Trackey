using Spectre.Console;
using Spectre.Console.Rendering;

namespace Trackey;

class PlaylistView : TableViewScreen<Track>
{
    private Guid playlistId;
    public override bool MultiSelect => true;

    public PlaylistView(Application app, Guid playlistId) : base(app)
    {
        this.playlistId = playlistId;
        ReloadPlaylist();
    }

    public override void HandleInput(ConsoleKeyInfo key)
    {
        // Add selected tracks to queue
        if (key.Key == ConsoleKey.P && key.Modifiers.HasFlag(ConsoleModifiers.Shift))
        {
            foreach (var i in SelectedIndices)
                app.Queue.AddTrack(Items[i].Id);
        }
        
        // Add hovered track to queue
        else if (key.Key == ConsoleKey.P)
        {
            app.Queue.AddTrack(Items[hoveredIndex].Id);
        }

        // Remove selected tracks from playlist
        else if (key.Key == ConsoleKey.R && key.Modifiers.HasFlag(ConsoleModifiers.Shift))
        {
        }

        // Remove hovered track from playlist
        else if (key.Key == ConsoleKey.R)
        {

        }

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