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
    public Screen CurrentScreen { get; set; } = null!;
    public Prompt? ActivePrompt { get; set; } = null;
    public Stack<Screen> backScreens = new();
    public Stack<Screen> forwardScreens = new();
    public bool PlaybackControlsUnlocked { get; set; } = false;

    public async Task InitializeAsync()
    {
        await Users.LoadUsers();
        await Lib.LoadLibrary();

        // Adds all tracks in library directly to queue

        // var allTracks = Lib.AllTracksIds;
        // foreach (var id in allTracks)
        //     Queue.AddTrack(id);
    }

    public async Task FinalizeAsync()
    {
        await Users.SaveUsers();
        await Lib.SaveLibrary();
        Logger.Clear();
    }

    public Application()
    {
        NavigateTo(new HomeScreen(this), false);
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
        if (!TrackExists && !Queue.IsEmpty)
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
            ActiveDownloads,
            ActivePrompt
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

    public void PlayTrackNow(Guid trackId)
    {
        if (CurrTrackId == trackId)
            SetCurrentTrack(trackId);
        else
        {
            Queue.PlayNext(trackId);
            SetCurrentTrack(Queue.Next());
        }
    }


    public void SetActivePrompt(Prompt prompt) => ActivePrompt = prompt;
    public void RemoveActivePrompt() => ActivePrompt = null;

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

        Logger.Log($"{CurrentScreen?.GetType()} => {nextScreen.GetType()}, {(saveHistory ? "saving" : "not saving")}");

        CurrentScreen = nextScreen;

        ClearForwardScreens();
    }

    public void NavigateBack(bool saveHistory = true)
    {
        if (backScreens.Count == 0)
            return;
        if (saveHistory)
            forwardScreens.Push(CurrentScreen);
        CurrentScreen = backScreens.Pop();
    }

    public void NavigateForward(bool saveHistory = true)
    {
        if (forwardScreens.Count == 0)
            return;
        if (saveHistory)
            backScreens.Push(CurrentScreen);
        CurrentScreen = forwardScreens.Pop();
    }

    public void ClearBackScreens() => backScreens.Clear();
    public void ClearForwardScreens() => forwardScreens.Clear();


    // Key Handlers
    public void HandleKey(ConsoleKeyInfo key)
    {
        Logger.Log($"{key.KeyChar} pressed [{key.Key}]");
        if (key.KeyChar == ';')
        {
            TogglePlaybackControls();
            return;
        }

        if (HandleGlobalShortcuts(key))
            return;

        if (PlaybackControlsUnlocked)
            HandlePlaybackShortcuts(key);
        else if (ActivePrompt is not null)
            ActivePrompt.HandleInput(key);
        else
            CurrentScreen.HandleInput(key);
    }

    public void TogglePlaybackControls() => PlaybackControlsUnlocked ^= true;

    public void HandlePlaybackShortcuts(ConsoleKeyInfo key)
    {
        if (key.KeyChar == '+') Player.IncreaseVolume(5);
        else if (key.KeyChar == '-') Player.DecreaseVolume(5);
        else if (key.Key == ConsoleKey.Spacebar) Player.TogglePause();
        else if (key.Key == ConsoleKey.M) Player.ToggleMute();
    }

    public IEnumerable<Shortcut> GlobalShortcuts => [
        new Shortcut() {
            Description = "[Q]uit Application",
            Combo = new KeyCombo(ConsoleKey.Q, Ctrl: true)
        },
        new Shortcut() {
            Description = "Play next track in queue",
            Combo = new KeyCombo(ConsoleKey.Oem6, Shift: true, Char: '}')
        },
        new Shortcut() {
            Description = "Play previous track in queue",
            Combo = new KeyCombo(ConsoleKey.Oem4, Shift: true, Char: '{')
        },
        new Shortcut() {
            Description = "Navigate to previous screen",
            Combo = new KeyCombo(ConsoleKey.Oem4, Shift: true, Char: '<')
        },
        new Shortcut() {
            Description = "Show Shortcuts Guide Screen",
            Combo = new KeyCombo(ConsoleKey.G, Ctrl: true)
        },
    ];

    public bool HandleGlobalShortcuts(ConsoleKeyInfo key)
    {
        bool ctrl = key.Modifiers.HasFlag(ConsoleModifiers.Control);

        if (ctrl && key.Key == ConsoleKey.Q) Quit();

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
        else if (key.Key == ConsoleKey.G && ctrl && CurrentScreen is not GuideScreen)
            NavigateTo(new GuideScreen(this, CurrentScreen.Shortcuts), true);
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

    public async Task AddDownload(string url, string title, string artist)
    {
        var taskInfo = new DownloadTaskInfo()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Url = url,
            Artist = artist
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
        Queue.Enqueue(track.Id);
    }

    public async void CreatePlaylist(string playlistTitle)
    {
        var playlist = new Playlist() {
        Title = playlistTitle,
        Id = Guid.NewGuid(),
        OwnerUserId = CurrUserId is not null ? CurrUserId.Value : new Guid()
        };

        await Lib.AddPlaylist(playlist);
    }

    // Event Handlers
    public void OnTrackEnded(object? sender, EventArgs eventArgs)
    {
        nextTrackRequested = true;
        Logger.Log("Next Track Requested");
    }

    private void Quit() => IsRunning = false;
}