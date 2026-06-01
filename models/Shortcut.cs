
namespace Trackey;

class Shortcut: ITableRow
{
    public required KeyCombo Combo;
    public required string Description;

    public static List<string> Headers() => ["Keys", "Description"];

    public List<string> Cells() => [Combo.ToString(), Description];

    public bool IsMatching(ConsoleKeyInfo keyInfo) => Combo.IsMatching(keyInfo);
}