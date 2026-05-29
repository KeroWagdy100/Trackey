using Spectre.Console;
using Spectre.Console.Rendering;

namespace Trackey;

class InputText
{
    private List<char> chars = [];    

    public int CursorIndex {get; private set;} = 0;
    public int Length => CursorIndex;
    public string Text => new([.. chars]);
    public Predicate<char>? CharValidator;

    public bool IsActive {get; set;} = false;
    public bool ShowCursor {get; private set;} = false;

    private DateTime lastToggled = DateTime.Now;

    public void ToggleShowCuror() => ShowCursor ^= true;

    public void SetText(string text)
    {
        chars = [.. text];
        CursorIndex = chars.Count;
    }

    public void Reset() 
    {
        chars.Clear();
        CursorIndex = 0;
    }

    public void HandleInput(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Backspace) // delete single char
        {
            if (CursorIndex > 0)
                chars.RemoveAt(--CursorIndex);
        }
        else if (key.Key == ConsoleKey.W && key.Modifiers.HasFlag(ConsoleModifiers.Control)) // delete word
        {
            bool nonEmptyFound = false;
            int i;
            for (i = CursorIndex-1; i >= 0; --i)
            {
                if (nonEmptyFound && char.IsWhiteSpace(chars[i]))
                {
                    ++i;
                    break;
                }

                if (!char.IsWhiteSpace(chars[i])) nonEmptyFound = true;
            }
            i = Math.Max(0, i);
            chars.RemoveRange(i, CursorIndex - i);
            CursorIndex = i;
        }
        else if (key.Key == ConsoleKey.RightArrow)
            MoveRight();
        else if (key.Key == ConsoleKey.LeftArrow)
            MoveLeft();
        else // insert char
        {
            if (CharValidator is not null && !CharValidator(key.KeyChar))
                return;
            chars.Insert(CursorIndex++, key.KeyChar);
        }

    }

    public void MoveRight()
    {
        if (CursorIndex == chars.Count) return;
        ++CursorIndex;
    }
    public void MoveLeft()
    {
        if (CursorIndex == 0) return;
        --CursorIndex;
    }

    // 𝖨
    public Renderable Render()
    {
        if (IsActive)
        {
            var now = DateTime.Now;
            if ((now - lastToggled).TotalMilliseconds >= 700)
            {
                ToggleShowCuror();
                lastToggled = now;
            }
        }

        string text = "";
        for (int i = 0; i < chars.Count; ++i)
        {
            string c = Ui.Sanitize(new string(chars[i], 1));

            if (IsActive && ShowCursor && i == CursorIndex)
                text += $"[black on white]{c}[/]";
            else
                text += c;
        }
        if (IsActive && ShowCursor && CursorIndex == chars.Count)
        text += "[black on white] [/]";

        return new Markup(text);
    }
}