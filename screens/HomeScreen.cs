namespace Trackey;

sealed class HomeScreen : TableScreen
{
    public override void Execute(int index)
    {
        if (index == 0)
            app.NavigateTo(new LoginScreen(app), true);
        else if (index == 1)
            app.NavigateTo(new RegisterScreen(app), true);
        else
            app.NavigateTo(new DownloadScreen(app), true);
    }

    public HomeScreen(Application app) : base(app, ["Login", "Register", "Download"])
    {
        Title = "Home Screen";
    }
}
