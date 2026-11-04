using FluentMigrator;

namespace Tutor.Infrastructure.Migrations;

[Migration(20261103000100, "Use questionnaire keys for teachers, derive pair members from requests and add articles")]
public sealed class UpdateMentorshipSchema : ForwardOnlyMigration
{
    public override void Up()
    {
        Execute.Sql("""
            LOCK TABLE pairs, requests_for_mentorship IN SHARE ROW EXCLUSIVE MODE;
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM pairs p
                    JOIN requests_for_mentorship r ON r.id = p.request_id
                    WHERE p.student_id <> r.student_id OR p.teacher_id <> r.teacher_id
                ) THEN
                    RAISE EXCEPTION 'Resolve mismatched student_id or teacher_id between pairs and their requests before migrating.';
                END IF;
            END $$;

            ALTER TABLE pairs DROP COLUMN student_id, DROP COLUMN teacher_id;

            ALTER TABLE teacher_info
                DROP COLUMN id,
                DROP COLUMN grade,
                DROP CONSTRAINT teacher_info_teacher_questionnaire_id_key,
                ADD PRIMARY KEY (teacher_questionnaire_id);

            CREATE TABLE articles (
                id uuid PRIMARY KEY,
                author uuid NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
                title varchar(255) NOT NULL,
                s3_key varchar(255) NOT NULL
            );
            CREATE INDEX ix_articles_author ON articles (author);
            """);
    }
}
