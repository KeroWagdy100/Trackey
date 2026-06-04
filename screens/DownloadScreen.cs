using System.Data.Common;
using Spectre.Console;
using Spectre.Console.Rendering;
using YoutubeDLSharp.Metadata;

namespace Trackey;

sealed class DownloadScreen : PromptScreen
{
    public DownloadScreen(Application app) : base(app)
    {
        Title = "Download Screen";
        AddQuestion(new Question("Paste YT url (Ctrl+Shift+v)", null, DownloadService.ValidateUrlChar));
        AddQuestion(new Question("Title", null, Library.ValidateTitleChar));
        AddQuestion(new Question("Artist", null, Library.ValidateArtistChar));
    }

    public override async void HandleInput(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Enter && hoveredIndex == 0)
        {
            var notificationId = app.AddNotification(Notification.Message("Fetching metadata..."));
            var operationResult = await app.Downloader.DownloadMetadataAsync(Answer(0));
            app.RemoveNotification(notificationId);

            if (!operationResult.Success || operationResult.Data is null)
            {
                app.AddNotification(Notification.Error("Fetching metadata failed. Try again!"));
                return;
            }
            app.AddNotification(Notification.Success("Fetched successfully!", 2));

            VideoData data = operationResult.Data;

            if (app.Lib.TryGetTrackByVideoId(data.ID, out Track? track))
                app.AddNotification(Notification.Warning($"Track already exists! Title: {track.Title}"));

            Logger.Log($"Metadata: {data.Title} - {data.Artist}");

            string title = string.IsNullOrEmpty(data.Title) ?
            "N/A" :
            string.Concat(data.Title.Select(c => Library.ValidateTitleChar(c) ? c : '?'));

            questions[1].Input.SetText(title);

            string artist = string.IsNullOrEmpty(data.Channel) ?
            "N/A" :
            string.Concat(data.Channel.Select(c => Library.ValidateTitleChar(c) ? c : '?'));
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

            string prompt = UI.Sanitize(q.Prompt);
            if (i == hoveredIndex)
                prompt = "[yellow]" + prompt + "[/]";
            table.AddRow(new Markup(prompt), q.Input.Render());
        }

        return table;
    }
}