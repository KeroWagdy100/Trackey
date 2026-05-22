namespace Trackey;

class Library
{
    private Dictionary<Guid, Track> tracks = new();
    private Dictionary<Guid, Playlist> playlists = new();

    public Track? GetTrack(Guid trackId) => tracks[trackId];
    public Track? UpdateTrack(Guid trackId, Track track)
    {
        if (tracks.ContainsKey(trackId))
            return tracks[trackId] = track;
        else return null;
    }

    public void AddTrack(Track track)           => tracks[track.Id] = track;
    public void AddPlaylist(Playlist playlist)  => playlists[playlist.Id] = playlist;

    public bool RemoveTrack(Guid trackId)       => tracks.Remove(trackId);
    public bool RemovePlaylist(Guid playlistId) => playlists.Remove(playlistId);
}