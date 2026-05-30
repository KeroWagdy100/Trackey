using System.Data.Common;

namespace Trackey;

class QueueManager
{
    public List<Guid> Tracks { get; private set; }
    private int curr = -1;

    public QueueManager()
    {
        Tracks = [];
    }

    public void AddTrack(Guid trackId) {
        if (!Tracks.Contains(trackId))
            Tracks.Add(trackId);
    }

    public void AddPlaylist(Playlist playlist)
    {
        foreach (Guid trackId in playlist.TrackIds)
            Tracks.Add(trackId);
    }

    public bool IsEmpty => Tracks.Count == 0;
    public Guid Next()
    {
        curr = (curr + 1) % Tracks.Count;
        return Tracks[curr];
    }

    public Guid Prev()
    {
        curr = (curr - 1 + Tracks.Count) % Tracks.Count;
        return Tracks[curr];
    }

    public IEnumerable<QueueItem> QueueItems()
    {
        for (int i = 0; i < Tracks.Count; ++i)
        {
            yield return new QueueItem(
                new Track() {Id = Tracks[i]}, 
                i < curr ? QueueItemType.PREVIOUS :
                i == curr ? QueueItemType.CURRENT :
                QueueItemType.NEXT
            );
        }
    }
}

record QueueItem(
    Track Track,
    QueueItemType Type
);

enum QueueItemType
{
    PREVIOUS,
    CURRENT,
    NEXT
};