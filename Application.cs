using LibVLCSharp.Shared;

namespace Trackey;

class Application
{
    public const int MX_TRIALS = 3; // TODO: REMOVE THIS LATER

    public Library Lib { get; }
    public AudioPlayer Player { get; }
    public QueueManager Queue { get; }

    /* Application State */
    public User? CurrUser { get; set; }
    public Guid? CurrTrackId { get; set; }
    public bool IsPlaying => Player.IsPlaying;
    public bool IsRunning { get; set; } = true; // if false, app quits


    public Track? CurrTrack => CurrTrackId is Guid id ? Lib.GetTrack(id) : null;

    public Application()
    {
        Player = new();
        Queue = new();
        Lib = new();
    }

    public void Demo()
    {
        Track track1 = new() {FileLocation = "./music/file1.mp3", Title = "Khaleek Fakerny", Artist = "Amr Diab"};
        Track track2 = new() {FileLocation = "./music/file2.mp3", Title = "Khaleek Fakerny", Artist = "Amr Diab"};
        Lib.AddTrack(track1);
        Lib.AddTrack(track2);

        Playlist pop = new();
        pop.AddTrack(track1.Id).AddTrack(track2.Id);
        Lib.AddPlaylist(pop);

        Queue.AddPlaylist(pop);
    }

    public void Run()
    {
        Demo();
        if (Queue.HasNext())
            UpdateCurrTrack(Queue.Next());

        while (IsRunning)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key;
                key = Console.ReadKey(true);
                HandleKey(key);
            }

            Console.Clear();
            Draw();
            Thread.Sleep(10);
        }
        Console.Clear();
    }

    public void HandleKey(ConsoleKeyInfo key)
    {
        if (key.KeyChar == 'Q')                     Quit();
        else if (key.KeyChar == '+')                Player.IncreaseVolume(5);
        else if (key.KeyChar == '-')                Player.DecreaseVolume(5);
        else if (key.Key == ConsoleKey.Spacebar)    Player.TogglePause();

        else if (key.Key == ConsoleKey.RightArrow)
        {
            if (Queue.HasNext())
                UpdateCurrTrack(Queue.Next());
        }
        else if (key.Key == ConsoleKey.LeftArrow)
            if (Queue.HasPrev())
                UpdateCurrTrack(Queue.Prev());
    }

    private void UpdateCurrTrack(Guid trackId)
    {
        // TODO: HANDLE EDGE CASES
        // e.g. Track not initialized in library
        CurrTrackId = trackId;
        Player.Play(CurrTrack!.FileLocation);
    }

    public void Draw()
    {
        if (Player.State == AudioPlayer.PlaybackState.NONE)
        {
            Console.WriteLine("Not Playing anything now");
            return;
        }

        char stateChar = Player.State == AudioPlayer.PlaybackState.PLAYING ? '⏸' : '►';

        Console.WriteLine($"({stateChar}): {CurrTrack.Title} | {CurrTrack.Artist}");
        Console.WriteLine($"Volume: {Player.Volume}");
    }

    private void Quit() => IsRunning = false;
}