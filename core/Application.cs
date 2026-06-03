using System.Collections.Specialized;
using System.Text;
using LibVLCSharp.Shared;
using Spectre.Console;
using YoutubeDLSharp.Metadata;

namespace Trackey;

class Application
{
    /* Constants */
    public const int TARGET_FPS = 60;

    /* Fields */
    private bool nextTrackRequested = false;
    private List<DownloadTaskInfo> ActiveDownloads = [];
    private Dictionary<Guid, Notification> ActiveNotifications = [];
    private Stack<Screen> BackScreens = new();
    private Stack<Screen> ForwardScreens = new();
    private Guid? playbackWarningNotificationId = null;

    /* Ctors */
    public Application()
    {
        NavigateTo(new HomeScreen(this), false);
    }

    /* Properties */
    // Services
    public Library Lib { get; } = new();
    public AudioPlayer Player { get; private set; } = new();
    public QueueManager Queue { get; } = new();
    public UserService Users { get; } = new();
    public DownloadService Downloader { get; } = new();
    // TODO: Create Download Manager that takes care of the download service

    /* Application State */
    public bool IsRunning { get; set; } = true;

    public Guid? CurrUserId { get; set; }
    public User? CurrUser =>
        CurrUserId is Guid id && Users.TryGetUser(id, out User? user) ? user
        : null;
    public bool LoggedIn => CurrUserId is not null;

    public Guid? CurrTrackId { get; set; }
    public Track? CurrTrack =>
        CurrTrackId is Guid id && Lib.TryGetTrack(id, out var track) ? track
        : null;
    public bool TrackExists => CurrTrackId is not null;


    // Ui
    private UI Ui { get; } = new();
    public Screen CurrentScreen { get; set; } = null!;
    public Prompt? ActivePrompt { get; set; } = null;
    public bool PlaybackControlsUnlocked { get; set; } = false;

    public static IEnumerable<Shortcut> GlobalShortcuts => [
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

    
    // --------------------------------------------------
    /* Methods */
    // --------------------------------------------------

    public async Task InitializeAsync()
    {
        // Force the console to process UTF-8
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;
        Paths.Init();
        Logger.Clear();

        try
        {
            Player.Init();
        }
        catch
        {
            Console.WriteLine(
                @"Trackey requires VLC Media Player.

                Install VLC:
                https://www.videolan.org/vlc/

                Then restart Trackey."
            );
            Quit();
        }

        AddNotification(Notification.Success($"VLC Found"));

        Player.TrackEnded += OnTrackEnded;

        
        Console.WriteLine("Looking for yt-dlp requirements...");
        await Downloader.Init();
        AddNotification(Notification.Success("Requirements found/downloaded"));

        var usersLoaded = await Users.LoadUsers();
        if (!usersLoaded.Success)
            AddNotification(Notification.Error(usersLoaded.ErrorMessage ?? "Failed to load users"));
        else
            AddNotification(Notification.Success("Loaded users successfully"));

        var libLoaded = await Lib.LoadLibrary();
        if (!libLoaded.Success)
            AddNotification(Notification.Error(usersLoaded.ErrorMessage ?? "Failed to load users"));
        else
            AddNotification(Notification.Success("Loaded library successfully"));

        Console.Clear();
    }
    public async Task FinalizeAsync()
    {
        var usersSaved = await Users.SaveUsers();
        if (!usersSaved.Success)
            AddNotification(Notification.Error(usersSaved.ErrorMessage ?? "Failed to save users"));
        else
            AddNotification(Notification.Success("Saved users successfully"));
        var libSaved = await Lib.SaveLibrary();
        if (!libSaved.Success)
            AddNotification(Notification.Error(libSaved.ErrorMessage ?? "Failed to save library"));
        else
            AddNotification(Notification.Success("Saved library successfully"));
    }


    public void Run()
    {
        AnsiConsole.Live(Ui.Layout)
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


    public void Update()
    {
        if (!TrackExists && !Queue.IsEmpty)
            OnTrackEnded(null, new());
        UpdateQueue();

        UpdateActiveDownloads();
        UpdateNotifications();

        Ui.Update(CurrentScreen,
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
            ActivePrompt,
            ActiveNotifications.Values
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
    public void UpdateActiveDownloads()
    {
        // Removes Active Downloads Completed before 4 seconds or more
        var now = DateTime.Now;
        ActiveDownloads.RemoveAll(
        t => t.CompletedAt is DateTime completed
        && (now - completed).TotalSeconds >= 4
        );
    }
    public void UpdateNotifications()
    {
        // Removes Notifications Completed before 4 seconds or more
        foreach (var n in ActiveNotifications.Where(n => n.Value.IsExpired()))
            ActiveNotifications.Remove(n.Key);

        if (playbackWarningNotificationId is Guid id && !ActiveNotifications.ContainsKey(id))
            playbackWarningNotificationId = null;
    }


    // Key Handlers
    public void HandleKey(ConsoleKeyInfo key)
    {
        // Logger.Log($"{key.KeyChar} pressed [{key.Key}]");
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
    public void HandlePlaybackShortcuts(ConsoleKeyInfo key)
    {
        if (key.KeyChar == '+') Player.IncreaseVolume(5);
        else if (key.KeyChar == '-') Player.DecreaseVolume(5);
        else if (key.Key == ConsoleKey.Spacebar) Player.TogglePause();
        else if (key.Key == ConsoleKey.M) Player.ToggleMute();
        else
        {
            if (playbackWarningNotificationId is not null)
                RemoveNotification(playbackWarningNotificationId.Value);
            playbackWarningNotificationId = AddNotification(Notification.Warning("press ; to lock playback controls", 2));
        }
    }
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


    // Screen Navigators
    public void NavigateTo(Screen nextScreen, bool saveHistory)
    {
        if (saveHistory && CurrentScreen != null)
            BackScreens.Push(CurrentScreen);

        Logger.Log($"{CurrentScreen?.GetType()} => {nextScreen.GetType()}, {(saveHistory ? "saving" : "not saving")}");

        CurrentScreen = nextScreen;

        ClearForwardScreens();
    }
    public void NavigateBack(bool saveHistory = true)
    {
        if (BackScreens.Count == 0)
            return;
        if (saveHistory)
            ForwardScreens.Push(CurrentScreen);
        CurrentScreen = BackScreens.Pop();
    }
    public void NavigateForward(bool saveHistory = true)
    {
        if (ForwardScreens.Count == 0)
            return;
        if (saveHistory)
            BackScreens.Push(CurrentScreen);
        CurrentScreen = ForwardScreens.Pop();
    }
    public void ClearBackScreens() => BackScreens.Clear();
    public void ClearForwardScreens() => ForwardScreens.Clear();


    // Queue Service
    // Returns the corresponding Track for each trackId from Queue.QueueItems
    public IEnumerable<QueueItem> GetQueueItems()
    {
        var items = Queue.QueueItems();
        foreach (var item in items)
        {
            if (Lib.TryGetTrack(item.Track.Id, out Track? track))
                yield return new QueueItem(track, item.Type);
        }
    }

    // Download Service
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

        var trackId = Guid.NewGuid();

        OperationResult<string> res = await Downloader.DownloadAudioAsync(
            url,
            taskInfo.UpdateProgress,
            new(),
            trackId.ToString("N")
        );

        taskInfo.CompletedAt = DateTime.Now;
        taskInfo.FilePath = res.Data;
        taskInfo.ErrorMessage = res.ErrorMessage;


        if (!res.Success)
        {
            AddNotification(Notification.Error($"Download failed: {UI.Sanitize(taskInfo.ErrorMessage??"")}"));
            return;
        }

        var track = new Track()
        {
            Id = trackId,
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


    // Active Prompt
    public void SetActivePrompt(Prompt prompt) => ActivePrompt = prompt;
    public void RemoveActivePrompt() => ActivePrompt = null;

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
    public void TogglePlaybackControls() => PlaybackControlsUnlocked ^= true;
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

    // Notifications
    public Guid AddNotification(Notification notification)
    {
        Guid guid = Guid.NewGuid();
        ActiveNotifications[guid] = notification;
        return guid;
    }
    public void RemoveNotification(Guid notificationId)
    {
        ActiveNotifications.Remove(notificationId);
    }

    // Library
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