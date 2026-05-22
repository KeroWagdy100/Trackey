using System.Dynamic;

namespace Trackey;

class Playlist
{
    public Guid Id {get; set;}
    public string Title { get; set; } = "N/A";
    public List<Guid> Tracks {get; set;} = [];
}