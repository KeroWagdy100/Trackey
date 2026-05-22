using System.Dynamic;

namespace Trackey;

class Playlist
{
    public Guid Id {get; set;} = Guid.NewGuid();
    public string Title { get; set; } = "N/A";
    public List<Guid> Tracks {get; private set;} = [];

    public Playlist AddTrack(Guid id)
    {
        Tracks.Add(id);
        return this;
    }

    public bool RemoveTrack(Guid id) => Tracks.Remove(id);
}