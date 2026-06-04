using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
using LibVLCSharp.Shared;

namespace Trackey;


class Library
{
    private Dictionary<Guid, Track> Tracks = [];
    private Dictionary<Guid, Playlist> Playlists = [];
    private static readonly string LIB_FILEPATH = Paths.LibraryFile;

    public static Predicate<char> ValidateTitleChar = c => char.IsAsciiLetterOrDigit(c) || "!@#$%^&*()[] ".Contains(c);
    public static Predicate<char> ValidateArtistChar = c => char.IsAsciiLetterOrDigit(c) || "!@#$%^&*()[] ".Contains(c);

    public IEnumerable<Guid> AllTracksIds => Tracks.Keys.ToList();
    public IEnumerable<Track> AllTracks => Tracks.Values.ToList();

    public IEnumerable<Guid> AllPlaylistsIds => Playlists.Keys.ToList();
    public IEnumerable<Playlist> AllPlaylists => Playlists.Values.ToList();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async void AddTracksToPlaylist(Guid playlistId, IEnumerable<Guid> tracksIds)
    {
        if (!TryGetPlaylist(playlistId, out var playlist))
            throw new ArgumentException($"Playlist with {playlistId} not found");

        foreach (var trackId in tracksIds)
            if (!playlist.TrackIds.Contains(trackId))
                playlist.AddTrack(trackId);
        await SaveLibrary();
    }

    public async void RemoveTracksFromPlaylist(Guid playlistId, IEnumerable<Guid> tracksIds)
    {
        if (!TryGetPlaylist(playlistId, out var playlist))
            throw new ArgumentException($"Playlist with {playlistId} not found");

        foreach (var trackId in tracksIds)
            playlist.TrackIds.RemoveAll(id => id == trackId);
        await SaveLibrary();
    }
    public async void RenamePlaylist(Guid playlistId, string newTitle)
    {
        if (!TryGetPlaylist(playlistId, out var playlist))
            throw new ArgumentException($"Playlist with {playlistId} not found");
        playlist.Title = newTitle;
        await SaveLibrary();
    }

    public bool TryGetTrack(Guid trackId, [NotNullWhen(true)] out Track? track)
    {
        if (!Tracks.TryGetValue(trackId, out track))
            return false;
        return true;
    }

    public bool TryGetPlaylist(Guid playlistId, [NotNullWhen(true)] out Playlist? playlist)
    {
        if (!Playlists.TryGetValue(playlistId, out playlist))
            return false;
        return true;
    }

    public IEnumerable<Track> GetPlaylistTracks(Guid playlistId)
    {
        if (!TryGetPlaylist(playlistId, out var playlist))
            throw new ArgumentException($"Playlist with {playlistId} not found");

        var ids = playlist.TrackIds;
        List<Track> tracks = [];
        foreach (var id in ids)
        {
            if (!TryGetTrack(id, out Track? track))
                throw new InvalidOperationException($"Track with {id} not found");
            tracks.Add(track);
        }

        return tracks;
    }

    public string GetPlaylistTitle(Guid playlistId)
    {
        if (!TryGetPlaylist(playlistId, out var playlist))
            throw new ArgumentException($"Playlist with {playlistId} not found");
        return playlist.Title;
    }

    public Track? UpdateTrack(Guid trackId, Track track)
    {
        if (Tracks.ContainsKey(trackId))
            return Tracks[trackId] = track;
        else return null;
    }

    public Playlist? UpdatePlaylist(Guid playlistId,  Playlist playlist)
    {
        if (Playlists.ContainsKey(playlistId))
            return Playlists[playlistId] = playlist;
        else return null;
    }

    public async Task<OperationResult> LoadLibrary()
    {
        Tracks = [];
        Playlists = [];
        if (!File.Exists(LIB_FILEPATH))
            return OperationResult.Ok();

        try
        {
            using FileStream fs = File.Open(LIB_FILEPATH, FileMode.Open);

            var data = await JsonSerializer.DeserializeAsync<LibraryData>(fs);

            if (data is null)
                return OperationResult.Fail("Failed to load library");

            Tracks = data.Tracks.ToDictionary(t => t.Id);
            Playlists = data.Playlists.ToDictionary(p => p.Id);


            Logger.Log($"Loaded Library Successfully");
            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            return OperationResult.Fail("Failed to load library");
        }
    }

    public async Task<OperationResult> SaveLibrary()
    {
        try
        {
            var data = new LibraryData()
            {
                Tracks = Tracks.Values.ToList(),
                Playlists = Playlists.Values.ToList()
            };

            using FileStream fs = File.Open(LIB_FILEPATH, FileMode.Create);
            await JsonSerializer.SerializeAsync(
                fs,
                data,
                JsonOptions
            );

            Logger.Log($"Saved Library Successfully");
            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            return OperationResult.Fail("Failed to save library");
        }
    }

    public async Task AddTrack(Track track)
    {
        Tracks[track.Id] = track;
        await SaveLibrary();
    }
    public async Task AddPlaylist(Playlist playlist)
    {
        Playlists[playlist.Id] = playlist;
        await SaveLibrary();
    }

    public bool RemoveTrack(Guid trackId) => Tracks.Remove(trackId);
    public bool RemovePlaylist(Guid playlistId) => Playlists.Remove(playlistId);

    public bool TryGetTrackByVideoId(string id, [NotNullWhen(true)] out Track? track)
    {
        track = null;
        if (Tracks.Values.Any(t => t.VideoId == id))
        {
            track = Tracks.Values.First(t => t.VideoId == id);
            return true; 
        }
        return false;
    }
}

class LibraryData
{
    public List<Track> Tracks { get; set; } = [];
    public List<Playlist> Playlists { get; set; } = [];
}
