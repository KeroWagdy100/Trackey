namespace Trackey;

using Spectre.Console;

interface ITableRow
{
    static abstract List<string> Headers();
    abstract List<string> Cells();
    abstract bool Search(string text);
}