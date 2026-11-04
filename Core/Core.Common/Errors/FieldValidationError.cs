namespace Core.Common.Errors;

public sealed class FieldValidationError(Dictionary<string, string[]> fields)
    : AppError(ErrorStatus.Validation, DefaultValidationMessage)
{
    public Dictionary<string, string[]> Fields { get; } = fields;
}
