using Spectre.Console;
using Spectre.Console.Rendering;

namespace Trackey;

sealed class DownloadScreen : PromptScreen
{
    public DownloadScreen(Application app) : base(app)
    {
        Title = "Download Screen";
        questions.Add(new Question("Pase Download Link", null));
    }

    public override void HandleInput(ConsoleKeyInfo key)
    {
        // base.HandleInput(key);
        if (char.IsLetterOrDigit(key.KeyChar) || ":=./?_".Any(c => c == key.KeyChar))
            questions[0].answer += key.KeyChar;
        else if (key.Key == ConsoleKey.Enter)
            OnSubmit();
    }

    protected override void OnSubmit()
    {
        string url = Answer(0);
        _ = app.AddDownload(url);
        app.NavigateBack();
    }

    public override IRenderable Render()
    {
        var table = new Table()
        .NoBorder()
        .Expand()
        .HideHeaders()
        .AddColumn("FieldName", col => col.LeftAligned().Width(20))
        .AddColumn("FieldAnwer", col => col.LeftAligned());


        for (int i = 0; i < questions.Count; ++i)
        {
            var q = questions[i];

            string errors = string.Join("\n", q.errors);

            string prompt = q.prompt;
            if (i == currentQuestionIndex)
                prompt = "[yellow]" + prompt + "[/]";
            table.AddRow($"{prompt}", $"{q.answer}");
        }

        return table;
    }
}