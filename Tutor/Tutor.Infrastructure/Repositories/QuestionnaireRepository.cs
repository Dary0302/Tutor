using System.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Tutor.Application.Questionnaires;
using Tutor.Domain.Questionnaires;

namespace Tutor.Infrastructure.Repositories;

internal sealed class QuestionnaireRepository(IDbContextFactory<TutorDbContext> factory, IMapper mapper)
    : IQuestionnaireRepository
{
    public async Task<QuestionnaireFormDto?> GetAsync(Guid userId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var questionnaire = await db.Set<Questionnaire>().AsNoTracking().AsSplitQuery()
            .Include(q => q.TeacherInfo)
            .Include(q => q.Skills.OrderBy(s => s.Skill.Name)).ThenInclude(s => s.Skill)
            .Include(q => q.Availability.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime))
            .SingleOrDefaultAsync(q => q.UserId == userId && db.Users.Any(u => u.Id == userId), ct);
        await transaction.CommitAsync(ct);
        return questionnaire is null ? null : mapper.Map<QuestionnaireFormDto>(questionnaire);
    }

    public async Task<bool> SaveAsync(Guid userId, QuestionnaireFormDto form, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var owner = await db.Users.FromSql($"""SELECT * FROM "Users" WHERE "Id" = {userId} FOR UPDATE""")
            .AsNoTracking().SingleOrDefaultAsync(ct);
        if (owner is null)
            return false;

        var questionnaire = await db.Set<Questionnaire>().Include(q => q.TeacherInfo)
            .SingleOrDefaultAsync(q => q.UserId == userId, ct);
        if (questionnaire is null)
        {
            questionnaire = mapper.Map<Questionnaire>(form);
            questionnaire.UserId = userId;
            db.Add(questionnaire);
        }
        else
        {
            mapper.Map(form, questionnaire);
            await db.Set<QuestionnaireSkill>().Where(x => x.QuestionnaireId == questionnaire.Id).ExecuteDeleteAsync(ct);
            await db.Set<AvailabilitySlot>().Where(x => x.QuestionnaireId == questionnaire.Id).ExecuteDeleteAsync(ct);
        }

        questionnaire.TeacherInfo = form.TeacherInfo is null ? null : mapper.Map(form.TeacherInfo, questionnaire.TeacherInfo);
        questionnaire.Availability.AddRange(mapper.Map<List<AvailabilitySlot>>(form.Availability));

        var skills = await ResolveSkillsAsync(db, form.Skills, ct);
        foreach (var input in form.Skills)
        {
            questionnaire.Skills.Add(new QuestionnaireSkill
            {
                QuestionnaireId = questionnaire.Id, SkillId = skills[input.Name.Trim()].Id, SkillLevel = input.SkillLevel
            });
        }

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    }

    private static async Task<Dictionary<string, Skill>> ResolveSkillsAsync(TutorDbContext db, List<FormSkillDto> inputs, CancellationToken ct)
    {
        var names = inputs.Select(x => x.Name.Trim()).ToArray();
        var keys = names.Select(name => name.ToUpperInvariant()).ToArray();
        async Task<Dictionary<string, Skill>> LoadAsync()
        {
            var existing = await db.Set<Skill>().Where(s => names.Contains(s.Name.Trim()) ||
                    keys.Contains(EF.Functions.Collate(s.Name.Trim(), "und-x-icu").ToUpper()))
                .OrderBy(s => s.Id).ToListAsync(ct);
            return existing.GroupBy(s => s.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        }

        var skills = await LoadAsync();
        if (names.All(skills.ContainsKey))
            return skills;

        await db.Database.ExecuteSqlRawAsync("LOCK TABLE skills IN SHARE ROW EXCLUSIVE MODE", ct);
        skills = await LoadAsync();
        foreach (var name in names.Where(name => !skills.ContainsKey(name)))
        {
            var skill = new Skill { Name = name };
            db.Add(skill);
            skills.Add(name, skill);
        }
        return skills;
    }
}
