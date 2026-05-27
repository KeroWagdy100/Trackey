using LibVLCSharp.Shared;
using Spectre.Console;

namespace Trackey;

class Application
{
    public const int MX_TRIALS = 3; // TODO: REMOVE THIS LATER
    public const int TARGET_FPS = 30;

    public Library Lib { get; } = new();
    public AudioPlayer Player { get; } = new();
    public QueueManager Queue { get; } = new();
    public UserService Users { get; } = new();

    /* Application State */
    public Guid? CurrUserId { get; set; }
    public Guid? CurrTrackId { get; set; }
    public bool IsPlaying => Player.IsPlaying;
    public bool IsRunning { get; set; } = true; // if false, app quits
    public bool PlaybackControlsUnlocked { get; set; } = false;

    public Track? CurrTrack =>
        CurrTrackId is Guid id && Lib.TryGetTrack(id, out var track) ? track
        : null;

    public User? CurrUser =>
        CurrUserId is Guid id && Users.GetUser(id, out User? user) ? user
        : null;

    // Ui
    private Ui ui = new();
    public Screen CurrentScreen { get; set; }

    public Application()
    {
        CurrentScreen = new HomeScreen(this);
        Users.LoadUsers();
    }

    public void Demo()
    {
        Track track1 = new() { FileLocation = "./music/file1.mp3", Title = "Khaleek Fakerny", Artist = "Amr Diab" };
        Track track2 = new() { FileLocation = "./music/file2.mp3", Title = "Khaleek Ma3aya", Artist = "Amr Diab" };
        Lib.AddTrack(track1);
        Lib.AddTrack(track2);

        Playlist pop = new();
        pop.AddTrack(track1.Id).AddTrack(track2.Id);
        Lib.AddPlaylist(pop);

        Queue.AddPlaylist(pop);
    }

    public void Run()
    {
        Console.Clear();
        Logger.Clear();

        Demo();
        if (!Queue.IsEmpty)
            SetCurrentTrack(Queue.Next());

        AnsiConsole.Live(ui.Layout)
        .Start(ctx =>
        {

            while (IsRunning)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key;
                    key = Console.ReadKey(true);
                    HandleKey(key);
                }

                ui.Update(CurrentScreen,
                    new PlaybackInfo(
                        Player.State,
                        Player.Volume,
                        CurrTrack,
                        CurrUser?.Username,
                        PlaybackControlsUnlocked,
                        Player.TimeMs,
                        Player.DurationMs
                        ),
                        GetQueueTracksAsString()
                    );


                ctx.Refresh();
                Thread.Sleep(1000 / TARGET_FPS);
            }

        });

        Logger.Log(Player.TimeMs.ToString());
        Users.SaveUsers();
    }

    public List<string> GetQueueTracksAsString()
    {
        List<string> tracks = [];
        foreach (var trackId in Queue.Tracks)
        {
            if (Lib.TryGetTrack(trackId, out Track? track))
                tracks.Add(track.Title);

        }
        return tracks;
    }

    public void NavigateTo(Screen screen)
    {
        // TODO: Stack<Screen> 
        CurrentScreen = screen;
    }

    public void HandleKey(ConsoleKeyInfo key)
    {
        /*
        Global shortcuts (Always working): 
        - Ctrl+Q ==> Quit App
        - Escape ==> Cancel
        - char'<' ==> Navigate to previous screen (to be implemented soon) 
        - char'>' ==> Navigate to next screen (to be implemented soon) 
        - char'{' ==> Queue Previous 
        - char'}' ==> Queue Next
        Playback controls (locked/unlocked using ';'): 
        - char '+'/'-' ==> increase/decrease volume 
        - Space ==> Toggle Pause
        */
        Logger.Log(
            $"Key={key.Key}, Char={key.KeyChar}, Mods={key.Modifiers}"
        );


        if (key.KeyChar == ';')
        {
            TogglePlaybackControls();
            return;
        }

        if (HandleGlobalShortcuts(key))
            return;

        if (PlaybackControlsUnlocked)
            HandlePlaybackShortcuts(key);
        else
            CurrentScreen.HandleInput(key);
    }

    public void TogglePlaybackControls()
    {
        PlaybackControlsUnlocked ^= true;
    }

    public void HandlePlaybackShortcuts(ConsoleKeyInfo key)
    {
        if (key.KeyChar == '+') Player.IncreaseVolume(5);
        else if (key.KeyChar == '-') Player.DecreaseVolume(5);
        else if (key.Key == ConsoleKey.Spacebar) Player.TogglePause();
    }


    public bool HandleGlobalShortcuts(ConsoleKeyInfo key)
    {
        bool ctrl = key.Modifiers.HasFlag(ConsoleModifiers.Control);

        if (ctrl && key.Key == ConsoleKey.Q) Quit();
        else if (key.Key == ConsoleKey.Escape)
        {
            // Cancel Current Operation
        }

        else if (key.KeyChar == '}')
        {
            if (!Queue.IsEmpty)
                SetCurrentTrack(Queue.Next());
        }
        else if (key.KeyChar == '{')
        {
            if (!Queue.IsEmpty)
                SetCurrentTrack(Queue.Prev());
        }
        else if (key.KeyChar == '>')
        {
            // next screen
        }
        else if (key.KeyChar == '<')
        {
            // prev screen
        }
        else
            return false;

        return true;
    }

    public bool HandleAllShortcuts(ConsoleKeyInfo key)
    {
        if (HandleGlobalShortcuts(key))
            return true;

        if (key.KeyChar == 'Q') Quit();
        else if (key.KeyChar == '+') Player.IncreaseVolume(5);
        else if (key.KeyChar == '-') Player.DecreaseVolume(5);
        else if (key.Key == ConsoleKey.Spacebar) Player.TogglePause();

        else if (key.Key == ConsoleKey.RightArrow)
        {
            if (!Queue.IsEmpty)
                SetCurrentTrack(Queue.Next());
        }
        else if (key.Key == ConsoleKey.LeftArrow)
        {
            if (!Queue.IsEmpty)
                SetCurrentTrack(Queue.Prev());
        }
        else
            return false;

        return true;
    }

    public void SetCurrentUser(Guid userId)
    {
        CurrUserId = userId;
    }

    private void SetCurrentTrack(Guid trackId)
    {
        // TODO: HANDLE EDGE CASES
        // e.g. Track not initialized in library
        CurrTrackId = trackId;
        Player.Play(CurrTrack!.FileLocation);
    }

    private void Quit() => IsRunning = false;
}