namespace Trackey;

class Program
{

    static async Task Main(string[] args)
    {
        Application app = new();

        await app.InitializeAsync();

        app.Run();

        await app.FinalizeAsync();
    }

}


