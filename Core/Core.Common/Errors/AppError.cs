using FluentResults;

namespace Core.Common.Errors;

public class AppError : Error
{
    private const string ForbiddenMessage = "Доступ запрещен";
    private const string UnauthorizedMessage = "Аутентификация не удалась";
    protected const string DefaultValidationMessage = "Ошибка валидации данных";
    private const string ConflictMessage = "Объект уже существует";
    private const string NotFoundMessage = "Объект не найден";
    private const string UnprocessableContentMessage = "Некорректные данные";
    private const string NotImplementedMessage = "Не реализовано";

    public ErrorStatus Status { get; }

    public AppError(ErrorStatus status, string message) : base(message)
    {
        Status = status;
        Metadata["Error"] = this;
    }

    public static AppError Forbidden(string message = ForbiddenMessage) => new(ErrorStatus.Forbidden, message);
    public static AppError Unauthorized(string message = UnauthorizedMessage) => new(ErrorStatus.Unauthorized, message);
    public static AppError Validation(string message = DefaultValidationMessage) => new(ErrorStatus.Validation, message);
    public static AppError Conflict(string message = ConflictMessage) => new(ErrorStatus.Conflict, message);
    public static AppError NotFound(string message = NotFoundMessage) => new(ErrorStatus.NotFound, message);
    public static AppError UnprocessableContent(string massage = UnprocessableContentMessage) => new(ErrorStatus.UnprocessableContent, massage);
    public static AppError NotImplemented(string message = NotImplementedMessage) => new(ErrorStatus.NotImplemented, message);
}