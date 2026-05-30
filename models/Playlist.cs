using System.Dynamic;

namespace Trackey;

class Playlist
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public List<Guid> TrackIds { get; set; } = [];

    public string Title { get; set; } = "";

    public DateTime CreatedAt { get; private set; }

    public Playlist AddTrack(Guid id)
    {
        TrackIds.Add(id);
        return this;
    }

    public bool RemoveTrack(Guid id) => TrackIds.Remove(id);
}