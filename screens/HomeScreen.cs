namespace Trackey;

sealed class HomeScreen : TableScreen
{
    public override void Execute(int index)
    {
        if (options[index] == "Login")
            app.NavigateTo(new LoginScreen(app), false);
        else if (options[index] == "Register")
            app.NavigateTo(new RegisterScreen(app), false);
        else if (options[index] == "Download")
            app.NavigateTo(new DownloadScreen(app), true);
        else if (options[index] == "Logout")
        {
            app.SetCurrentUser(null);
            app.NavigateTo(new HomeScreen(app), false);
        }
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
            options.Add("Logout");
        }
    }
}
