using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: true),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.CheckConstraint("chk_notifications_type", "type IN ('task_assigned', 'user_mentioned', 'project_invited', 'task_status_changed', 'comment_added', 'task_blocked')");
                });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                table: "notifications",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_unread",
                table: "notifications",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true },
                filter: "is_read = false");

            // Cross-module foreign keys to users and organisations, added in raw SQL.
            migrationBuilder.Sql(
                """
                ALTER TABLE notifications
                    ADD CONSTRAINT fk_notifications_users_user_id
                    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE;
                """);
            migrationBuilder.Sql(
                """
                ALTER TABLE notifications
                    ADD CONSTRAINT fk_notifications_organisations_organisation_id
                    FOREIGN KEY (organisation_id) REFERENCES organisations (id) ON DELETE CASCADE;
                """);

            // Row-Level Security: tenant isolation as defence-in-depth (not FORCED yet; engages with
            // a restricted database role).
            migrationBuilder.Sql("ALTER TABLE notifications ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(
                """
                CREATE POLICY tenant_isolation_notifications ON notifications
                    USING (organisation_id = current_setting('app.current_organisation_id', true)::uuid);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_notifications ON notifications;");

            migrationBuilder.DropTable(
                name: "notifications");
        }
    }
}
