namespace Trackey;

class TrackListScreen : TableViewScreen<Track>
{
    public Guid? PlaylistId;
    public Action<IEnumerable<Track>>? OnSubmit {get; set;}
    public override bool MultiSelect => true;

    public TrackListScreen(Application app) : base(app)
    {
        Title = "Tracks";
    }

    public TrackListScreen(Application app, Guid playlistId) : base(app)
    {
        PlaylistId = playlistId;
        ReloadPlaylist();
    }

    public TrackListScreen(Application app, IEnumerable<Track> tracks) : base(app)
    {
        AddItems(tracks);
        Title = "Tracks";
    }

    public void ReloadPlaylist()
    {
        if (PlaylistId is null) return;
        Items.Clear();
        AddItems(app.Lib.GetPlaylistTracks(PlaylistId.Value));
        Title = $"{app.Lib.GetPlaylistTitle(PlaylistId.Value)} - Playlist";
    }

    public override IEnumerable<Shortcut> Shortcuts => [
        new Shortcut() {
            Description = "Play hovered track now",
            Combo = new KeyCombo(ConsoleKey.P)
        },
        new Shortcut() {
            Description = "Add selected tracks to [Q]ueue",
            Combo = new KeyCombo(ConsoleKey.Q, Shift: true)
        },
        new Shortcut() {
            Description = "Add hovered track to [q]ueue",
            Combo = new KeyCombo(ConsoleKey.Q)
        },
    ];

    public override void HandleInput(ConsoleKeyInfo key)
    {
        if (IsSearching && SearchInputActive)
        {
            base.HandleInput(key);
            return;
        }
        bool shift = key.Modifiers.HasFlag(ConsoleModifiers.Shift);
        if (key.Key == ConsoleKey.Enter && OnSubmit is not null)
        {
            OnSubmit(SelectedIndices.Select(i => Items[i]));
        }

        // Play hovered track now
        if (key.Key == ConsoleKey.P)
        {
            app.PlayTrackNow(Items[hoveredIndex].Id);
        }

        // Add selected tracks to queue
        else if (key.Key == ConsoleKey.Q && shift)
        {
            foreach (var i in SelectedIndices)
                app.Queue.Enqueue(Items[i].Id);
        }

        // Add hovered track to queue
        else if (key.Key == ConsoleKey.Q)
        {
            app.Queue.Enqueue(Items[hoveredIndex].Id);
        }

        // Add selected tracks to some playlist
        else if (key.Key == ConsoleKey.A && shift)
        {

        }

        // Add hovered track to some playlist
        else if (key.Key == ConsoleKey.A)
        {

        }

        // Edit hovered track
        else if (key.Key == ConsoleKey.E)
        {

        }

        // [R]emove selected tracks from the playlist
        else if (PlaylistId is not null && key.Key == ConsoleKey.R && shift)
        {
            app.Lib.RemoveTracksFromPlaylist(
                PlaylistId.Value,
                SelectedIndices.Select(i => Items[i].Id)
            );
            ReloadPlaylist();
        }

        // [R]emove hovered track from the playlist
        else if (PlaylistId is not null && key.Key == ConsoleKey.R)
        {
            app.Lib.RemoveTracksFromPlaylist(
                PlaylistId.Value,
                [Items[hoveredIndex].Id]
            );
            ReloadPlaylist();
        }

        else
            base.HandleInput(key);
    }

}