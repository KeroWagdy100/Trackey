using System.Diagnostics.CodeAnalysis;

namespace Trackey;

class Library
{
    private Dictionary<Guid, Track> tracks = new();
    private Dictionary<Guid, Playlist> playlists = new();

    public bool TryGetTrack(Guid trackId, [NotNullWhen(true)] out Track? track)
    {
        if (!tracks.TryGetValue(trackId, out track))
            return false;
        return true;
    }

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