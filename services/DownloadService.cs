namespace Trackey;

using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

record DownloadResult(
    bool Success,
    string? Filepath,
    IReadOnlyList<string> ErrorResult
)
{
    public override string ToString()
    {
        return
        "Donwload Result: "
        + (!Success ? $"[Fail] - {ErrorResult}" : $"[Success] - path: '{Filepath}'")
        + "\n";
    }
}

class DownloadService
{
    private YoutubeDL ytdl;

    public static Predicate<char> ValidateUrlChar = c => char.IsLetterOrDigit(c) || ":=./?_-".Contains(c);

    public async Task Init()
    {
        ytdl = new YoutubeDL
        {
            OutputFolder = Paths.MusicDir,
        };

        if (!File.Exists(Paths.YtDlpPath))
        {
            Logger.Log($"Downloading YTDLP..");
            Console.WriteLine($"Downloading yt-dlp..");

            await YoutubeDLSharp.Utils.DownloadYtDlp(Paths.YtDlpDir);
        }

        if (!File.Exists(Paths.FfmpegPath))
        {
            Logger.Log($"Downloading FFMPEG..");
            Console.WriteLine($"Downloading ffmpeg..");

            await YoutubeDLSharp.Utils.DownloadFFmpeg(Paths.FfmpegDir);
        }

        ytdl.YoutubeDLPath = Paths.YtDlpPath;
        ytdl.FFmpegPath = Paths.FfmpegPath;
    }

    public async Task<OperationResult<string>> DownloadAudioAsync(string url, Action<DownloadProgress> progressHandler, CancellationToken token, string filename)
    {
        ytdl.OutputFileTemplate = $"{filename}.%(ext)s";

        var options = new OptionSet()
        {
            // Format = "best"
        };

        Logger.Log("Download Started");
        var progress = new Progress<DownloadProgress>(progressHandler);
        var res = await ytdl.RunAudioDownload(url, progress: progress, ct: token, overrideOptions: options);
        Logger.Log($"Download Finished, filepath: {res.Data} [{File.Exists(res.Data)}], e: {string.Join("\n", res.ErrorOutput)}");
        // return new(res.Success, res.Data, res.ErrorOutput);
        return new(res.Success, res.Data, string.Join('\n', res.ErrorOutput));
    }

    public async Task<OperationResult<VideoData>> DownloadMetadataAsync(string url)
    {
        Logger.Log($"Fetching Metadata about {url}");
        var res = await ytdl.RunVideoDataFetch(url);
        Logger.Log($"Metadata Ready, e: {string.Join("\n", res.ErrorOutput)}");
        return new(res.Success, res.Data, string.Join('\n', res.ErrorOutput));
    }
}