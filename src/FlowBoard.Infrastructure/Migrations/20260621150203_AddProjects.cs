using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                    table.CheckConstraint("chk_projects_name", "char_length(name) BETWEEN 1 AND 150");
                    table.CheckConstraint("chk_projects_status", "status IN ('active', 'archived')");
                });

            migrationBuilder.CreateIndex(
                name: "ix_projects_active",
                table: "projects",
                column: "id",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_projects_organisation_id",
                table: "projects",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_organisation_id_status",
                table: "projects",
                columns: new[] { "organisation_id", "status" },
                filter: "deleted_at IS NULL");

            // Foreign keys. organisation_id is within the schema but cross-module in code;
            // created_by_id references the Identity-owned users table. Added in raw SQL.
            migrationBuilder.Sql(
                """
                ALTER TABLE projects
                    ADD CONSTRAINT fk_projects_organisations_organisation_id
                    FOREIGN KEY (organisation_id) REFERENCES organisations (id) ON DELETE CASCADE;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE projects
                    ADD CONSTRAINT fk_projects_users_created_by_id
                    FOREIGN KEY (created_by_id) REFERENCES users (id) ON DELETE RESTRICT;
                """);

            // updated_at is database-managed: attach the shared set_updated_at() trigger.
            migrationBuilder.Sql(
                """
                CREATE TRIGGER trg_projects_updated_at
                BEFORE UPDATE ON projects
                FOR EACH ROW
                EXECUTE FUNCTION set_updated_at();
                """);

            // Row-Level Security: tenant isolation as defence-in-depth on top of the repository's
            // organisation scoping. The session variable app.current_organisation_id is set inside
            // command transactions by the unit of work; current_setting(..., true) yields NULL when
            // unset (matching no rows) rather than erroring. Not FORCED, so the table-owning
            // application role is not yet subject to it; enforcement engages once a restricted
            // (non-owner) database role is provisioned.
            migrationBuilder.Sql("ALTER TABLE projects ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(
                """
                CREATE POLICY tenant_isolation_projects ON projects
                    USING (organisation_id = current_setting('app.current_organisation_id', true)::uuid);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_projects ON projects;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_projects_updated_at ON projects;");

            migrationBuilder.DropTable(
                name: "projects");
        }
    }
}
