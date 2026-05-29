using Spectre.Console;

namespace Trackey;

class Application
{
    public const int TARGET_FPS = 30;

    /* Services */
    public Library Lib { get; } = new();
    public AudioPlayer Player { get; } = new();
    public QueueManager Queue { get; } = new();
    public UserService Users { get; } = new();
    public DownloadService Downloader { get; } = new();
    // TODO: Create Download Manager that takes care of the download service

    /* Application State */
    public bool IsRunning { get; set; } = true;

    // Current User
    public Guid? CurrUserId { get; set; }
    public User? CurrUser =>
        CurrUserId is Guid id && Users.TryGetUser(id, out User? user) ? user
        : null;
    public bool LoggedIn => CurrUserId is not null;

    // Current Track
    public Guid? CurrTrackId { get; set; }
    public Track? CurrTrack =>
        CurrTrackId is Guid id && Lib.TryGetTrack(id, out var track) ? track
        : null;
    public bool TrackExists => CurrTrackId is not null;
    private bool nextTrackRequested = false;

    public List<DownloadTaskInfo> ActiveDownloads = [];

    // Ui
    private Ui ui = new();
    public Screen CurrentScreen { get; set; }
    public Stack<Screen> backScreens = new();
    public Stack<Screen> forwardScreens = new();
    public bool PlaybackControlsUnlocked { get; set; } = false;

    public async Task InitializeAsync()
    {
        await Users.LoadUsers();
        await Lib.LoadLibrary();

        // Adds all tracks in library directly to queue
        var allTracks = Lib.AllTracksIds;
        foreach (var id in allTracks)
            Queue.AddTrack(id);
    }

    public async Task FinalizeAsync()
    {
        await Users.SaveUsers();
        await Lib.SaveLibrary();
        Logger.Clear();
    }

    public Application()
    {
        CurrentScreen = new HomeScreen(this);
        Player.TrackEnded += OnTrackEnded;
    }

    public void Run()
    {
        AnsiConsole.Live(ui.Layout)
        .Start(ctx =>
        {

            while (IsRunning)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    HandleKey(key);
                }

                Update();

                ctx.Refresh();
                Thread.Sleep(1000 / TARGET_FPS);
            }

        });
    }

// --------------------------------------------------

    public void Update()
    {
        if (!TrackExists)
            OnTrackEnded(null, new());
        UpdateQueue();

        UpdateActiveDownloads();

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
            GetQueueItems(),
            ActiveDownloads
        );
    }

    public void UpdateQueue()
    {
        if (!nextTrackRequested || Queue.IsEmpty)
            return;

        nextTrackRequested = false;
        SetCurrentTrack(Queue.Next());

        Logger.Log("Next Track Invoked");
    }

    // Removes Active Downloads Completed before 4 seconds or more
    public void UpdateActiveDownloads()
    {
        var now = DateTime.Now;
        ActiveDownloads.RemoveAll(
        t => t.CompletedAt is DateTime completed
        && (now - completed).TotalSeconds >= 4
        );
    }

    public IEnumerable<QueueItem> GetQueueItems()
    {
        var items = Queue.QueueItems();
        foreach (var item in items)
        {
            if (Lib.TryGetTrack(item.Track.Id, out Track? track))
                yield return new QueueItem(track, item.Type);
        }
    }


    // Screen Navigators
    public void NavigateTo(Screen nextScreen, bool saveHistory)
    {
        if (saveHistory && CurrentScreen != null)
            backScreens.Push(CurrentScreen);

        CurrentScreen = nextScreen;

        forwardScreens.Clear();
    }

    public void NavigateBack()
    {
        if (backScreens.Count == 0)
            return;
        forwardScreens.Push(CurrentScreen);
        CurrentScreen = backScreens.Pop();
    }

    public void NavigateForward()
    {
        if (forwardScreens.Count == 0)
            return;
        backScreens.Push(CurrentScreen);
        CurrentScreen = forwardScreens.Pop();
    }


    // Key Handlers
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

    public void TogglePlaybackControls() => PlaybackControlsUnlocked ^= true;

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
            NavigateForward();
        else if (key.KeyChar == '<')
            NavigateBack();
        else
            return false;

        return true;
    }

    // Mutating State
    public void SetCurrentUser(Guid? userId)
    {
        CurrUserId = userId;
    }

    private void SetCurrentTrack(Guid trackId)
    {
        CurrTrackId = trackId;
        if (CurrTrack is not null)
            Player.Play(CurrTrack.Filepath);
    }

    public async Task AddDownload(string url)
    {
        var data = await Downloader.DownloadMetadataAsync(url);
        var taskInfo = new DownloadTaskInfo()
        {
            Id = Guid.NewGuid(),
            Title = data.Title,
            Url = url,
            Artist = data.Channel
        };

        ActiveDownloads.Add(taskInfo);

        var res = await Downloader.DownloadAudioAsync(url, taskInfo.UpdateProgress, new());
        taskInfo.CompletedAt = DateTime.Now;
        taskInfo.FilePath = res.Filepath;
        taskInfo.ErrorMessage = string.Join("\n", res.ErrorResult);

        var track = new Track()
        {
            Id = Guid.NewGuid(),
            OwnerUserId = CurrUserId!.Value,
            Title = taskInfo.Title,
            Artist = taskInfo.Artist,
            SourceUrl = taskInfo.Url,
            Filepath = taskInfo.FilePath!,
            DownloadedAt = taskInfo.CompletedAt.Value
        };

        await Lib.AddTrack(track);
        Queue.AddTrack(track.Id);
    }

    // Event Handlers
    public void OnTrackEnded(object? sender, EventArgs eventArgs)
    {
        nextTrackRequested = true;
        Logger.Log("Next Track Requested");
    }

    private void Quit() => IsRunning = false;
}