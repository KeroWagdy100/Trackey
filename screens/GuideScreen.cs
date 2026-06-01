using System.Windows.Markup;
using Spectre.Console;
using Spectre.Console.Rendering;
namespace Trackey;

class GuideScreen : TableViewScreen<Shortcut>
{
    private int CurrentScreenShortcutsCount {get; set;}
    public GuideScreen(Application app, IEnumerable<Shortcut> shortcuts) : base(app)
    {
        Logger.Log("Guide Screen");
        CurrentScreenShortcutsCount = shortcuts.Count();
        AddItems(shortcuts);
        AddItems(app.GlobalShortcuts);
    }

    public override void HandleInput(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Escape)
            app.NavigateBack(true);
        base.HandleInput(key);
    }

    public override IRenderable Render()
    {
        var table = new Table().Expand().ShowRowSeparators().Ascii2Border();

        var cols = Shortcut.Headers();
        foreach (var col in cols)
            table.AddColumn(col);

        for (int i = 0; i < Items.Count; ++i)
        {
            List<string> vals = Items[i].Cells();

            if (i == CurrentScreenShortcutsCount)
                table.AddEmptyRow();
            for (int j = 0; j < vals.Count; ++j)
                vals[j] = $"[white]{(j > 0 ? Ui.Sanitize(vals[j]) : vals[j])}[/]";

            table.AddRow(vals.ToArray());
        }

        return table;
        // return base.Render();
    }

    public override int OptionsCount() => Items.Count;
}