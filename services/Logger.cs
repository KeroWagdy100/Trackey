
namespace Trackey;

static class Logger
{
    private static readonly string filepath = "./trackey.log";

    public static void Log(string message)
    {
        File.AppendAllText(filepath, $"[{DateTime.Now}: {message}]\n");
    }

    public static void Clear()
    {
        File.Open(filepath, FileMode.Truncate);
    }
}