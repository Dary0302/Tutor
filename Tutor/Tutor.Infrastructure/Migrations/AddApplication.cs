using FluentMigrator;

namespace Tutor.Infrastructure.Migrations;

[Migration(20261115000103, "Add Application")]
public sealed class AddApplication: ForwardOnlyMigration
{
    public override void Up()
    {
        Execute.Sql("""
                    CREATE TABLE applications (
                    id uuid PRIMARY KEY,
                    student uuid NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
                    mentor uuid NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
                    "mentorsAproove" boolean,
                    "tutorsAproove" boolean,
                    "reasonForRefusal" varchar(255));
                    """);
    }
}