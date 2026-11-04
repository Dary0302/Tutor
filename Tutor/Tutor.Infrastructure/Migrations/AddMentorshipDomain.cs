using FluentMigrator;

namespace Tutor.Infrastructure.Migrations;

[Migration(20261102000200, "Add questionnaires, mentorship and learning records")]
public sealed class AddMentorshipDomain : Migration
{
    public override void Up()
    {
        Execute.Sql("""
            ALTER TABLE "Users" ALTER COLUMN "Name" TYPE varchar(255);

            CREATE TABLE skills (
                id uuid PRIMARY KEY,
                name varchar(255) NOT NULL
            );

            CREATE TABLE questionnaires (
                id uuid PRIMARY KEY,
                first_name varchar(255) NOT NULL,
                last_name varchar(255) NOT NULL,
                city varchar(255) NOT NULL,
                education varchar(255) NOT NULL,
                goal_description varchar(255) NOT NULL,
                communication_format varchar(50) NOT NULL CHECK (communication_format IN ('Online', 'InPerson', 'Hybrid')),
                months_number_min integer NOT NULL,
                months_number_max integer NOT NULL,
                hours_number_week integer NOT NULL,
                user_id uuid NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT UNIQUE
            );

            CREATE TABLE questionnaire_skills (
                skill_id uuid NOT NULL REFERENCES skills(id) ON DELETE RESTRICT,
                questionnaires_id uuid NOT NULL REFERENCES questionnaires(id) ON DELETE CASCADE,
                skill_level varchar(50) NOT NULL CHECK (skill_level IN ('Beginner', 'Intermediate', 'Advanced', 'Expert')),
                PRIMARY KEY (skill_id, questionnaires_id)
            );

            CREATE TABLE teacher_info (
                id uuid PRIMARY KEY,
                grade varchar(50) NOT NULL CHECK (grade IN ('Junior', 'Middle', 'Senior', 'Lead')),
                place_of_work varchar(255) NOT NULL,
                position varchar(255) NOT NULL,
                years_experience integer NOT NULL,
                max_students integer NOT NULL,
                teacher_questionnaire_id uuid NOT NULL REFERENCES questionnaires(id) ON DELETE CASCADE UNIQUE
            );

            CREATE TABLE accessibility (
                id uuid PRIMARY KEY,
                questionnaire_id uuid NOT NULL REFERENCES questionnaires(id) ON DELETE CASCADE,
                day_of_week varchar(50) NOT NULL CHECK (day_of_week IN ('Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday')),
                start_time time NOT NULL,
                end_time time NOT NULL
            );

            CREATE TABLE transactions (
                id uuid PRIMARY KEY,
                user_id uuid NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
                points_amount integer NOT NULL,
                balance_amount integer NOT NULL,
                reason varchar(50) NOT NULL CHECK (reason IN ('Reward', 'Payment', 'Refund', 'Adjustment')),
                description text NOT NULL,
                created_at timestamptz NOT NULL
            );

            CREATE TABLE requests_for_mentorship (
                id uuid PRIMARY KEY,
                student_id uuid NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
                teacher_id uuid NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
                student_letter varchar(255) NOT NULL,
                status varchar(50) NOT NULL CHECK (status IN ('Pending', 'Accepted', 'Declined', 'Cancelled')),
                curator_id uuid REFERENCES "Users"("Id") ON DELETE RESTRICT
            );

            CREATE TABLE pairs (
                id uuid PRIMARY KEY,
                student_id uuid NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
                teacher_id uuid NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
                status varchar(50) NOT NULL CHECK (status IN ('Active', 'Completed', 'Terminated')),
                started_at timestamptz NOT NULL,
                ended_at timestamptz,
                end_reason varchar(50) CHECK (end_reason IN ('GoalsAchieved', 'ParticipantRequest', 'Other')),
                ended_by_user_id uuid REFERENCES "Users"("Id") ON DELETE RESTRICT,
                request_id uuid NOT NULL REFERENCES requests_for_mentorship(id) ON DELETE RESTRICT UNIQUE
            );

            CREATE TABLE goals (
                id uuid PRIMARY KEY,
                title varchar(255) NOT NULL,
                description varchar(255) NOT NULL,
                status varchar(50) NOT NULL CHECK (status IN ('Active', 'Completed', 'Cancelled')),
                created_at timestamptz NOT NULL,
                closed_at timestamptz,
                skill_id uuid NOT NULL REFERENCES skills(id) ON DELETE RESTRICT,
                pair_id uuid NOT NULL REFERENCES pairs(id) ON DELETE RESTRICT
            );

            CREATE TABLE tasks (
                id uuid PRIMARY KEY,
                name varchar(255) NOT NULL,
                status varchar(50) NOT NULL CHECK (status IN ('Pending', 'InProgress', 'Completed')),
                completed_at timestamptz,
                goal_id uuid NOT NULL REFERENCES goals(id) ON DELETE CASCADE
            );

            CREATE TABLE sessions (
                id uuid PRIMARY KEY,
                starts_at timestamptz NOT NULL,
                ends_at timestamptz NOT NULL,
                agenda varchar(255) NOT NULL,
                status varchar(50) NOT NULL CHECK (status IN ('Scheduled', 'Completed', 'Cancelled')),
                cancelled_at timestamptz,
                cancellation_reason varchar(50) CHECK (cancellation_reason IN ('StudentRequest', 'TeacherRequest', 'Other')),
                pair_id uuid NOT NULL REFERENCES pairs(id) ON DELETE RESTRICT
            );

            CREATE INDEX ix_questionnaire_skills_questionnaires_id ON questionnaire_skills (questionnaires_id);
            CREATE INDEX ix_accessibility_questionnaire_id ON accessibility (questionnaire_id);
            CREATE INDEX ix_transactions_user_id ON transactions (user_id);
            CREATE INDEX ix_requests_for_mentorship_student_id ON requests_for_mentorship (student_id);
            CREATE INDEX ix_requests_for_mentorship_teacher_id ON requests_for_mentorship (teacher_id);
            CREATE INDEX ix_requests_for_mentorship_curator_id ON requests_for_mentorship (curator_id);
            CREATE INDEX ix_pairs_student_id ON pairs (student_id);
            CREATE INDEX ix_pairs_teacher_id ON pairs (teacher_id);
            CREATE INDEX ix_pairs_ended_by_user_id ON pairs (ended_by_user_id);
            CREATE INDEX ix_goals_skill_id ON goals (skill_id);
            CREATE INDEX ix_goals_pair_id ON goals (pair_id);
            CREATE INDEX ix_tasks_goal_id ON tasks (goal_id);
            CREATE INDEX ix_sessions_pair_id ON sessions (pair_id);
            """);
    }

    public override void Down()
    {
        Execute.Sql("""
            DROP TABLE sessions, tasks, goals, pairs, requests_for_mentorship, transactions, accessibility, teacher_info, questionnaire_skills, questionnaires, skills;
            ALTER TABLE "Users" ALTER COLUMN "Name" TYPE varchar(100);
            """);
    }
}
