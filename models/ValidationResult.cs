namespace Trackey;

record ValidationResult(
    bool Success,
    string? FieldName,
    List<string>? Errors
);