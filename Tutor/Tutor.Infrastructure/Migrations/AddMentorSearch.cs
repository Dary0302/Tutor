using FluentMigrator;

namespace Tutor.Infrastructure.Migrations;

[Migration(20261114000100, "Support mentor ratings, fractional weekly hours and search indexes")]
public sealed class AddMentorSearch : ForwardOnlyMigration
{
    public override void Up()
    {
        Execute.Sql("""
            ALTER TABLE teacher_info ADD COLUMN rating numeric(3, 2) NULL
                CONSTRAINT ck_teacher_info_rating CHECK (rating BETWEEN 0 AND 5);
            ALTER TABLE questionnaires ALTER COLUMN hours_number_week TYPE numeric;
            CREATE INDEX ix_teacher_info_years_experience ON teacher_info (years_experience);
            CREATE INDEX ix_teacher_info_rating ON teacher_info (rating);
            CREATE INDEX ix_questionnaires_hours_number_week ON questionnaires (hours_number_week);
            CREATE INDEX ix_accessibility_questionnaire_day ON accessibility (questionnaire_id, day_of_week);
            CREATE INDEX ix_pairs_active_request ON pairs (request_id) WHERE status = 'Active';
            """);
    }
}
