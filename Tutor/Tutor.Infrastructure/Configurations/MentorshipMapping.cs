using Microsoft.EntityFrameworkCore;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Users;
using Tutor.Domain.Questionnaires;

namespace Tutor.Infrastructure.Configurations;

internal static class MentorshipMapping
{
    public static void Configure(ModelBuilder model)
    {
        ConfigureMentorshipRequest(model);
        ConfigureMentorshipPair(model);
        ConfigureLearningGoal(model);
        ConfigureGoalTask(model);
        ConfigureMentoringSession(model);
    }

    private static void ConfigureMentorshipRequest(ModelBuilder model)
    {
        var entity = model.Entity<MentorshipRequest>();
        entity.ToTable("requests_for_mentorship");
        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.StudentId).HasColumnName("student_id");
        entity.HasOne<User>().WithMany().HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.TeacherId).HasColumnName("teacher_id");
        entity.HasOne<User>().WithMany().HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.StudentLetter).HasColumnName("student_letter").HasMaxLength(255);
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.CuratorId).HasColumnName("curator_id");
        entity.HasOne<User>().WithMany().HasForeignKey(x => x.CuratorId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureMentorshipPair(ModelBuilder model)
    {
        var entity = model.Entity<MentorshipPair>();
        entity.ToTable("pairs");
        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.StartedAt).HasColumnName("started_at");
        entity.Property(x => x.EndedAt).HasColumnName("ended_at");
        entity.Property(x => x.EndReason).HasColumnName("end_reason").HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.EndedByUserId).HasColumnName("ended_by_user_id");
        entity.HasOne<User>().WithMany().HasForeignKey(x => x.EndedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.RequestId).HasColumnName("request_id");
        entity.HasOne<MentorshipRequest>().WithOne().HasForeignKey<MentorshipPair>(x => x.RequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureLearningGoal(ModelBuilder model)
    {
        var entity = model.Entity<LearningGoal>();
        entity.ToTable("goals");
        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(255);
        entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.ClosedAt).HasColumnName("closed_at");
        entity.Property(x => x.SkillId).HasColumnName("skill_id");
        entity.HasOne<Skill>().WithMany().HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.PairId).HasColumnName("pair_id");
        entity.HasOne<MentorshipPair>().WithMany().HasForeignKey(x => x.PairId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureGoalTask(ModelBuilder model)
    {
        var entity = model.Entity<GoalTask>();
        entity.ToTable("tasks");
        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(255);
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
        entity.Property(x => x.GoalId).HasColumnName("goal_id");
        entity.HasOne<LearningGoal>().WithMany().HasForeignKey(x => x.GoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureMentoringSession(ModelBuilder model)
    {
        var entity = model.Entity<MentoringSession>();
        entity.ToTable("sessions");
        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.StartsAt).HasColumnName("starts_at");
        entity.Property(x => x.EndsAt).HasColumnName("ends_at");
        entity.Property(x => x.Agenda).HasColumnName("agenda").HasMaxLength(255);
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.CancelledAt).HasColumnName("cancelled_at");
        entity.Property(x => x.CancellationReason).HasColumnName("cancellation_reason").HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.PairId).HasColumnName("pair_id");
        entity.HasOne<MentorshipPair>().WithMany().HasForeignKey(x => x.PairId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
