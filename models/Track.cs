using Spectre.Console;

namespace Trackey;

class Track : ITableRow
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }

    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public string VideoId { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public string Filepath { get; set; } = "";

    public DateTime DownloadedAt { get; set; }


    public static List<string> Headers() => ["Title", "Artist"];
    public List<string> Cells() => [Title, Artist];
    public bool Search(string text)
    {
        return Title.Contains(text)  || Artist.Contains(text);
    }

    // TODO: Add track duration
}
