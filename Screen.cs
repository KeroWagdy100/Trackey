using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Trackey;

abstract class Screen
{
    protected Application app;
    protected Screen(Application app)
    {
        this.app = app;
    }

    public abstract IRenderable Render();
    public abstract void HandleInput(ConsoleKeyInfo key);
    public bool CapturesTextInput {get; protected set;} = false;
    public string Title {get; protected set;} = "";
}


abstract class TableScreen : Screen
{
    protected TableScreen(Application app, List<string> options) : base(app)
    {
        this.options = options;
    }
    protected List<string> options;

    protected int hoveredIndex = 0;
    protected HashSet<int> selectedIndices = new();

    public bool IsMultiselect { get; protected set; } = false;

    public override IRenderable Render()
    {
        Table table = new Table().AddColumn("").Border(TableBorder.None).HideHeaders();
        for (int i = 0; i < options.Count; ++i)
        {
            string style = hoveredIndex == i ? "yellow" : IsSelected(i) ? "red" : "gray";
            table.AddRow($"[{style}]{options[i]}[/]");
        }
        return table;
    }

    public abstract void Execute(int index);

    public override void HandleInput(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.Tab && key.Modifiers.HasFlag(ConsoleModifiers.Shift))
            MoveUp();
        else if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.Tab)
            MoveDown();
        else if (key.Key == ConsoleKey.Enter)
        {
            if (IsMultiselect)
                ToggleSelection(hoveredIndex);
            else
                Execute(hoveredIndex);
        }
    }


    public void MoveUp() => hoveredIndex = (hoveredIndex - 1 + options.Count) % options.Count;
    public void MoveDown() => hoveredIndex = (hoveredIndex + 1) % options.Count;

    public void ToggleSelection(int index)
    {
        if (selectedIndices.Contains(index))
            selectedIndices.Remove(index);
        else
        {
            selectedIndices.Add(index);
            if (!IsMultiselect)
                Execute(index);
        }
    }

    public bool IsSelected(int index) => selectedIndices.Contains(index);
}


sealed class HomeScreen : TableScreen
{
    public override void Execute(int index)
    {
        if (index == 0)
        {
            // Login
            app.NavigateTo(new LoginScreen(app), true);
        }
        else
        {
            // Register
            app.NavigateTo(new RegisterScreen(app), true);
        }
    }

    public HomeScreen(Application app) : base(app, ["Login", "Register"])
    {
        Title = "Home Screen";
    }
}


abstract class PromptScreen : Screen
{
    public class Question(string prompt, Func<string, ValidationResult>? validator, string answer = "", bool answered = false)
    {
        public string prompt = prompt;
        public string answer = answer;
        public bool isAnswered = answered;
        public List<string> errors = [];
        public Func<string, ValidationResult>? Validator = validator;
    }

    protected List<Question> questions;
    protected int currentQuestionIndex = 0;

    protected PromptScreen(Application app) : base(app)
    {
        questions = [];
        CapturesTextInput = true;
    }

    public override IRenderable Render()
    {
        var table = new Table()
        .Expand()
        .HideHeaders()
        .ShowRowSeparators()
        .AddColumn("FieldName", col => col.LeftAligned().Width(10))
        .AddColumn("FieldAnwer", col => col.LeftAligned().Width(18))
        .AddColumn("FieldMessage", col => col.RightAligned());
        

        for (int i = 0; i < questions.Count; ++i)
        {
            var q = questions[i];

            string errors = string.Join("\n", q.errors);

            string prompt = q.prompt;
            if (i == currentQuestionIndex)
                prompt = "[yellow]" + prompt + "[/]";
            table.AddRow($"{prompt}", $"{q.answer}", $"[red]{errors}[/]");
        }

        return table;
    }

    public override void HandleInput(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Enter)
        {
            if (currentQuestionIndex == questions.Count - 1)        
                OnSubmit();
            else
                ++currentQuestionIndex;
        }
        else if (key.Key == ConsoleKey.Backspace)
        {
            string answer = questions[currentQuestionIndex].answer;
            if (string.IsNullOrEmpty(answer))
                return;
            answer = answer[..^1];
            questions[currentQuestionIndex].answer = answer;
            UpdateValidation();
        }
        else if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.Tab && key.Modifiers.HasFlag(ConsoleModifiers.Shift)) 
            MoveToPrev();
        else if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.Tab)
            MoveToNext();
        else if (char.IsLetterOrDigit(key.KeyChar) || UserService.VALID_SPECIAL_CHARS.Any(c => c == key.KeyChar))
        {
            Question question = questions[currentQuestionIndex];
            question.answer += key.KeyChar;
            questions[currentQuestionIndex] = question;
            UpdateValidation();
        }
    }

    protected abstract void OnSubmit();
    protected string Answer(int index) => questions[index].answer;

    protected void MoveTo(int index) => currentQuestionIndex = index;
    protected void MoveToNext() => currentQuestionIndex = (currentQuestionIndex + 1) % questions.Count;
    protected void MoveToPrev() => currentQuestionIndex = (currentQuestionIndex - 1 + questions.Count) % questions.Count;


    protected void Reset(int index)
    {
        if (index >= 0 && index < questions.Count)
            questions[index].answer = "";
    }

    protected void Reset()
    {
        for (int i = 0; i < questions.Count; ++i)
            Reset(i);
    }

    protected void ClearErrors(int index) => questions[index].errors.Clear();

    protected void ClearErrors()
    {
        for (int i = 0; i < questions.Count; ++i)
            ClearErrors(i);
            
    }

    protected void AddError(int index, string error) => questions[index].errors.Add(error);

    

    protected void UpdateValidation()
    {
        foreach (var q in questions)
        {
            if (q.Validator is null)
                continue;

            var res = q.Validator(q.answer);
            q.errors = res.Errors;
        }
    }
}

sealed class LoginScreen : PromptScreen
{
    public LoginScreen(Application app) : base(app)
    {
        Title = "Login Screen";
        questions.Add(new Question("username", null));
        questions.Add(new Question("password", null));
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

sealed class RegisterScreen : PromptScreen
{
    public RegisterScreen(Application app) : base(app)
    {
        Title = "Register Screen";
        questions.Add(new Question("username", app.Users.ValidateUsername));
        questions.Add(new Question("password", app.Users.ValidatePassword));
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
                    questions[0].errors.Add(result.Errors[i]);
                else
                    questions[1].errors.Add(result.Errors[i]);
            }
            Reset();
            return;
        }

        app.SetCurrentUser(user!.Id);
        app.NavigateTo(new HomeScreen(app), false);
    }
}
