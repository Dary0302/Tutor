using Core.Common.Errors;
using FluentResults;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Users;

namespace Tutor.Application.Sessions;

public sealed class SessionService(ISessionRepository repository, TimeProvider timeProvider) : ISessionService
{
    public async Task<Result<IReadOnlyList<SessionDto>>> ListAsync(Guid userId, UserRole role,
        SessionRangeRequest request, CancellationToken ct)
    {
        if (role is not (UserRole.Mentor or UserRole.Student))
            return Result.Fail(AppError.Forbidden());
        if (request.From == default || request.To == default || request.From >= request.To)
            return Result.Fail(new FieldValidationError(new() { ["to"] = ["Конец диапазона должен быть позже его начала."] }));

        var sessions = await repository.ListAsync(userId, role, request.From.ToUniversalTime(),
            request.To.ToUniversalTime(), ct);
        return Result.Ok(sessions);
    }

    public async Task<Result<IReadOnlyList<SessionParticipantDto>>> GetParticipantsAsync(Guid mentorId, CancellationToken ct) =>
        Result.Ok(await repository.GetParticipantsAsync(mentorId, ct));

    public async Task<Result<SessionDto>> CreateAsync(Guid mentorId, CreateSessionRequest request, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow();
        if (request.StartsAt.ToUniversalTime() <= now)
            return Result.Fail(new FieldValidationError(new() { ["startsAt"] = ["Сессия должна начинаться в будущем."] }));
        if (request.EndsAt.ToUniversalTime() <= request.StartsAt.ToUniversalTime())
            return Result.Fail(new FieldValidationError(new() { ["endsAt"] = ["Время окончания должно быть позже времени начала."] }));
        if (string.IsNullOrWhiteSpace(request.Agenda) || request.Agenda.Trim().Length > 255)
            return Result.Fail(new FieldValidationError(new() { ["agenda"] = ["Тема обязательна и не должна превышать 255 символов."] }));

        await using var edit = await repository.EditPairAsync(mentorId, request.PairId, ct);
        if (edit is null)
            return Result.Fail(AppError.NotFound("Активная пара наставничества не найдена."));
        if (edit.Pair.Status != PairStatus.Active)
            return Result.Fail(AppError.Conflict("Создавать сессии можно только в активной паре наставничества."));

        var session = new MentoringSession
        {
            PairId = edit.Pair.Id,
            StartsAt = request.StartsAt.ToUniversalTime(),
            EndsAt = request.EndsAt.ToUniversalTime(),
            Agenda = request.Agenda.Trim(),
            Status = SessionStatus.Scheduled
        };
        edit.Add(session);
        await edit.SaveAsync(ct);
        return Result.Ok(new SessionDto(session.Id, session.PairId, session.StartsAt, session.EndsAt,
            session.Agenda, session.Status, edit.Pair.StudentName));
    }

    public async Task<Result> CancelAsync(Guid mentorId, Guid sessionId, CancellationToken ct)
    {
        await using var edit = await repository.EditSessionAsync(mentorId, sessionId, ct);
        if (edit is null)
            return Result.Fail(AppError.NotFound("Сессия не найдена."));

        var session = edit.Session;
        if (session.Status != SessionStatus.Scheduled || session.StartsAt <= timeProvider.GetUtcNow())
            return Result.Fail(AppError.Conflict("Отменить можно только будущую запланированную сессию."));

        var cancelledAt = timeProvider.GetUtcNow();
        session.Status = SessionStatus.Cancelled;
        session.CancelledAt = cancelledAt;
        session.CancellationReason = SessionCancellationReason.TeacherRequest;
        await edit.SaveAsync(ct);
        return Result.Ok();
    }
}
