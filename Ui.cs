namespace Trackey;

class Ui
{
    public enum Screen
    {
        HOME,
        LIBRARY,
        PLAYLIST,
        ADD_TO_PLAYLIST
    };

    public Screen CurrentScreen {get; set;} = Screen.HOME;

    public void HandleInput()
    {
        
    }
}