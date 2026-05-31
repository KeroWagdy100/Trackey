using System.Dynamic;

namespace Trackey;

class Playlist : ITableRow
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Title { get; set; } = "";
    public List<Guid> TrackIds { get; set; } = [];


    public DateTime CreatedAt { get; private set; } = DateTime.Now;

    public static List<string> Headers()
    {
        return ["Title", "Num. Of tracks", "Created at"];
    }

    public Playlist AddTrack(Guid id)
    {
        TrackIds.Add(id);
        return this;
    }

    public List<string> Cells()
    {
        return [Title, TrackIds.Count.ToString(), CreatedAt.ToString()];
    }

    public bool RemoveTrack(Guid id) => TrackIds.Remove(id);
}