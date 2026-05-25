using System.Formats.Asn1;
using Spectre.Console;
using Spectre.Console.Rendering;
using Trackey.Utils;

namespace Trackey;

/*
All screens should be scrollable
*/

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
        Table table = new Table().AddColumn("Select one of the following:").Border(TableBorder.None);
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
        if (key.Key == ConsoleKey.DownArrow)
            MoveDown();
        else if (key.Key == ConsoleKey.UpArrow)
            MoveUp();
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
            app.NavigateTo(new LoginScreen(app));
        }
        else
        {
            // Register
        }
    }

    public HomeScreen(Application app) : base(app, ["Login", "Register"])
    {
    }
}


abstract class PromptScreen : Screen
{
    public struct Question(string prompt, string answer = "", bool answered = false)
    {
        public string prompt = prompt;
        public string answer = answer;
        public bool isAnswered = answered;
    }

    protected List<Question> questions;
    protected int currentQuestionIndex = 0;

    protected PromptScreen(Application app, List<string> questionPrompts) : base(app)
    {
        questions = [];
        questionPrompts.ForEach(q => questions.Add(new Question() { prompt = q }));
        CapturesTextInput = true;
    }

    public override IRenderable Render()
    {

        var lines = questions.Select(q => new Markup($"{q.prompt}: {q.answer}"));
        return new Rows(lines);
    }

    public override void HandleInput(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Enter)
            ++currentQuestionIndex;
        else if (key.Key == ConsoleKey.Backspace)
        {
            Question question = questions[currentQuestionIndex];
            if (!string.IsNullOrEmpty(question.answer))
                question.answer = question.answer[..^1];
            questions[currentQuestionIndex] = question;
        }
        else if (!char.IsControl(key.KeyChar))
        {
            Question question = questions[currentQuestionIndex];
            question.answer += key.KeyChar;
            questions[currentQuestionIndex] = question;
        }
    }
}

sealed class LoginScreen : PromptScreen
{
    public LoginScreen(Application app) : base(app, ["username", "password"])
    {
    }

}

