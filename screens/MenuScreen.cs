namespace Trackey;

using Spectre.Console;
using Spectre.Console.Rendering;

abstract class MenuScreen : Screen
{
    protected MenuScreen(Application app) : base(app)
    {
    }
    protected List<string> options = [];

    protected int hoveredIndex = 0;
    protected HashSet<int> selectedIndices = new();

    public bool IsMultiselect { get; protected set; } = false;

    public override IRenderable Render()
    {
        var rows = new Rows(
        options.Select((option, i) =>
        {
            string style =
                hoveredIndex == i ? "yellow" :
                IsSelected(i) ? "blue" :
                "white";
            Color color = 
                hoveredIndex == i ? Color.Yellow :
                IsSelected(i) ? Color.Blue :
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
        if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.Tab && key.Modifiers.HasFlag(ConsoleModifiers.Shift))
            MoveUp();
        else if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.Tab)
            MoveDown();
        else if (key.Key == ConsoleKey.Enter)
        {
            if (IsMultiselect)
                ToggleSelection(hoveredIndex);
            else
                Execute(hoveredIndex);
        }
    }


    public void MoveUp() => hoveredIndex = (hoveredIndex - 1 + options.Count) % options.Count;
    public void MoveDown() => hoveredIndex = (hoveredIndex + 1) % options.Count;

    public void ToggleSelection(int index)
    {
        if (selectedIndices.Contains(index))
            selectedIndices.Remove(index);
        else
        {
            selectedIndices.Add(index);
            if (!IsMultiselect)
                Execute(index);
        }
    }

    public bool IsSelected(int index) => selectedIndices.Contains(index);
}