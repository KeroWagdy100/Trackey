using YoutubeDLSharp;
namespace Trackey;


class DownloadTaskInfo
{
    public Guid             Id           {get; init;}
    public string           Url          {get; init;} = "N/A";
    public string           Title        {get; init;} = "N/A";
    public string           Artist       {get; init;} = "N/A";
    public string?          FilePath     {get; set; }
    public string?          ErrorMessage {get; set; }
    public DateTime?        CompletedAt  {get; set; }
    public DownloadProgress Progress     {get; set; } = new DownloadProgress(DownloadState.None);

    public DownloadState State => Progress.State;

    public void UpdateProgress(DownloadProgress progress)
    {
        this.Progress = progress;
        Logger.Log($"P: {progress.Progress.ToString()}");
    }

    public override string ToString()
    {
        return $"{State.ToString()}";
    }
}