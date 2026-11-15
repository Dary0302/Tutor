using System.Data;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Tutor.Application.Mentors;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Questionnaires;
using Tutor.Domain.Users;

namespace Tutor.Infrastructure.Repositories;

internal sealed class MentorSearchRepository(IDbContextFactory<TutorDbContext> factory, IMapper mapper)
    : IMentorSearchRepository
{
    public async Task<MentorPreferences?> GetPreferencesAsync(Guid userId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Set<Questionnaire>().AsNoTracking()
            .Where(q => q.UserId == userId && db.Users.Any(u => u.Id == userId))
            .ProjectTo<MentorPreferences>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<MentorSearchResult> SearchAsync(MentorSearchRequest request,
        Expression<Func<Questionnaire, MentorMatch>> compatibility, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var filtered = FilterMentors(db, request);
        var total = await filtered.CountAsync(ct);
        var offset = checked((request.Page - 1) * request.PageSize);
        var items = offset >= total
            ? []
            : await BuildPageQuery(db, filtered, request, compatibility, mapper.ConfigurationProvider).ToListAsync(ct);
        await transaction.CommitAsync(ct);
        return new MentorSearchResult(items, total, request.Page, request.PageSize);
    }

    internal static IQueryable<Questionnaire> FilterMentors(TutorDbContext db, MentorSearchRequest request)
    {
        var mentors = db.Set<Questionnaire>().AsNoTracking()
            .Where(q => q.TeacherInfo != null && db.Users.Any(u => u.Id == q.UserId && u.Role == UserRole.Mentor));

        var skillIds = request.SkillIds.Distinct().ToArray();
        if (skillIds.Length > 0)
            mentors = mentors.Where(q => q.Skills.Count(s => skillIds.Contains(s.SkillId)) == skillIds.Length);
        if (request.MinYearsExperience is { } minExperience)
            mentors = mentors.Where(q => q.TeacherInfo!.YearsExperience >= minExperience);
        if (request.MaxYearsExperience is { } maxExperience)
            mentors = mentors.Where(q => q.TeacherInfo!.YearsExperience <= maxExperience);
        if (request.DurationMonths.Length > 0)
            mentors = mentors.Where(q => request.DurationMonths.Any(m => m >= q.MonthsNumberMin && m <= q.MonthsNumberMax));
        if (request.DaysOfWeek.Length > 0)
            mentors = mentors.Where(q => q.Availability.Any(a => request.DaysOfWeek.Contains(a.DayOfWeek)));
        if (request.MinHoursPerWeek is { } minHours)
            mentors = mentors.Where(q => q.HoursNumberWeek >= minHours);
        if (request.MaxHoursPerWeek is { } maxHours)
            mentors = mentors.Where(q => q.HoursNumberWeek <= maxHours);
        if (!string.IsNullOrWhiteSpace(request.PlaceOfWork))
        {
            var pattern = "%" + EscapeLike(request.PlaceOfWork.Trim()) + "%";
            mentors = mentors.Where(q => EF.Functions.ILike(q.TeacherInfo!.PlaceOfWork, pattern, "\\"));
        }
        if (request.MinRating is { } minRating)
            mentors = mentors.Where(q => q.TeacherInfo!.Rating >= minRating);
        if (request.HasAvailablePlaces is { } hasPlaces)
        {
            var activeRequests = ActiveRequests(db);
            mentors = mentors.Where(q => (q.TeacherInfo!.MaxStudents >
                activeRequests.Count(r => r.TeacherId == q.UserId)) == hasPlaces);
        }
        return mentors;
    }

    internal static IQueryable<MentorCardDto> BuildPageQuery(TutorDbContext db, IQueryable<Questionnaire> filtered,
        MentorSearchRequest request, Expression<Func<Questionnaire, MentorMatch>> compatibility,
        IConfigurationProvider mappings)
    {
        var activeRequests = ActiveRequests(db);
        var matches = filtered.Select(compatibility).Select(match => new MentorSearchEntry
        {
            Mentor = match.Mentor,
            Compatibility = match.Compatibility,
            AvailablePlaces = Math.Max(0, match.Mentor.TeacherInfo!.MaxStudents -
                activeRequests.Count(r => r.TeacherId == match.Mentor.UserId))
        });
        var ordered = request.Sort switch
        {
            MentorSort.Compatibility => matches.OrderByDescending(x => x.Compatibility ?? -1m),
            MentorSort.Rating => matches.OrderByDescending(x => x.Mentor.TeacherInfo!.Rating.HasValue)
                .ThenByDescending(x => x.Mentor.TeacherInfo!.Rating),
            MentorSort.Experience => matches.OrderByDescending(x => x.Mentor.TeacherInfo!.YearsExperience),
            MentorSort.HoursPerWeek => matches.OrderBy(x => x.Mentor.HoursNumberWeek),
            var _ => throw new ArgumentOutOfRangeException(nameof(request.Sort))
        };
        return ordered.ThenBy(x => x.Mentor.Id)
            .Skip(checked((request.Page - 1) * request.PageSize))
            .Take(request.PageSize)
            .ProjectTo<MentorCardDto>(mappings);
    }

    private static IQueryable<MentorshipRequest> ActiveRequests(TutorDbContext db) =>
        db.Set<MentorshipRequest>().Where(r => db.Set<MentorshipPair>()
            .Any(p => p.RequestId == r.Id && p.Status == PairStatus.Active));

    private static string EscapeLike(string value) =>
        value.Replace("\\", @"\\").Replace("%", "\\%").Replace("_", "\\_");
}