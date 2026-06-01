namespace Trackey;

class TrackListScreen : TableViewScreen<Track>
{

    public Action<IEnumerable<Track>>? OnSubmit {get; set;}
    public override bool MultiSelect => true;

    public TrackListScreen(Application app) : base(app)
    {

    }

    public TrackListScreen(Application app, IEnumerable<Track> tracks) : base(app)
    {
        Items.AddRange(tracks);
    }

    public void AddTrack(Track track)
    {
        Items.Add(track);
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
        else if (key.Key == ConsoleKey.Q && key.Modifiers.HasFlag(ConsoleModifiers.Shift))
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
        else if (key.Key == ConsoleKey.A && key.Modifiers.HasFlag(ConsoleModifiers.Shift))
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

        else
            base.HandleInput(key);
    }

}