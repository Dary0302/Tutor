using FluentMigrator;

namespace Tutor.Infrastructure.Migrations;

[Migration(20261122000100, "Index mentoring sessions by pair and start time")]
public sealed class AddSessionCalendarIndex : ForwardOnlyMigration
{
    public override void Up() =>
        Execute.Sql("CREATE INDEX ix_sessions_pair_starts_at ON sessions (pair_id, starts_at);");
}
