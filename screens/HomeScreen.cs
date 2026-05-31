namespace Trackey;

sealed class HomeScreen : MenuScreen
{
    public override void Execute(int index)
    {
        if (options[index] == "Login")
            app.NavigateTo(new LoginScreen(app), true);
        else if (options[index] == "Register")
            app.NavigateTo(new RegisterScreen(app), true);
        else if (options[index] == "Download")
            app.NavigateTo(new DownloadScreen(app), true);
        else if (options[index] == "Logout")
        {
            app.SetCurrentUser(null);
            app.NavigateTo(new HomeScreen(app), false);
        }
        else if (options[index] == "Library (All Playlists)")
            app.NavigateTo(new LibraryViewScreen(app), true);
        else if (options[index] == "All Tracks")
            app.NavigateTo(new TrackListScreen(app, app.Lib.AllTracks), true);
    }

    public HomeScreen(Application app) : base(app)
    {
        Title = "Home Screen";

        // Login - Register
        if (app.CurrUser is null)
        {
            options.Add("Login");
            options.Add("Register");
        }
        else
        {
            options.Add("Download");
            options.Add("All Tracks");
            options.Add("Library (All Playlists)");
            options.Add("Logout");
        }

    }
}
