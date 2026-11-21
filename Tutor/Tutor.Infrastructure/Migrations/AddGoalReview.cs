using FluentMigrator;

namespace Tutor.Infrastructure.Migrations;

[Migration(20261114000200, "Add goal review status and task ordering")]
public sealed class AddGoalReview : ForwardOnlyMigration
{
    public override void Up()
    {
        Execute.Sql("""
            ALTER TABLE goals DROP CONSTRAINT goals_status_check;
            ALTER TABLE goals ADD CONSTRAINT goals_status_check
                CHECK (status IN ('Active', 'PendingReview', 'Completed', 'Cancelled'));

            ALTER TABLE tasks ADD COLUMN position integer NOT NULL DEFAULT 0;
            WITH ordered AS (
                SELECT id, (row_number() OVER (PARTITION BY goal_id ORDER BY id) - 1)::integer AS position
                FROM tasks
            )
            UPDATE tasks SET position = ordered.position FROM ordered WHERE tasks.id = ordered.id;

            UPDATE goals g SET status = 'PendingReview', closed_at = NULL
            WHERE g.status = 'Active'
                AND EXISTS (SELECT 1 FROM tasks t WHERE t.goal_id = g.id)
                AND NOT EXISTS (SELECT 1 FROM tasks t WHERE t.goal_id = g.id AND t.status <> 'Completed');
            """);
    }
}
