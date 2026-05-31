using System.Data.Common;

namespace Trackey;

class QueueManager
{
    public List<Guid> TracksIds { get; private set; }
    private int curr = -1;

    public QueueManager()
    {
        TracksIds = [];
    }

    public void Enqueue(Guid trackId)
    {
        // TracksIds.RemoveAll(tId => tId == trackId);
        TracksIds.Add(trackId);
    }


    public void Enqueue(Playlist playlist)
    {
        foreach (Guid trackId in playlist.TrackIds)
            TracksIds.Add(trackId);
    }

    public void PlayNext(Guid trackId)
    {
        // TracksIds.Remove(tId => tId == trackId);
        TracksIds.Insert(curr+1, trackId);
    }

    public bool IsEmpty => TracksIds.Count == 0;

    public Guid? Current => curr == -1 ? null : TracksIds[curr];

    public Guid Next()
    {
        if (TracksIds.Count == 0)
            throw new InvalidOperationException("Queue is empty");
        curr = (curr + 1) % TracksIds.Count;
        return TracksIds[curr];
    }

    public Guid Prev()
    {
        if (TracksIds.Count == 0)
            throw new InvalidOperationException("Queue is empty");
        curr = (curr - 1 + TracksIds.Count) % TracksIds.Count;
        return TracksIds[curr];
    }

    public IEnumerable<QueueItem> QueueItems()
    {
        for (int i = 0; i < TracksIds.Count; ++i)
        {
            yield return new QueueItem(
                new Track() { Id = TracksIds[i] },
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