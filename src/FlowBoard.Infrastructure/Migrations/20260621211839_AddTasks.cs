using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    assignee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tasks", x => x.id);
                    table.CheckConstraint("chk_tasks_priority", "priority IN ('low', 'medium', 'high', 'critical')");
                    table.CheckConstraint("chk_tasks_status", "status IN ('todo', 'in_progress', 'blocked', 'done')");
                    table.CheckConstraint("chk_tasks_title", "char_length(title) BETWEEN 1 AND 255");
                });

            migrationBuilder.CreateIndex(
                name: "ix_tasks_assignee_id",
                table: "tasks",
                column: "assignee_id",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_due_date",
                table: "tasks",
                column: "due_date",
                filter: "due_date IS NOT NULL AND deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_organisation_id",
                table: "tasks",
                column: "organisation_id",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_organisation_id_priority",
                table: "tasks",
                columns: new[] { "organisation_id", "priority" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_organisation_id_status",
                table: "tasks",
                columns: new[] { "organisation_id", "status" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_project_id",
                table: "tasks",
                column: "project_id",
                filter: "deleted_at IS NULL");

            // Cross-module foreign keys (projects and the Identity-owned users table), added in raw SQL.
            migrationBuilder.Sql(
                """
                ALTER TABLE tasks
                    ADD CONSTRAINT fk_tasks_projects_project_id
                    FOREIGN KEY (project_id) REFERENCES projects (id) ON DELETE CASCADE;
                """);
            migrationBuilder.Sql(
                """
                ALTER TABLE tasks
                    ADD CONSTRAINT fk_tasks_organisations_organisation_id
                    FOREIGN KEY (organisation_id) REFERENCES organisations (id);
                """);
            migrationBuilder.Sql(
                """
                ALTER TABLE tasks
                    ADD CONSTRAINT fk_tasks_users_assignee_id
                    FOREIGN KEY (assignee_id) REFERENCES users (id) ON DELETE SET NULL;
                """);
            migrationBuilder.Sql(
                """
                ALTER TABLE tasks
                    ADD CONSTRAINT fk_tasks_users_created_by_id
                    FOREIGN KEY (created_by_id) REFERENCES users (id) ON DELETE RESTRICT;
                """);

            // Full-text search vector: a stored generated column kept in sync with title/description,
            // plus a GIN index. Used by Sprint 7 search. Managed by the database, not the EF model.
            migrationBuilder.Sql(
                """
                ALTER TABLE tasks ADD COLUMN search_vector tsvector
                    GENERATED ALWAYS AS (
                        to_tsvector('english', coalesce(title, '') || ' ' || coalesce(description, ''))
                    ) STORED;
                """);
            migrationBuilder.Sql("CREATE INDEX idx_tasks_search ON tasks USING GIN (search_vector);");

            // updated_at is database-managed: attach the shared set_updated_at() trigger.
            migrationBuilder.Sql(
                """
                CREATE TRIGGER trg_tasks_updated_at
                BEFORE UPDATE ON tasks
                FOR EACH ROW
                EXECUTE FUNCTION set_updated_at();
                """);

            // Row-Level Security: tenant isolation as defence-in-depth on top of repository scoping.
            // Not FORCED yet (the app connects as the table owner); enforcement engages with a
            // restricted database role.
            migrationBuilder.Sql("ALTER TABLE tasks ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(
                """
                CREATE POLICY tenant_isolation_tasks ON tasks
                    USING (organisation_id = current_setting('app.current_organisation_id', true)::uuid);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_tasks ON tasks;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_tasks_updated_at ON tasks;");

            migrationBuilder.DropTable(
                name: "tasks");
        }
    }
}
