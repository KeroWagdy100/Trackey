namespace Trackey;

sealed class LoginScreen : PromptScreen
{
    public LoginScreen(Application app) : base(app)
    {
        Title = "Login Screen";
        AddQuestion(new Question("username", null, UserService.ValidateChar));
        AddQuestion(new Question("password", null, UserService.ValidateChar));
    }

    protected override void OnSubmit()
    {
        string username = Answer(0);
        string password = Answer(1);

        var result = app.Users.Login(username, password, out User? user);

        if (!result.Success)
        {

            if (result.Field == "username")
            {
                ClearErrors(0);
                // Reset(0);
                foreach (var e in result!.Errors!)
                    AddError(0, e);
            }
            else
            {
                ClearErrors(1);
                // Reset(1);
                foreach (var e in result!.Errors!)
                    AddError(1, e);
            }
            return;
        }

        app.SetCurrentUser(user!.Id);
        app.NavigateTo(new HomeScreen(app), false);
    }
}
