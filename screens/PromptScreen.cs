using Spectre.Console;
using Spectre.Console.Rendering;

namespace Trackey;

abstract class PromptScreen : Screen
{
    public class Question
    {
        public string Prompt;
        public InputText Input = new();
        public bool IsAnswered;
        public List<string> Errors = [];
        public Func<string, ValidationResult>? Validator;
        public Predicate<char>? IsValidChar;

        public Question( string prompt, Func<string, ValidationResult>? validator = null, Predicate<char>? isValidChar = null)
        {
            Prompt = prompt;
            Validator = validator;
            IsValidChar = isValidChar;
            Input.CharValidator = isValidChar;
        }

        public string Answer => Input.Text;
    }

    protected List<Question> questions;
    protected int currentQuestionIndex = 0;

    protected PromptScreen(Application app) : base(app)
    {
        questions = [];
        CapturesTextInput = true;
    }

    protected void AddQuestion(Question question)
    {
        questions.Add(question);
        if (questions.Count == 1)
            questions[0].Input.IsActive = true;
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

            string errors = string.Join("\n", q.Errors);

            string prompt = q.Prompt;
            if (i == currentQuestionIndex)
                prompt = "[yellow]" + prompt + "[/]";
            table.AddRow(new Markup(prompt), q.Input.Render(), new Markup(errors, new Style(ConsoleColor.Red)));
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
                MoveDown();
        }
        else if (key.Key == ConsoleKey.Escape)
            app.NavigateBack();
        else if (char.IsAscii(key.KeyChar))
        {
            questions[currentQuestionIndex].Input.HandleInput(key);
            UpdateValidation();
        }
        else 
            base.HandleInput(key);
    }

    protected abstract void OnSubmit();
    protected string Answer(int index) => questions[index].Answer;

    public override void MoveTo(int index)
    {
        questions[currentQuestionIndex].Input.IsActive = false;
        base.MoveTo(index);
        questions[currentQuestionIndex].Input.IsActive = true;
    }
    public override void MoveUp()
    {
        questions[currentQuestionIndex].Input.IsActive = false;
        base.MoveUp();
        questions[currentQuestionIndex].Input.IsActive = true;
    }
    public override void MoveDown()
    {
        questions[currentQuestionIndex].Input.IsActive = false;
        base.MoveDown();
        questions[currentQuestionIndex].Input.IsActive = true;
    }


    protected void Reset(int index)
    {
        if (index >= 0 && index < questions.Count)
            questions[index].Input.Reset();
    }

    protected void Reset()
    {
        for (int i = 0; i < questions.Count; ++i)
            Reset(i);
    }

    protected void ClearErrors(int index) => questions[index].Errors.Clear();

    protected void ClearErrors()
    {
        for (int i = 0; i < questions.Count; ++i)
            ClearErrors(i);

    }

    protected void AddError(int index, string error) => questions[index].Errors.Add(error);



    protected void UpdateValidation()
    {
        foreach (var q in questions)
        {
            if (q.Validator is null)
                continue;

            var res = q.Validator(q.Answer);
            q.Errors = res.Errors ?? [];
        }
    }

    public override int OptionsCount() => questions.Count;

}
