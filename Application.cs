using LibVLCSharp.Shared;

namespace Trackey;

class Application
{
    public const int MX_TRIALS = 3; // TODO: REMOVE THIS LATER
    public const int TARGET_FPS = 30;

    public Library Lib { get; }
    public AudioPlayer Player { get; }
    public QueueManager Queue { get; }

    /* Application State */
    public User? CurrUser { get; set; }
    public Guid? CurrTrackId { get; set; }
    public bool IsPlaying => Player.IsPlaying;
    public bool IsRunning { get; set; } = true; // if false, app quits


    public Track? CurrTrack() {
        if (CurrTrackId is Guid id && Lib.TryGetTrack(id, out var track))
            return track;
        return null;
    }

    public Application()
    {
        Player = new();
        Queue = new();
        Lib = new();
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

        Demo();
        if (Queue.CanNavigate)
            UpdateCurrTrack(Queue.Next());

        while (IsRunning)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key;
                key = Console.ReadKey(true);
                HandleKey(key);
            }

            Draw();

            Thread.Sleep(1000 / TARGET_FPS);
        }

        Console.Clear();
    }

    public void HandleKey(ConsoleKeyInfo key)
    {
        if (key.KeyChar == 'Q') Quit();
        else if (key.KeyChar == '+') Player.IncreaseVolume(5);
        else if (key.KeyChar == '-') Player.DecreaseVolume(5);
        else if (key.Key == ConsoleKey.Spacebar) Player.TogglePause();

        else if (key.Key == ConsoleKey.RightArrow)
        {
            if (Queue.CanNavigate)
                UpdateCurrTrack(Queue.Next());
        }
        else if (key.Key == ConsoleKey.LeftArrow)
            if (Queue.CanNavigate)
                UpdateCurrTrack(Queue.Prev());
    }

    private void UpdateCurrTrack(Guid trackId)
    {
        // TODO: HANDLE EDGE CASES
        // e.g. Track not initialized in library
        CurrTrackId = trackId;
        Player.Play(CurrTrack()!.FileLocation);
    }

    public void Draw()
    {
        Console.Clear();
        if (Player.State == AudioPlayer.PlaybackState.NONE)
        {
            Console.WriteLine("Not Playing anything now");
            return;
        }

        char stateChar = Player.State == AudioPlayer.PlaybackState.PLAYING ? '⏸' : '►';

        Console.WriteLine($"({stateChar}): {CurrTrack()?.Title} | {CurrTrack()?.Artist}");
        Console.WriteLine($"Volume: {Player.Volume}");
    }

    private void Quit() => IsRunning = false;
}