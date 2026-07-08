using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFileAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "file_attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    mime_type = table.Column<string>(type: "text", nullable: false),
                    storage_key = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_attachments", x => x.id);
                    table.CheckConstraint("chk_file_attachments_size", "file_size_bytes > 0");
                    table.ForeignKey(
                        name: "fk_file_attachments_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_file_attachments_organisation_id",
                table: "file_attachments",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_file_attachments_task_id",
                table: "file_attachments",
                column: "task_id",
                filter: "deleted_at IS NULL");

            // Cross-module foreign keys to the Identity-owned users table and organisations, in raw SQL.
            migrationBuilder.Sql(
                """
                ALTER TABLE file_attachments
                    ADD CONSTRAINT fk_file_attachments_users_uploaded_by_id
                    FOREIGN KEY (uploaded_by_id) REFERENCES users (id) ON DELETE RESTRICT;
                """);
            migrationBuilder.Sql(
                """
                ALTER TABLE file_attachments
                    ADD CONSTRAINT fk_file_attachments_organisations_organisation_id
                    FOREIGN KEY (organisation_id) REFERENCES organisations (id);
                """);

            // Row-Level Security: tenant isolation as defence-in-depth (not FORCED yet). Metadata-only
            // table: no updated_at trigger.
            migrationBuilder.Sql("ALTER TABLE file_attachments ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(
                """
                CREATE POLICY tenant_isolation_file_attachments ON file_attachments
                    USING (organisation_id = current_setting('app.current_organisation_id', true)::uuid);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_file_attachments ON file_attachments;");

            migrationBuilder.DropTable(
                name: "file_attachments");
        }
    }
}
