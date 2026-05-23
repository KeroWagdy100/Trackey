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

    /* Application State */
    public User? CurrUser { get; set; }
    public Guid? CurrTrackId { get; set; }
    public bool IsPlaying => Player.IsPlaying;
    public bool IsRunning { get; set; } = true; // if false, app quits


    // Ui
    private Ui ui = new();

    public Track? CurrTrack =>
        CurrTrackId is Guid id && Lib.TryGetTrack(id, out var track) ? track
        : null;

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

                ui.Update(lastPressed, new PlaybackInfo(Player.State, Player.Volume, CurrTrack));
                lastPressed = null; // reset

                ctx.Refresh();
                Thread.Sleep(1000 / TARGET_FPS);

            }

        });

        Console.Clear();
    }

    private ConsoleKeyInfo? lastPressed;

    public void HandleKey(ConsoleKeyInfo key)
    {
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
        {
            lastPressed = key;
            // Console.WriteLine("Hi");
        }
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