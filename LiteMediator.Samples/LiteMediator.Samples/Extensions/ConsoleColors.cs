namespace LiteMediator.Samples.Extensions;

public static class StyledConsole
{
    public static void Info(string message) => WriteLine(message, ConsoleStyle.Bold, ConsoleStyle.Cyan);

    public static void Section(string title) => WriteLine(title, ConsoleStyle.Bold, ConsoleStyle.Yellow);

    public static void Success(string message) => WriteLine(message, ConsoleStyle.Green);

    public static void Item(string label, string value)
    {
        Console.Write($"{ConsoleStyle.Magenta}[{label}]{ConsoleStyle.Reset}: ");
        Console.WriteLine(value);
    }

    public static void WriteLine(string message, params string[] styles)
    {
        var stylePrefix = string.Concat(styles);
        Console.WriteLine($"{stylePrefix}{message}{ConsoleStyle.Reset}");
    }
}

public static class ConsoleStyle
{
    public const string Reset = "\u001b[0m";
    public const string Bold = "\u001b[1m";
    public const string Cyan = "\u001b[36m";
    public const string Green = "\u001b[32m";
    public const string Yellow = "\u001b[33m";
    public const string Magenta = "\u001b[35m";
}