using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
using LibVLCSharp.Shared;

namespace Trackey;


class Library
{
    private Dictionary<Guid, Track> Tracks = [];
    private Dictionary<Guid, Playlist> Playlists = [];
    private const string LIB_FILEPATH = "./data/library.json";

    public List<Guid> AllTracksIds => Tracks.Keys.ToList();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public bool TryGetTrack(Guid trackId, [NotNullWhen(true)] out Track? track)
    {
        if (!Tracks.TryGetValue(trackId, out track))
            return false;
        return true;
    }

    public Track? UpdateTrack(Guid trackId, Track track)
    {
        if (Tracks.ContainsKey(trackId))
            return Tracks[trackId] = track;
        else return null;
    }

    public async Task<bool> LoadLibrary()
    {
        Tracks = [];
        Playlists = [];
        if (!File.Exists(LIB_FILEPATH))
            return true;

        try
        {
            using FileStream fs = File.Open(LIB_FILEPATH, FileMode.Open);

            var data = await JsonSerializer.DeserializeAsync<LibraryData>(fs);

            if (data is null)
                return false;

            Tracks = data.Tracks.ToDictionary(t => t.Id);
            Playlists = data.Playlists.ToDictionary(p => p.Id);

            Logger.Log($"Loaded Library Successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            return false;
        }
    }

    public async Task<bool> SaveLibrary()
    {
        try
        {
            var data = new LibraryData() {
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
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            return false;
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
}

class LibraryData
{
    public List<Track> Tracks { get; set; } = [];
    public List<Playlist> Playlists { get; set; } = [];
}