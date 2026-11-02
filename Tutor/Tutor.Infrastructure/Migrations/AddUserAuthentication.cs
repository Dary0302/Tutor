using FluentMigrator;

namespace Tutor.Infrastructure.Migrations;

[Migration(20261102000100, "Move user authentication into Tutor")]
public sealed class AddUserAuthentication : ForwardOnlyMigration
{
    public override void Up()
    {
        // Use the existing Auth table in place when DefaultConnection points at its database.
        Execute.Sql("""
            CREATE TABLE IF NOT EXISTS "Users" (
                "Id" uuid PRIMARY KEY,
                "Name" varchar(100) NOT NULL,
                "Password" text NOT NULL,
                "Role" varchar(50) NOT NULL,
                "TotalBalance" integer NOT NULL,
                "TotalPoints" integer NOT NULL,
                "RefreshToken" uuid NOT NULL,
                "CreatedAt" timestamptz NOT NULL,
                "UpdatedAt" timestamptz,
                "DeletedAt" timestamptz
            );

            ALTER TABLE "Users" RENAME COLUMN "RefreshToken" TO "RefreshTokenHash";
            ALTER TABLE "Users"
                ALTER COLUMN "RefreshTokenHash" TYPE bytea USING sha256(uuid_send("RefreshTokenHash")),
                ALTER COLUMN "RefreshTokenHash" DROP NOT NULL,
                ADD COLUMN "RefreshTokenExpiresAt" timestamptz;
            UPDATE "Users" SET "RefreshTokenExpiresAt" = now() + interval '30 days';

            CREATE UNIQUE INDEX users_email_unique ON "Users" ("Name");
            CREATE UNIQUE INDEX users_refresh_token_unique ON "Users" ("RefreshTokenHash");
            """);
    }
}
