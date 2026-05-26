namespace Trackey;

record ValidationResult(
    bool Success,
    string? Field,
    List<string>? Errors
);