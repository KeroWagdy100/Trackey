namespace Trackey;

class QueueManager
{
    public List<Guid> Tracks {get; private set;}
    private int curr = -1;

    public QueueManager()
    {
        Tracks = [];
    }

    public void AddTrack(Guid trackId) => Tracks.Add(trackId);
    public void AddPlaylist(Playlist playlist)
    {
        foreach (Guid trackId in playlist.Tracks)
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

}