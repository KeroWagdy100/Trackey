namespace Trackey;

static class Paths
{
    public static readonly string DataDir = Path.Combine(AppContext.BaseDirectory, "data");
    public static readonly string UsersFile = Path.Combine(DataDir, "users.json");
    public static readonly string LibraryFile = Path.Combine(DataDir, "library.json");
    public static readonly string LogFile = Path.Combine(AppContext.BaseDirectory, "trackey.log");

    public static void Init()
    {
        Directory.CreateDirectory(DataDir);
    }
}