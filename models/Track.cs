namespace Trackey;

class Track
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }

    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public string Filepath { get; set; } = "";

    public DateTime DownloadedAt { get; set; }
}
