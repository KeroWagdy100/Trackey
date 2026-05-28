using Spectre.Console.Rendering;

namespace Trackey;

abstract class Screen
{
    protected Application app;
    protected Screen(Application app)
    {
        this.app = app;
    }

    public abstract IRenderable Render();
    public abstract void HandleInput(ConsoleKeyInfo key);
    public bool CapturesTextInput { get; protected set; } = false;
    public string Title { get; protected set; } = "";
}
