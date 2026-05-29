namespace Trackey;

sealed class RegisterScreen : PromptScreen
{
    public RegisterScreen(Application app) : base(app)
    {
        Title = "Register Screen";
        AddQuestion(new Question("username", app.Users.ValidateUsername, UserService.ValidateChar));
        AddQuestion(new Question("password", app.Users.ValidatePassword, UserService.ValidateChar));
    }

    protected override void OnSubmit()
    {
        string username = Answer(0);
        string password = Answer(1);

        var result = app.Users.Register(username, password, out User? user);

        if (!result.Success)
        {

            for (int i = 0; i < result.Errors?.Count; ++i)
            {
                if (result.Field == "username")
                    questions[0].Errors.Add(result.Errors[i]);
                else
                    questions[1].Errors.Add(result.Errors[i]);
            }
            Reset();
            return;
        }

        app.SetCurrentUser(user!.Id);
        app.NavigateTo(new HomeScreen(app), false);
    }
}
