namespace Trackey;

enum NotificationType
{
    SuccessMessage,
    Warning,
    Error
}

record Notification (
    string Text,
    NotificationType Type,
    DateTime CreatedAt
)
{
    public string RenderedText()
    {
        if (Type == NotificationType.SuccessMessage)
            return $"[green]✓ {Text}[/]";
        else if (Type == NotificationType.Warning)
            return $"[yellow]⚠ {Text}[/]";

        return $"[red]✖ {Text}[/]";
    }

    public static Notification Error(string text) => new(text, NotificationType.Error, DateTime.Now);
    public static Notification Warning(string text) => new(text, NotificationType.Warning, DateTime.Now);
    public static Notification Success(string text) => new(text, NotificationType.SuccessMessage, DateTime.Now);
}