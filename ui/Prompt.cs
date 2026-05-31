using Spectre.Console;
using Spectre.Console.Rendering;

namespace Trackey;

class Prompt
{
    required public InputText Input {get; set;}
    required public string Ask {get; set;}

    public IRenderable Render()
    {
        Input.IsActive = true;
        return new Panel(new Markup($"{Ask}: {Input.RenderedText()}")).NoBorder();
    }
    public void HandleInput(ConsoleKeyInfo key)
    {
        Input.HandleInput(key);
    }
}