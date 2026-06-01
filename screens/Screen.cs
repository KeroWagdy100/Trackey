using Spectre.Console.Rendering;

namespace Trackey;

abstract class Screen
{
    protected Application app;
    protected Screen(Application app)
    {
        this.app = app;
    }

    protected int hoveredIndex = 0;
    public abstract IRenderable Render();
    public virtual void HandleInput(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.UpArrow
        || key.Key == ConsoleKey.Tab && key.Modifiers.HasFlag(ConsoleModifiers.Shift)
        || key.Key == ConsoleKey.K && !CapturesTextInput)
            MoveUp();
        else if (key.Key == ConsoleKey.DownArrow
        || key.Key == ConsoleKey.Tab
        || key.Key == ConsoleKey.J && !CapturesTextInput)
            MoveDown();
    }

    public abstract int OptionsCount();
    public virtual void MoveTo(int index) => hoveredIndex = index < OptionsCount() && index >= 0 ? index : hoveredIndex;
    public virtual void MoveUp() => hoveredIndex = (hoveredIndex - 1 + OptionsCount()) % OptionsCount();
    public virtual void MoveDown() => hoveredIndex = (hoveredIndex + 1) % OptionsCount();

    public bool CapturesTextInput { get; protected set; } = false;
    public virtual string Title {get; set;} = "";
    public virtual IEnumerable<Shortcut> Shortcuts => [];
}