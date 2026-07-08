using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganisations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "organisations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisations", x => x.id);
                    table.CheckConstraint("chk_organisations_name", "char_length(name) BETWEEN 2 AND 100");
                    table.CheckConstraint("chk_organisations_slug", "slug ~ '^[a-z0-9-]+$'");
                });

            migrationBuilder.CreateTable(
                name: "memberships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    invited_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_memberships", x => x.id);
                    table.CheckConstraint("chk_memberships_role", "role IN ('owner', 'admin', 'member', 'guest')");
                    table.ForeignKey(
                        name: "fk_memberships_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_memberships_organisation_id",
                table: "memberships",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_memberships_organisation_id_role",
                table: "memberships",
                columns: new[] { "organisation_id", "role" });

            migrationBuilder.CreateIndex(
                name: "ix_memberships_user_id",
                table: "memberships",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_memberships_user_id_organisation_id",
                table: "memberships",
                columns: new[] { "user_id", "organisation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisations_active",
                table: "organisations",
                column: "id",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_organisations_owner_id",
                table: "organisations",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisations_slug",
                table: "organisations",
                column: "slug",
                unique: true);

            // Cross-module foreign keys to the Identity-owned users table. The Organisations
            // module holds no code reference to Identity, so EF cannot model these; they are
            // added in raw SQL to preserve referential integrity at the database level.
            migrationBuilder.Sql(
                """
                ALTER TABLE organisations
                    ADD CONSTRAINT fk_organisations_users_owner_id
                    FOREIGN KEY (owner_id) REFERENCES users (id) ON DELETE RESTRICT;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE memberships
                    ADD CONSTRAINT fk_memberships_users_user_id
                    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE memberships
                    ADD CONSTRAINT fk_memberships_users_invited_by_id
                    FOREIGN KEY (invited_by_id) REFERENCES users (id) ON DELETE SET NULL;
                """);

            // updated_at is database-managed: attach the shared set_updated_at() trigger
            // (function created in the AddUsers migration) to the organisations table.
            migrationBuilder.Sql(
                """
                CREATE TRIGGER trg_organisations_updated_at
                BEFORE UPDATE ON organisations
                FOR EACH ROW
                EXECUTE FUNCTION set_updated_at();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_organisations_updated_at ON organisations;");

            // The cross-module foreign keys are defined on the organisations and memberships
            // tables, so they are removed automatically when those tables are dropped below.
            migrationBuilder.DropTable(
                name: "memberships");

            migrationBuilder.DropTable(
                name: "organisations");
        }
    }
}
