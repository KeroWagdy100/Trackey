namespace Trackey;

record KeyCombo (
    ConsoleKey Key,
    char? Char = null,
    bool Ctrl = false,
    bool Shift = false,
    bool Alt = false
) {
    public bool IsMatching(ConsoleKeyInfo keyInfo)
    {
        return (keyInfo.Key == Key || Char is not null && keyInfo.KeyChar == Char)
        && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) == Ctrl
        && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift) == Shift
        && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt) == Alt;
    }

    public override string ToString()
    {
        List<string> parts = [];
        
        if (Ctrl) parts.Add("Ctrl");
        if (Shift) parts.Add("Shift");
        if (Alt) parts.Add("Alt");

        parts.Add(Char is not null ? $"{Char}" : Key.ToString());

        return string.Join('+', parts);
    }
};