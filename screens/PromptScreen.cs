using Spectre.Console;
using Spectre.Console.Rendering;

namespace Trackey;

abstract class PromptScreen : Screen
{
    public class Question(
        string prompt,
        Func<string, ValidationResult>? validator = null,
        Predicate<char>? isValidChar = null,
        string answer = "",
        bool answered = false)
    {
        public string prompt = prompt;
        public string answer = answer;
        public bool isAnswered = answered;
        public List<string> errors = [];
        public Func<string, ValidationResult>? Validator = validator;
        public Predicate<char>? IsValidChar = isValidChar;
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
                MoveToNext();
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
        else
        {
            Question question = questions[currentQuestionIndex];

            if (question.IsValidChar is not null && !question.IsValidChar(key.KeyChar))
                return;

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
            q.errors = res.Errors ?? [];
        }
    }
}
