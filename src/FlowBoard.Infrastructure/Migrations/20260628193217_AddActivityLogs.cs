using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activity_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_activity_logs_actor_id",
                table: "activity_logs",
                columns: new[] { "actor_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_activity_logs_entity",
                table: "activity_logs",
                columns: new[] { "organisation_id", "entity_type", "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_activity_logs_organisation_id",
                table: "activity_logs",
                columns: new[] { "organisation_id", "created_at" });

            // Cross-module foreign keys (raw SQL: EF cannot model references into other modules).
            // The organisation cascade-deletes its logs; the actor is nulled if the user is removed.
            migrationBuilder.Sql(
                """
                ALTER TABLE activity_logs
                    ADD CONSTRAINT fk_activity_logs_organisations_organisation_id
                    FOREIGN KEY (organisation_id) REFERENCES organisations (id) ON DELETE CASCADE;
                """);
            migrationBuilder.Sql(
                """
                ALTER TABLE activity_logs
                    ADD CONSTRAINT fk_activity_logs_users_actor_id
                    FOREIGN KEY (actor_id) REFERENCES users (id) ON DELETE SET NULL;
                """);

            // Row-Level Security: tenant isolation as defence-in-depth (not FORCED yet). Append-only
            // table: no updated_at trigger.
            migrationBuilder.Sql("ALTER TABLE activity_logs ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(
                """
                CREATE POLICY tenant_isolation_activity_logs ON activity_logs
                    USING (organisation_id = current_setting('app.current_organisation_id', true)::uuid);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_activity_logs ON activity_logs;");

            migrationBuilder.DropTable(
                name: "activity_logs");
        }
    }
}
