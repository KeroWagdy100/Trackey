using Spectre.Console;
using Spectre.Console.Rendering;

namespace Trackey;

class TableViewScreen<T> : Screen where T : ITableRow
{
    protected List<T> Items { get; } = [];
    protected HashSet<int> SelectedIndices { get; } = [];

    public override int OptionsCount() => Items.Count;
    public virtual bool MultiSelect => false;

    public TableViewScreen(Application app) : base(app)
    {
    }

    public void AddItem(T newItem) => Items.Add(newItem);
    public void AddItems(IEnumerable<T> newItems) => Items.AddRange(newItems);

    public override IRenderable Render()
    {
        var table = new Table()
        .Expand()
        .NoBorder()
        .ShowRowSeparators()
        .AddColumn("", col => col.Width = 4);

        var cols = T.Headers();
        foreach (var col in cols)
            table.AddColumn(col, col => col.Centered());

        for (int i = 0; i < Items.Count; ++i)
        {
            List<string> vals = Items[i].Cells();

            if (SelectedIndices.Contains(i))
                vals.Insert(0, "[[✓]]");
            else
                vals.Insert(0, "[[ ]]");

            for (int j = 0; j < vals.Count; ++j)
            {
                string col = i == hoveredIndex ? "yellow" : "white";
                vals[j] = $"[{col}]{(j > 0 ? UI.Sanitize(vals[j], 30) : vals[j])}[/]";
            }

            table.AddRow(vals.ToArray());
        }

        return new Padder(table, new Padding(0, 1, 0, 0));
    }

    public override void HandleInput(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Spacebar)
            ToggleSelection(hoveredIndex);
        else
            base.HandleInput(key);
    }

    public void ToggleSelection(int index)
    {
        if (!SelectedIndices.Remove(index))
            SelectedIndices.Add(index);
    }

    public void ClearSelection() => SelectedIndices.Clear();
}