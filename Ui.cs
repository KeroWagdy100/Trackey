namespace Trackey;

using Spectre.Console;

class Ui
{
    private Panel PlaybackPanel { get; set; }
    private Panel MainPanel { get; set; }
    public Layout Layout { get; private set; }

    public Ui()
    {
        PlaybackPanel = new("Nothing is playing")
        {
            Header = new PanelHeader("Trackey", Justify.Center),
            Expand = true
        };

        MainPanel = new("")
        {
            Header = new PanelHeader("Main Screen", Justify.Center),
            Expand = true
        };

        Layout = new Layout("Root")
        .SplitRows(
            new Layout("Playback").Size(4),
            new Layout("Main")
        );
        Layout["Playback"].Update(PlaybackPanel);
        Layout["Main"].Update(MainPanel);
    }

    // public enum Screen
    // {
    //     HOME,
    //     LIBRARY,
    //     PLAYLIST,
    //     ADD_TO_PLAYLIST
    // };

    // public Screen CurrentScreen { get; set; } = Screen.HOME;


    public void UpdatePlaybackPanel(PlaybackInfo playbackState)
    {
        string panelText = "";
        if (playbackState.playerState == AudioPlayer.PlayerState.NONE)
            panelText = "Not Playing anything now";
        else
        {
            char stateChar = playbackState.playerState == AudioPlayer.PlayerState.PLAYING ? '⏸' : '►';

            panelText += $"({stateChar}): {playbackState.currentTrack!.Title} | {playbackState.currentTrack.Artist}\n";
            panelText += $"Volume: {playbackState.volume}";
        }

        PlaybackPanel = new Panel(panelText)
        {
            Header = new("Trackey", Justify.Center),
            Expand = true
        }.BorderColor(Color.Green);
    }


    public void UpdateMainPanel(Screen currentScreen)
    {

        MainPanel = new Panel(currentScreen.Render())
        {
            Header = new("Main Screen", Justify.Center),
            Expand = true
        }.BorderColor(Color.Yellow);

    }

    public void Update(Screen currentScreen, PlaybackInfo playbackState)
    {
        UpdatePlaybackPanel(playbackState);
        UpdateMainPanel(currentScreen);

        Layout["Playback"].Update(PlaybackPanel);
        Layout["Main"].Update(MainPanel);
    }

    // private string mainPanelText = "";

}