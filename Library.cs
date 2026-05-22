namespace Trackey;

class Library
{
    private Dictionary<Guid, Track> tracks = new();

    public Track? GetTrack(Guid trackId) => tracks[trackId];
    public Track? UpdateTrack(Guid trackId, Track track)
    {
        if (tracks.ContainsKey(trackId))
            return tracks[trackId] = track;
        else return null;
    }

    public Guid CreateTrack(string filelocation, string title, string? artist)
    {
        Track track = new Track{FileLocation = filelocation, Title = title, Artist = artist ?? "N/A"};
        Guid trackId = Guid.NewGuid();
        tracks[trackId] = track;
        return trackId;
    }
}