using Tutor.Domain.Mentorship;
using System.Text.Json.Serialization;

namespace Tutor.Application.Sessions;

public sealed class SessionRangeRequest
{
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}

public sealed class CreateSessionRequest
{
    public Guid PairId { get; init; }
    public DateTimeOffset StartsAt { get; init; }
    public DateTimeOffset EndsAt { get; init; }
    public required string Agenda { get; init; }
}

public sealed record SessionDto(Guid Id, Guid PairId, DateTimeOffset StartsAt, DateTimeOffset EndsAt,
    string Agenda, [property: JsonConverter(typeof(JsonStringEnumConverter<SessionStatus>))] SessionStatus Status,
    string? PartnerName);

public sealed record SessionParticipantDto(Guid PairId, Guid StudentId, string? FirstName, string? LastName);
