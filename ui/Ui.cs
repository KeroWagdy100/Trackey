namespace Trackey;

using Spectre.Console;
using Trackey.models;
using YoutubeDLSharp;

class Ui
{
    private Panel PlaybackPanel { get; set; }
    private Panel MainPanel { get; set; }
    private Panel QueuePanel { get; set; }
    private Panel DownloadsPanel { get; set; }
    public Layout Layout { get; private set; }

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

        Layout = new Layout("Root").SplitColumns(
            new Layout("MainCol").Ratio(5),
            new Layout("Queue").Ratio(1)
        );

        Layout["MainCol"].SplitRows(
            // new Layout("Playback").Ratio(1),
            // new Layout("Main").Ratio(4)
            new Layout("Playback").Size(4),
            new Layout("Main"),
            new Layout("Downloads").Size(4)
        );

        Layout["Playback"].Update(PlaybackPanel);
        Layout["Main"].Update(MainPanel);
        Layout["Queue"].Update(QueuePanel);
        Layout["Downloads"].Update(DownloadsPanel);
    }

    private string BuildProgressBar(long currentMs, long totalMs, int width)
    {
        double ratio = (double)currentMs / totalMs;
        int filled = (int)(ratio * width);

        int currentSecond = (int)currentMs / 1000;
        int currentMinute = currentSecond / 60;
        currentSecond %= 60;

        int totalSecond = (int)totalMs / 1000;
        int totalMinute = totalSecond / 60;
        totalSecond %= 60;

        string line = 
            " [green]"
            + new string('-', filled)
            + "[/]"
            + new string('-', width - filled);

        return
            currentMinute.ToString().PadLeft(2, '0') + ":" + currentSecond.ToString().PadLeft(2, '0')
            + " "
            + line
            + totalMinute.ToString().PadLeft(2, '0') + ":" + totalSecond.ToString().PadLeft(2, '0');
    }
    

    public void UpdatePlaybackPanel(PlaybackInfo playbackState)
    {
        bool currentlyPlaying = playbackState.playerState == AudioPlayer.PlayerState.PLAYING;

        string mode = new (' ', 13);
        if (!playbackState.PlaybackControlsUnlocked)
            mode = "[gray][[[bold];[/] to unlock]][/]";
        //     mode += "[green bold][[On]][/]";
        // else

        string track = "", volume = "";
        if (playbackState.playerState == AudioPlayer.PlayerState.NONE)
            track = "Not Playing anything now";
        else
        {

            track += $"{playbackState.currentTrack!.Title} | {playbackState.currentTrack.Artist}";
            volume += $"Volume: {playbackState.volume}";
        }


        string title = "[bold]Trackey";
        if (!string.IsNullOrEmpty(playbackState.username))
            title += $" - {playbackState.username}";
        title += "[/]";

        var trackStyle = currentlyPlaying ? new Style(Color.Green) : new Style();

        Columns topColumn = new Columns(
        [
            new Markup(mode).LeftJustified(),
            new Markup(track).Centered(),
            new Markup(volume).RightJustified(),
            // new Panel(new Markup(volume).RightJustified()) {Border = BoxBorder.None},
            // new Markup(track, trackStyle).Centered(),
            // new Markup(volume).RightJustified()
        ]
        ).Expand();

        int progressBarWidth = 50;
        char stateChar = currentlyPlaying ? '⏸' : '►';
        string progressBar = BuildProgressBar(playbackState.CurrentTimeMs, playbackState.DurationMs, progressBarWidth);

        var progress = new Align(
            new Markup(
                progressBar),
            HorizontalAlignment.Center
        );
        Rows rows = new Rows(
            topColumn,
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
        }.BorderColor(Color.Yellow);
    }

    public void UpdateQueuePanel(List<string> tracks)
    {
        QueuePanel = new Panel(new Markup(string.Join("\n", tracks)))
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
                rows.Add(new Markup($"[green]✓ Downloaded: {d.Title}[/]"));
                continue;
            }

            string line = "";
            if (d.Title.Length > 15)
                line += $"{d.Title.AsSpan(0, 15)}..";
            else
                line += $"{d.Title}";

            float ratio = d.Progress.Progress;
            int filled = (int)(ratio * width);

            line +=
            " [blue]"
            + new string('█', filled)
            + "[/]"
            + new string('░', width - filled) + " "
            + Math.Round(d.Progress.Progress * 100.0).ToString() + "%";


            rows.Add(new Markup(line));
        }

        DownloadsPanel = new Panel(new Rows(rows))
        {
            Header = new("Downloads", Justify.Left),
            Expand = true
        }.BorderColor(Color.Blue);

        return rows.Count > 0;
    }


    // TODO: Remove PlaybackPanel & QueuePanel while not registered/logged-in
    public void Update(Screen currentScreen, PlaybackInfo playbackInfo, List<string> tracks, List<DownloadTaskInfo> downloads)
    {
        UpdatePlaybackPanel(playbackInfo);
        UpdateMainPanel(currentScreen);
        UpdateQueuePanel(tracks);
        Layout["Playback"].Update(PlaybackPanel);
        Layout["Main"].Update(MainPanel);
        Layout["Queue"].Update(QueuePanel);
        
        bool downloadExist = UpdateDownloadsPanel(downloads);
        if (downloadExist)
            Layout["Downloads"].Update(DownloadsPanel).Visible();
        else
            Layout["Downloads"].Invisible();
    }

}