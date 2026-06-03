namespace Trackey;

static class Paths
{
    public static readonly string DataDir = Path.Combine(AppContext.BaseDirectory, "data");
    public static readonly string MusicDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Trackey");
    public static readonly string ToolsDir = Path.Combine(AppContext.BaseDirectory, "tools");
    public static readonly string YtDlpDir = Path.Combine(ToolsDir, "ytdlp");
    public static readonly string FfmpegDir = Path.Combine(ToolsDir, "ffmpeg");

    public static readonly string UsersFile = Path.Combine(DataDir, "users.json");
    public static readonly string LibraryFile = Path.Combine(DataDir, "library.json");
    public static readonly string LogFile = Path.Combine(AppContext.BaseDirectory, "trackey.log");

    public static readonly string YtDlpPath = Path.Combine(YtDlpDir, OperatingSystem.IsWindows() ? "yt-dlp.exe" : "yt-dlp");
    public static readonly string FfmpegPath = Path.Combine(FfmpegDir, OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg");


    public static void Init()
    {
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(MusicDir);
        Directory.CreateDirectory(ToolsDir);
        Directory.CreateDirectory(YtDlpDir);
        Directory.CreateDirectory(FfmpegDir);
    }
}