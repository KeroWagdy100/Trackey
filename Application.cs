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

    public void Run()
    {
        Guid track1Id = Lib.CreateTrack("./music/file1.mp3", "Khaleek Fakerny", "Amr Diab");
        Guid track2Id = Lib.CreateTrack("./music/file2.mp3", "Khaleek Ma3aya", "Amr Diab");
        Playlist amoora = new Playlist {Title = "Amoora", Tracks = {track1Id, track2Id}};
        Queue.AddPlaylist(amoora);
        // Queue.AddTrack(track1Id);
        // Queue.AddTrack(track2Id);

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
        if (key.KeyChar == 'Q')
            Quit();
        else if (key.KeyChar == '+')
            Player.IncreaseVolume(5);
        else if (key.KeyChar == '-')
            Player.DecreaseVolume(5);
        else if (key.Key == ConsoleKey.Spacebar)
            Player.TogglePause();
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
        if (!IsPlaying)
        {
            Console.WriteLine("Not Playing anything now");
            return;
        }

        string trackName = CurrTrack?.Title ?? "N/A";
        
        Console.WriteLine($"Now Playing: {trackName} ({Player.State()})");
        Console.WriteLine($"Volume: {Player.Volume}");
    }

    private void MainMenu()
    {
        // ! -> 
    }

    private void Quit() => IsRunning = false;
}