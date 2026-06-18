using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_outbox_messages",
                table: "outbox_messages");

            migrationBuilder.AddPrimaryKey(
                name: "pk_outbox_messages",
                table: "outbox_messages",
                column: "id");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    avatar_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            // Reusable trigger function that stamps updated_at on every row update.
            // Created here, in the first migration to introduce a mutable table; every
            // later mutable table attaches a BEFORE UPDATE trigger to this same function.
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION set_updated_at()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.updated_at = now();
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                """);

            migrationBuilder.Sql(
                """
                CREATE TRIGGER trg_users_updated_at
                BEFORE UPDATE ON users
                FOR EACH ROW
                EXECUTE FUNCTION set_updated_at();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_users_updated_at ON users;");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS set_updated_at();");

            migrationBuilder.DropPrimaryKey(
                name: "pk_outbox_messages",
                table: "outbox_messages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_outbox_messages",
                table: "outbox_messages",
                column: "id");
        }
    }
}
