namespace Trackey;

using Spectre.Console;
using Spectre.Console.Rendering;
using YoutubeDLSharp;

class Ui
{
    public static string Ellipsis(string s, int mxWidth)
    {
        if (s.Length > mxWidth)
            return $"{s.AsSpan(0, mxWidth)}..";
        return s;
    }

    public static string Sanitize(string s, int mxWidth = -1)
    {
        s = Markup.Escape(s);
        if (mxWidth != -1)
            return Ellipsis(s, mxWidth);
        return s;
    }

    public static string BuildProgressBar(double ratio, int width, char unfilledChar = '⎯', char filledChar = '⎯', string unfilledColor = "white", string filledColor = "green")
    {
        int filled = (int)(ratio * width);
        string line = 
        $"[{filledColor}]" 
        + new string(filledChar, filled) 
        + $"[/][{unfilledColor}]" 
        + new string(unfilledChar, width - filled) + "[/]";
        return line;
    }
    

// ---------------------------------------------------------------

    public Layout Layout { get; private set; }

    private Panel PlaybackPanel { get; set; }
    private Panel MainPanel { get; set; }
    private Panel QueuePanel { get; set; }
    private Panel DownloadsPanel { get; set; }
    private Panel ActivePromptPanel { get; set; }
    private Rows NotificationsRows { get; set; }

    public Ui()
    {
        PlaybackPanel = new("Nothing is playing")
        {
            Header = new PanelHeader($"Trackey", Justify.Center),
            Expand = true
        };

        MainPanel = new("")
        {
            Header = new PanelHeader("Main Screen", Justify.Center),
            Expand = true
        };

        QueuePanel = new("")
        {
            Header = new PanelHeader("Queue", Justify.Center),
            Expand = true
        };

        DownloadsPanel = new("")
        {
            Header = new PanelHeader("Downloads", Justify.Center),
            Expand = true
        };

        ActivePromptPanel = new("")
        {
            Header = new PanelHeader("", Justify.Center),
            Expand = true,
            Border = BoxBorder.None
        };

        NotificationsRows = new();

        Layout = new Layout("Root").SplitRows(
            new Layout("MainRow"),
            new Layout("Playback").Size(5)
        );

        Layout["MainRow"].SplitColumns(
            new Layout("MainWindow").Ratio(3),
            new Layout("Queue").Ratio(1)
        );

        Layout["MainWindow"].SplitRows(
            new Layout("Main"),
            new Layout("Notifications").Size(3),
            new Layout("ActivePrompt").Size(3),
            new Layout("Downloads").Size(4)
        );

        Layout["Playback"].Update(PlaybackPanel);
        Layout["Main"].Update(MainPanel);
        Layout["Queue"].Update(QueuePanel);
        Layout["Downloads"].Update(DownloadsPanel);
        Layout["ActivePrompt"].Update(ActivePromptPanel);
        Layout["Notifications"].Update(NotificationsRows);
    }

    private string BuildPlaybackBar(long currentMs, long totalMs, int width)
    {
        double ratio = 0.0;
        if (totalMs > 0)
            ratio = (double)currentMs / totalMs;

        int currentSecond = (int)currentMs / 1000;
        int currentMinute = currentSecond / 60;
        currentSecond %= 60;

        int totalSecond = (int)totalMs / 1000;
        int totalMinute = totalSecond / 60;
        totalSecond %= 60;


        return
        currentMinute.ToString().PadLeft(2, '0') + ":" + currentSecond.ToString().PadLeft(2, '0')
        + " "
        + BuildProgressBar(ratio, width, '⎯', '⎯', "white", "green")
        + " "
        + totalMinute.ToString().PadLeft(2, '0') + ":" + totalSecond.ToString().PadLeft(2, '0');
    }

    public void UpdatePlaybackPanel(PlaybackInfo info)
    {
        bool currentlyPlaying = info.playerState == AudioPlayer.PlayerState.PLAYING;
        
        string mode = new (' ', 13);
        if (!info.PlaybackControlsUnlocked)
            mode = "[gray][[[bold];[/] to unlock]][/]";

        string volume = $"🔊 {info.volume,3}";
        if (info.volume == 0)
            volume = "🔇" + new string(' ', 4);
        

        string track =
        info.playerState == AudioPlayer.PlayerState.NONE ? "Not Playing anything now"
        : $"{Sanitize(info.currentTrack?.Title ?? "", 40)} | {Sanitize(info.currentTrack?.Artist ?? "", 20)}";


        string title = "[bold]";
        if (!string.IsNullOrEmpty(info.username))
            title += Sanitize(info.username);
        else
            title += $"Trackey";
        title += "[/]";

        Columns trackColumn = new Columns(
        [
            // new Markup(mode).LeftJustified(),
            new Markup(track).Centered().Ellipsis(),
            // new Markup(volume).RightJustified(),
        ]
        ).Expand();

        Columns secondColumn = new Columns(
        [
            new Markup(mode).LeftJustified(),
            new Markup(volume).RightJustified(),
        ]
        ).Expand();

        // TODO: Make playback bar width dynamic (consider available width in current panel)
        int playbackBarWidth = 50;
        string playbackBar = BuildPlaybackBar(info.CurrentTimeMs, info.DurationMs, playbackBarWidth);

        var progress = new Align(
            new Markup(
                playbackBar),
            HorizontalAlignment.Center
        );
        Rows rows = new Rows(
            trackColumn,
            secondColumn,
            progress
        ).Expand();

        PlaybackPanel = new Panel(rows)
        {
            Header = new(title),
            Expand = true
        }.BorderColor(currentlyPlaying ? Color.Green : Color.Default);

    }

    public void UpdateMainPanel(Screen currentScreen)
    {
        MainPanel = new Panel(currentScreen.Render())
        {
            Header = new(currentScreen.Title, Justify.Center),
            Expand = true
        }.NoBorder();
    }

    public void UpdateQueuePanel(IEnumerable<QueueItem> queueItems)
    {
        QueueItem? lastPrev = null;
        List<QueueItem> visible = [];
        foreach (var item in queueItems)
        {
            if (item.Type == QueueItemType.PREVIOUS) lastPrev = item;
            else visible.Add(item);
        }

        List<Renderable> markups = [];
        if (lastPrev is not null)
            visible.Insert(0, lastPrev);
        else
            markups.Add(new Markup("\n"));

        foreach (var item in visible)
        {
            string color = "white";
            if (item.Type == QueueItemType.PREVIOUS)
                color = "gray";
            else if (item.Type == QueueItemType.CURRENT)
                color = "green";

            string title = Sanitize(item.Track.Title, 30);

            string text = $"[{color}]{(item.Type == QueueItemType.CURRENT ? "⇨ " : "")}{title}[/]\n";
            markups.Add(new Markup(text).Crop());
        }

        QueuePanel = new Panel(new Rows(markups))
        {
            Header = new("Queue", Justify.Center),
            Expand = true
        }.BorderColor(Color.Green);
    }

    public bool UpdateDownloadsPanel(List<DownloadTaskInfo> downloads, int width = 30)
    {
        List<Markup> rows = [];
        foreach (var d in downloads)
        {
            if (d.State == DownloadState.Success)
            {
                rows.Add(new Markup($"[green]✓ Downloaded: {Sanitize(d.Title)}[/]"));
                continue;
            }

            string line = Sanitize(d.Title, 15);

            line += 
            " " + BuildProgressBar(d.Progress.Progress, width, '░', '█', "gray", "blue")
            + " " + Math.Round(d.Progress.Progress * 100.0).ToString() + "%";

            rows.Add(new Markup(line));
        }

        DownloadsPanel = new Panel(new Rows(rows))
        {
            Header = new("Downloads", Justify.Left),
            Expand = true
        }.BorderColor(Color.Blue);

        return rows.Count > 0;
    }

    public void UpdateActivePrompt(Prompt prompt)
    {
        ActivePromptPanel = new Panel(prompt.Render())
        {
            // Header = new("", Justify.Right),
            Expand = true,
        }.BorderColor(Color.Yellow);
    }

    public void UpdateNotifications(IEnumerable<Notification> nots)
    {
        NotificationsRows = new Rows(nots.Select(n => new Markup(n.RenderedText())))
        {
            // Header = new("", Justify.Right),
            Expand = true,
        };
    }

    public void Update(
        Screen currentScreen,
        PlaybackInfo playbackInfo,
        IEnumerable<QueueItem> queueTracks,
        List<DownloadTaskInfo> downloads,
        Prompt? activePrompt,
        IEnumerable<Notification> notifications
        )
    {
        UpdatePlaybackPanel(playbackInfo);
        UpdateMainPanel(currentScreen);
        UpdateQueuePanel(queueTracks);
        Layout["Playback"].Update(PlaybackPanel);
        Layout["Main"].Update(MainPanel);
        Layout["Queue"].Update(QueuePanel);
        
        bool downloadExist = UpdateDownloadsPanel(downloads);
        if (downloadExist)
            Layout["Downloads"].Update(DownloadsPanel).Visible();
        else
            Layout["Downloads"].Invisible();
        
        if (activePrompt is not null)
        {
            UpdateActivePrompt(activePrompt);
            Layout["ActivePrompt"].Update(ActivePromptPanel).Visible();
        }
        else
            Layout["ActivePrompt"].Invisible();

        if (notifications.Any())
        {
            UpdateNotifications(notifications);

            Layout["Notifications"]
            .Update(NotificationsRows)
            .Visible()
            .Size(notifications.Count()+1);
        }
        else
            Layout["Notifications"].Invisible();
    }

}