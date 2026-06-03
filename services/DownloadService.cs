namespace Trackey;

using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

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
    private readonly YoutubeDL ytdl;

    public static Predicate<char> ValidateUrlChar = c => char.IsLetterOrDigit(c) || ":=./?_-".Contains(c);

    public DownloadService()
    {
        ytdl = new YoutubeDL
        {
            OutputFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)}"
        };
    }

    public async Task<DownloadResult> DownloadAudioAsync(string url, Action<DownloadProgress> progressHandler, CancellationToken token)
    {
        Logger.Log("Download Started");
        var progress = new Progress<DownloadProgress>(progressHandler);
        var res = await ytdl.RunAudioDownload(url, progress: progress, ct: token);
        Logger.Log($"Download Finished, filepath: {res.Data} [{File.Exists(res.Data)}], e: {string.Join("\n", res.ErrorOutput)}");
        return new(res.Success, res.Data, res.ErrorOutput);
    }

    public async Task<VideoData> DownloadMetadataAsync(string url)
    {
        Logger.Log($"Fetching Metadata about {url}");
        var res = await ytdl.RunVideoDataFetch(url);
        Logger.Log($"Metadata Ready, e: {string.Join("\n", res.ErrorOutput)}");
        return res.Data;
    }
}