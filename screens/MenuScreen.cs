namespace Trackey;

using Spectre.Console;
using Spectre.Console.Rendering;

abstract class MenuScreen(Application app) : Screen(app)
{
    protected List<string> options = [];

    public override IRenderable Render()
    {
        var rows = new Rows(
        options.Select((option, i) =>
        {
            string style =
                hoveredIndex == i ? "yellow" :
                "white";
            Color color = 
                hoveredIndex == i ? Color.Yellow :
                Color.White;

            return new Panel($"[{style}]{option}[/]")
                .Border(BoxBorder.Square).BorderColor(color);
        })
        );

        return new Align(
            rows,
            HorizontalAlignment.Left
            // VerticalAlignment.Middle
        );
    }

    public abstract void Execute(int index);

    public override void HandleInput(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Enter)
            Execute(hoveredIndex);
        else
            base.HandleInput(key);
    }

    public override int OptionsCount() => options.Count;
}