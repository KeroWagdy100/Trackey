namespace Trackey;

using Spectre.Console;
using Spectre.Console.Rendering;

abstract class TableScreen : Screen
{
    protected TableScreen(Application app, List<string> options) : base(app)
    {
        this.options = options;
    }
    protected List<string> options;

    protected int hoveredIndex = 0;
    protected HashSet<int> selectedIndices = new();

    public bool IsMultiselect { get; protected set; } = false;

    public override IRenderable Render()
    {
        Table table = new Table().AddColumn("").Border(TableBorder.None).HideHeaders();
        for (int i = 0; i < options.Count; ++i)
        {
            string style = hoveredIndex == i ? "yellow" : IsSelected(i) ? "red" : "gray";
            table.AddRow($"[{style}]{options[i]}[/]");
        }
        return table;
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