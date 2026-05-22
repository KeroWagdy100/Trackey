using System.Text;

namespace Trackey.Utils
{
    public class MyMath
    {
        public static bool Between(int x, int left, int right)
        {
            if (left > right)
                (left, right) = (right, left);
            return x >= left && x <= right;
        }
    }
    public class MyFile
    {
        public static void AddText(FileStream fs, string text)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(text);
            fs.Write(info, 0, info.Length);
        }
    }

    public static class Terminal
    {
        public static ConsoleColor MainColor { get; set; } = ConsoleColor.Gray;
        public static ConsoleColor InputColor { get; set; } = ConsoleColor.Cyan;
        public static ConsoleColor FailColor { get; set; } = ConsoleColor.Red;
        public static ConsoleColor ShadowColor { get; set; } = ConsoleColor.Black;
        public static ConsoleColor WarningColor { get; set; } = ConsoleColor.Yellow;

        private static void Colorize(ConsoleColor color) => Console.ForegroundColor = color;
        private static void ColorReset() => Console.ForegroundColor = MainColor;

        public static bool Input(string prompt, out string result, string? rules = null)
        {
            if (rules is not null)
                WriteLine(rules, ShadowColor);

            Write($"{prompt} > ", MainColor);

            Colorize(InputColor);
            result = Console.ReadLine() ?? "";
            ColorReset();

            if (result == "")
                return false;
            return true;
        }
        public static bool Input(string prompt, out int result, string? rules = null)
        {
            if (rules is not null)
                WriteLine(rules, ShadowColor);

            Write($"{prompt} > ", MainColor);

            Colorize(InputColor);
            bool can = int.TryParse(Console.ReadLine(), out result);
            ColorReset();

            return can;
        }

        public static void OperationFailed(string text, bool newLine = true)
        {
            if(newLine)
                WriteLine(text, FailColor);
            else
                Write(text, WarningColor);
        }
        public static void Warning(string text, bool newLine = true) 
        {
            if(newLine)
                WriteLine(text, WarningColor);
            else
                Write(text, WarningColor);
        }

        public static void Write<T>(T printable, ConsoleColor? color = null)
        {
            Colorize(color ?? MainColor);
            Console.Write(printable);
            Colorize(MainColor);
        }

        public static void WriteLine<T>(T printable, ConsoleColor? color = null)
        {
            Colorize(color ?? MainColor);
            Console.WriteLine(printable);
            Colorize(MainColor);
        }
    
        public static void InvalidInputWarning<T>(string before, T input, string after)
        {
            Write("Invalid Input: ", WarningColor);
            Write(before);
            Write(input, InputColor);
            WriteLine(after);
        }

    }
}
