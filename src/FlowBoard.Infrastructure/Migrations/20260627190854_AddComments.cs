using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comments", x => x.id);
                    table.CheckConstraint("chk_comments_content", "char_length(content) BETWEEN 1 AND 10000");
                    table.ForeignKey(
                        name: "fk_comments_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_comments_author_id",
                table: "comments",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_organisation_id",
                table: "comments",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_task_id",
                table: "comments",
                column: "task_id",
                filter: "deleted_at IS NULL");

            // Cross-module foreign keys to the Identity-owned users table and organisations, in raw SQL.
            migrationBuilder.Sql(
                """
                ALTER TABLE comments
                    ADD CONSTRAINT fk_comments_users_author_id
                    FOREIGN KEY (author_id) REFERENCES users (id) ON DELETE RESTRICT;
                """);
            migrationBuilder.Sql(
                """
                ALTER TABLE comments
                    ADD CONSTRAINT fk_comments_organisations_organisation_id
                    FOREIGN KEY (organisation_id) REFERENCES organisations (id);
                """);

            // updated_at is database-managed: attach the shared set_updated_at() trigger.
            migrationBuilder.Sql(
                """
                CREATE TRIGGER trg_comments_updated_at
                BEFORE UPDATE ON comments
                FOR EACH ROW
                EXECUTE FUNCTION set_updated_at();
                """);

            // Row-Level Security: tenant isolation as defence-in-depth (not FORCED yet).
            migrationBuilder.Sql("ALTER TABLE comments ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(
                """
                CREATE POLICY tenant_isolation_comments ON comments
                    USING (organisation_id = current_setting('app.current_organisation_id', true)::uuid);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_comments ON comments;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_comments_updated_at ON comments;");

            migrationBuilder.DropTable(
                name: "comments");
        }
    }
}
