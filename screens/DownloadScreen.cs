using System.Data.Common;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Trackey;

sealed class DownloadScreen : PromptScreen
{
    public DownloadScreen(Application app) : base(app)
    {
        Title = "Download Screen";
        AddQuestion(new Question("Paste Download Link", null, DownloadService.ValidateUrlChar));
        AddQuestion(new Question("Title", null, Library.ValidateTitleChar));
        AddQuestion(new Question("Artist", null, Library.ValidateArtistChar));
    }

    public override async void HandleInput(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Enter && hoveredIndex == 0)
        {
            var data = await app.Downloader.DownloadMetadataAsync(Answer(0));

            Logger.Log($"Metadata: {data?.Title} - {data?.Artist}");

            string title = string.IsNullOrEmpty(data?.Title) ?
            "N/A" :
            string.Concat(data?.Title.Select(c => Library.ValidateTitleChar(c) ? c : '?') ?? "");

            questions[1].Input.SetText(title);

            string artist = string.IsNullOrEmpty(data?.Channel) ?
            "N/A" :
            string.Concat(data?.Channel.Select(c => Library.ValidateTitleChar(c) ? c : '?') ?? "");
            questions[2].Input.SetText(artist);

            MoveTo(1);
        }
        else
            base.HandleInput(key);
    }

    protected override void OnSubmit()
    {
        string url = Answer(0);
        string title = Answer(1);
        string artist = Answer(2);
        _ = app.AddDownload(url, title, artist);
        app.NavigateBack(false);
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

            string errors = string.Join("\n", q.Errors);

            string prompt = Ui.Sanitize(q.Prompt);
            if (i == hoveredIndex)
                prompt = "[yellow]" + prompt + "[/]";
            table.AddRow(new Markup(prompt), q.Input.Render());
        }

        return table;
    }
}