
namespace Trackey;

static class Logger
{
    private static readonly string filepath = "./trackey.log";
    private static readonly object _lock = new();

    public static void Log(string message)
    {
        lock (_lock)
        {
            File.AppendAllText(filepath, $"[{DateTime.Now}: {message}]\n");
        }
    }

    public static void Clear()
    {
        File.WriteAllText(filepath, "");
    }
}