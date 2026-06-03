namespace Trackey;

enum NotificationType
{
    SuccessMessage,
    Warning,
    Error,
    Message
}

record Notification (
    string Text,
    NotificationType Type,
    DateTime CreatedAt,
    double ExpiresAfter = 4.0
)
{
    public string RenderedText()
    {
        if (Type == NotificationType.SuccessMessage)
            return $"[green]✓ {Text}[/]";
        else if (Type == NotificationType.Warning)
            return $"[yellow]⚠ {Text}[/]";
        else if (Type == NotificationType.Error)
            return $"[red]✖ {Text}[/]";

        return $"[white]{Text}[/]";
    }

    public bool IsExpired() => (DateTime.Now - CreatedAt).TotalSeconds >= ExpiresAfter;

    public static Notification Error(string text, double duration = 4.0) => new(text, NotificationType.Error, DateTime.Now, duration);
    public static Notification Warning(string text, double duration = 4.0) => new(text, NotificationType.Warning, DateTime.Now, duration);
    public static Notification Success(string text, double duration = 4.0) => new(text, NotificationType.SuccessMessage, DateTime.Now, duration);
    public static Notification Message(string text, double duration = 4.0) => new(text, NotificationType.Message, DateTime.Now, duration);
}