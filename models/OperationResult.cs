namespace Trackey;

public record OperationResult(
    bool Success,
    string? ErrorMessage = null
)
{
    public static OperationResult Ok() => new(true);
    public static OperationResult Fail(string error) => new(false, error);
}

public record OperationResult<T>(
    bool Success,
    T? Data = default,
    string? ErrorMessage = null
)
{
    public static OperationResult<T> Ok(T data) => new(true, data);
    public static OperationResult<T> Fail(string error) => new(false, default, error);
}