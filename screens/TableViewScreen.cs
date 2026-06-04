using Spectre.Console;
using Spectre.Console.Rendering;
using System.Linq;

namespace Trackey;

class TableViewScreen<T> : Screen where T : ITableRow
{
    protected List<T> Items { get; } = [];
    protected List<int> VisibleItemsIndices { get; private set; } = [];
    protected HashSet<int> SelectedIndices { get; } = [];

    protected InputText? SearchedText;
    protected bool SearchInputActive = true;
    protected bool IsSearching => SearchedText is not null;

    public override int OptionsCount() => VisibleItemsIndices.Count;
    public virtual bool MultiSelect => false;


    public TableViewScreen(Application app) : base(app)
    {
    }

    public void AddItem(T newItem)
    {
        Items.Add(newItem);
        UpdateVisibleItems();
    }
    public void AddItems(IEnumerable<T> newItems)
    {
        Items.AddRange(newItems);
        UpdateVisibleItems();
    }

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

        bool hasItems = OptionsCount() > 0;
        foreach (var i in VisibleItemsIndices)
        {
            List<string> vals = Items[i].Cells();

            if (SelectedIndices.Contains(i))
                vals.Insert(0, "[[✓]]");
            else
                vals.Insert(0, "[[ ]]");

            for (int j = 0; j < vals.Count; ++j)
            {
                string col = 
                (hasItems && i == VisibleItemsIndices[hoveredIndex] && (!IsSearching || !SearchInputActive))
                ? "yellow"
                : "white";
                vals[j] = $"[{col}]{(j > 0 ? UI.Sanitize(vals[j], 30) : vals[j])}[/]";
            }



            table.AddRow(vals.ToArray());
        }

        return new Padder(table, new Padding(0, 1, 0, 0));
    }

    public override IRenderable? Footer()
    {
        if (!IsSearching)
            return null;
        string slashStyle = SearchInputActive ? "yellow" : "white";
        return new Markup($"[bold {slashStyle}]/[/]{SearchedText!.RenderedText()}");
    }

    protected void UpdateVisibleItems()
    {
        if (!IsSearching)
            VisibleItemsIndices = Enumerable.Range(0, Items.Count()).ToList();
        else
            VisibleItemsIndices = Items
            .Select((item, index) => (item, index))
            .Where(x => x.item.Search(SearchedText!.Text))
            .Select(x => x.index)
            .ToList();
    }

    public override void HandleInput(ConsoleKeyInfo key)
    {
        // Cancel Search
        if (key.Key == ConsoleKey.Escape && IsSearching)
            CancelSearch();
        else if (IsSearching && SearchInputActive)
        {
            // Cancel Search if trying to erase empty search
            if (key.Key == ConsoleKey.Backspace && string.IsNullOrEmpty(SearchedText!.Text))
                CancelSearch();
            
            // Pause Search
            else if (key.Key == ConsoleKey.Enter || key.KeyChar == '/')
                PauseSearch();
            else
            {
                SearchedText!.HandleInput(key);
                UpdateVisibleItems();
            }
        }
        else if (key.KeyChar == '/')
        {
            // Continue Searching
            if (!SearchInputActive)
                ResumeSearch();
            
            // Start new search
            else
                StartSearch();
        }
        else if (key.Key == ConsoleKey.Spacebar  && OptionsCount() > 0)
            ToggleSelection(VisibleItemsIndices[hoveredIndex]);
        else
            base.HandleInput(key);
    }

    public void StartSearch()
    {
        SearchedText = new() {IsActive = true};
        hoveredIndex = 0;
    }

    public void PauseSearch()
    {
        SearchInputActive = false;
        SearchedText!.IsActive = false;
    }

    public void ResumeSearch()
    {
        SearchInputActive = true;
        SearchedText!.IsActive = true;
    }

    public void CancelSearch()
    {
        SearchedText = null;
        UpdateVisibleItems();
    }

    public void ToggleSelection(int index)
    {
        if (!SelectedIndices.Remove(index))
            SelectedIndices.Add(index);
    }

    public void ClearSelection() => SelectedIndices.Clear();
}