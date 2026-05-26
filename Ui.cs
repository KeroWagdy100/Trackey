namespace Trackey;

using Spectre.Console;

class Ui
{
    private Panel PlaybackPanel { get; set; }
    private Panel MainPanel { get; set; }
    private Panel QueuePanel { get; set; }
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

        Layout = new Layout("Root").SplitColumns(
            new Layout("MainCol").Ratio(5),
            new Layout("Queue").Ratio(1)
        );

        Layout["MainCol"].SplitRows(
            new Layout("Playback").Ratio(1),
            new Layout("Main").Ratio(4)
        );

        Layout["Playback"].Update(PlaybackPanel);
        Layout["Main"].Update(MainPanel);
        Layout["Queue"].Update(QueuePanel);
    }

    public void UpdatePlaybackPanel(PlaybackInfo playbackState)
    {


        string mode = "";
        if (playbackState.PlaybackControlsUnlocked)
            mode += "[green bold][[On]][/]";
        else
            mode += "[gray][[Off]][/]";

        string track = "", volume = "";
        if (playbackState.playerState == AudioPlayer.PlayerState.NONE)
            track = "Not Playing anything now";
        else
        {
            char stateChar = playbackState.playerState == AudioPlayer.PlayerState.PLAYING ? '⏸' : '►';

            track += $"({stateChar}): {playbackState.currentTrack!.Title} | {playbackState.currentTrack.Artist}";
            volume += $"Volume: {playbackState.volume}";
        }


        Table table = new Table()
        .Expand()
        .NoBorder()
        .AddColumn("Track")
        .AddColumn("Volume")
        .AddColumn("Mode", col => col.Width(5).RightAligned());

        table.AddRow(track, volume, mode);
        table.HideHeaders();

        string title = "Trackey";
        if (!string.IsNullOrEmpty(playbackState.username))
            title += $" - {playbackState.username}";
        PlaybackPanel = new Panel(table)
        {
            Header = new(title, Justify.Center),
            Expand = true
        }.BorderColor(playbackState.PlaybackControlsUnlocked ? Color.Green : Color.Default);
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

    // TODO: Remove PlaybackPanel & QueuePanel while not registered/logged-in
    public void Update(Screen currentScreen, PlaybackInfo playbackState, List<string> tracks)
    {
        UpdatePlaybackPanel(playbackState);
        UpdateMainPanel(currentScreen);
        UpdateQueuePanel(tracks);

        Layout["Playback"].Update(PlaybackPanel);
        Layout["Main"].Update(MainPanel);
        Layout["Queue"].Update(QueuePanel);
    }

}