namespace Trackey;

class Track
{
    private static int cnt = 0;
    public Guid Id {get; set;}
    public string Title {get; set;}          = "N/A";
    public string Artist {get; set;}         = "N/A";
    public string FileLocation { get; set; } = "N/A";

}
