using Microsoft.EntityFrameworkCore;
using Tutor.Domain.Questionnaires;
using Tutor.Domain.Users;

namespace Tutor.Infrastructure.Configurations;

internal static class QuestionnairesMapping
{
    public static void Configure(ModelBuilder model)
    {
        ConfigureSkill(model);
        ConfigureQuestionnaire(model);
        ConfigureQuestionnaireSkill(model);
        ConfigureTeacherInfo(model);
        ConfigureAvailabilitySlot(model);
    }

    private static void ConfigureSkill(ModelBuilder model)
    {
        var entity = model.Entity<Skill>();
        entity.ToTable("skills");
        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(255);
    }

    private static void ConfigureQuestionnaire(ModelBuilder model)
    {
        var entity = model.Entity<Questionnaire>();
        entity.ToTable("questionnaires");
        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(255);
        entity.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(255);
        entity.Property(x => x.City).HasColumnName("city").HasMaxLength(255);
        entity.Property(x => x.Education).HasColumnName("education").HasMaxLength(255);
        entity.Property(x => x.GoalDescription).HasColumnName("goal_description").HasMaxLength(255);
        entity.Property(x => x.CommunicationFormat).HasColumnName("communication_format").HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.MonthsNumberMin).HasColumnName("months_number_min");
        entity.Property(x => x.MonthsNumberMax).HasColumnName("months_number_max");
        entity.Property(x => x.HoursNumberWeek).HasColumnName("hours_number_week");
        entity.Property(x => x.UserId).HasColumnName("user_id");
        entity.HasOne<User>().WithOne().HasForeignKey<Questionnaire>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureQuestionnaireSkill(ModelBuilder model)
    {
        var entity = model.Entity<QuestionnaireSkill>();
        entity.ToTable("questionnaire_skills");
        entity.HasKey(x => new { x.SkillId, x.QuestionnaireId });
        entity.Property(x => x.SkillId).HasColumnName("skill_id");
        entity.HasOne(x => x.Skill).WithMany().HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.QuestionnaireId).HasColumnName("questionnaires_id");
        entity.HasOne<Questionnaire>().WithMany(q => q.Skills).HasForeignKey(x => x.QuestionnaireId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.Property(x => x.SkillLevel).HasColumnName("skill_level").HasConversion<string>().HasMaxLength(50);
    }

    private static void ConfigureTeacherInfo(ModelBuilder model)
    {
        var entity = model.Entity<TeacherInfo>();
        entity.ToTable("teacher_info");
        entity.HasKey(x => x.TeacherQuestionnaireId);
        entity.Property(x => x.PlaceOfWork).HasColumnName("place_of_work").HasMaxLength(255);
        entity.Property(x => x.Position).HasColumnName("position").HasMaxLength(255);
        entity.Property(x => x.YearsExperience).HasColumnName("years_experience");
        entity.Property(x => x.MaxStudents).HasColumnName("max_students");
        entity.Property(x => x.TeacherQuestionnaireId).HasColumnName("teacher_questionnaire_id").ValueGeneratedNever();
        entity.HasOne<Questionnaire>().WithOne(q => q.TeacherInfo).HasForeignKey<TeacherInfo>(x => x.TeacherQuestionnaireId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureAvailabilitySlot(ModelBuilder model)
    {
        var entity = model.Entity<AvailabilitySlot>();
        entity.ToTable("accessibility");
        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.QuestionnaireId).HasColumnName("questionnaire_id");
        entity.HasOne<Questionnaire>().WithMany(q => q.Availability).HasForeignKey(x => x.QuestionnaireId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.Property(x => x.DayOfWeek).HasColumnName("day_of_week").HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.StartTime).HasColumnName("start_time");
        entity.Property(x => x.EndTime).HasColumnName("end_time");
    }
}
