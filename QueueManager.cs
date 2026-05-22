namespace Trackey;

class QueueManager
{
    private List<Guid> queue;
    private int curr = -1;

    public QueueManager()
    {
        queue = [];
    }

    public void AddTrack(Guid trackId) => queue.Add(trackId);
    public void AddPlaylist(Playlist playlist) {
        foreach (Guid trackId in playlist.Tracks)
            queue.Add(trackId);
    }

    public bool HasNext() => queue.Count != 0;
    public bool HasPrev() => queue.Count != 0;
    public Guid Next()
    {
        curr = (curr + 1) % queue.Count;
        return queue[curr];
    }

    public Guid Prev()
    {
        curr = (curr - 1 + queue.Count) % queue.Count;
        return queue[curr];
    }
}