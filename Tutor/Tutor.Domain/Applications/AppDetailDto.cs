namespace Tutor.Domain.Applications;

public record AppDetailDto(
    Guid Id,
    Guid StudentId,
    Guid MentorId);