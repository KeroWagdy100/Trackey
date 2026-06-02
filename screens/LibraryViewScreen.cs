
using System.Collections;
using System.Threading.Tasks;

namespace Trackey;

class LibraryViewScreen : TableViewScreen<Playlist>
{
    public LibraryViewScreen(Application app) : base(app)
    {
        ReloadItems();
    }

    public void ReloadItems()
    {
        Items.Clear();
        AddItems(app.Lib.AllPlaylists);
    }

    public override IEnumerable<Shortcut> Shortcuts => [
        new Shortcut() {
            Description = "View hovered playlist",
            Combo = new KeyCombo(ConsoleKey.Enter)
        },
        new Shortcut() {
            Description = "Create [N]ew Playlist",
            Combo = new KeyCombo(ConsoleKey.N)
        },
        new Shortcut() {
            Description = "[R]ename hovered playlist",
            Combo = new KeyCombo(ConsoleKey.R)
        },
        new Shortcut() {
            Description = "[A]dd tracks to hovered playlist",
            Combo = new KeyCombo(ConsoleKey.A)
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
        // Create [N]ew playlist
        if (key.Key == ConsoleKey.N)
        {
            Logger.Log("Creating playlist");
            app.SetActivePrompt(new Prompt() {
                Ask = "Playlist Title",
                Input = new InputText() {
                    OnSubmit = title => {
                        app.CreatePlaylist(title);
                        app.RemoveActivePrompt();
                        ReloadItems();
                    },
                    OnCancel = () => app.RemoveActivePrompt()}
            });
        }

        // [R]ename hovered playlist
        else if (key.Key == ConsoleKey.R)
        {
            Logger.Log("Renaming playlist");
            app.SetActivePrompt(new Prompt() {
                Ask = $"{Items[hoveredIndex].Title} => ",
                Input = new InputText() {
                    OnSubmit = newTitle => {
                        app.Lib.RenamePlaylist(Items[hoveredIndex].Id, newTitle);
                        app.RemoveActivePrompt();
                        ReloadItems();
                    },
                    OnCancel = () => app.RemoveActivePrompt()}
            });

        }

        // [A]dd tracks to hovered playlist => track list shown
        else if (key.Key == ConsoleKey.A)
        {
            var playlist = Items[hoveredIndex];
            var screen = new TrackListScreen(app) {
                Title = $"Add Tracks to Playlist {playlist.Title}",
                OnSubmit = selectedTracks =>
                {
                    app.Lib.AddTracksToPlaylist(playlist.Id, selectedTracks.Select(t => t.Id));
                    app.NavigateBack(false); // hmmmm
                    ReloadItems();
                }
            };

            var filteredTracks = app.Lib.AllTracks.Where(track => !playlist.TrackIds.Contains(track.Id));
            screen.AddItems(filteredTracks);

            app.NavigateTo(screen, true);
        }

        // Add selected playlists to [Q]ueue
        else if (key.Key == ConsoleKey.Q && key.Modifiers.HasFlag(ConsoleModifiers.Shift))
        {
            foreach (var i in SelectedIndices)
                app.Queue.Enqueue(Items[i]);
        }

        // Add hovered playlist to [Q]ueue
        else if (key.Key == ConsoleKey.Q)
        {
            app.Queue.Enqueue(Items[hoveredIndex]);
        }

        // View hovered playlist
        else if (key.Key == ConsoleKey.Enter)
        {
            app.NavigateTo(new TrackListScreen(app, Items[hoveredIndex].Id), true);
        }

        base.HandleInput(key);
    }

}