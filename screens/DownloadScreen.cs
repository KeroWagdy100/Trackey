using Spectre.Console;
using Spectre.Console.Rendering;

namespace Trackey;

sealed class DownloadScreen : PromptScreen
{
    public DownloadScreen(Application app) : base(app)
    {
        Title = "Download Screen";
        questions.Add(new Question("Paste Download Link", null, DownloadService.ValidateUrlChar));
        questions.Add(new Question("Title", null, Library.ValidateTitleChar));
        questions.Add(new Question("Artist", null, Library.ValidateArtistChar));
    }

    public override async void HandleInput(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Enter && currentQuestionIndex == 0)
        {
            var data = await app.Downloader.DownloadMetadataAsync(Answer(0));
            // if (!string.IsNullOrEmpty(data.Title)
            // && data.Title.All(c => Library.ValidateTitleChar(c)))
            //     questions[1].answer = data.Title; 

            Logger.Log($"Metadata: {data.Title} - {data.Artist}");

            string title = string.IsNullOrEmpty(data.Title) ? "N/A" : string.Concat(data.Title.Where(c => Library.ValidateTitleChar(c)));
            questions[1].answer = title;

            string artist = string.IsNullOrEmpty(data.Channel) ? "N/A" : string.Concat(data.Channel.Where(c => Library.ValidateTitleChar(c)));
            questions[2].answer = artist;

            // if (!string.IsNullOrEmpty(data.Artist)
            // && data.Artist.All(c => Library.ValidateArtistChar(c)))
            //     questions[2].answer = data.Artist; 
            
        }

        base.HandleInput(key);
    }

    protected override void OnSubmit()
    {
        string url = Answer(0);
        string title = Answer(1);
        string artist = Answer(2);
        _ = app.AddDownload(url, title, artist);
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

            string prompt = Ui.Sanitize(q.prompt);
            if (i == currentQuestionIndex)
                prompt = "[yellow]" + prompt + "[/]";
            table.AddRow($"{prompt}", $"{Ui.Sanitize(q.answer)}");
        }

        return table;
    }
}