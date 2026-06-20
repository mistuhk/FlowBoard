using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    invited_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invitations", x => x.id);
                    table.CheckConstraint("chk_invitations_role", "role IN ('admin', 'member', 'guest')");
                    table.ForeignKey(
                        name: "fk_invitations_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_invitations_email",
                table: "invitations",
                column: "invited_email");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_organisation_id",
                table: "invitations",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_pending",
                table: "invitations",
                columns: new[] { "organisation_id", "expires_at" },
                filter: "accepted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_token_hash",
                table: "invitations",
                column: "token_hash",
                unique: true);

            // Cross-module foreign key to the Identity-owned users table. The Organisations module
            // holds no code reference to Identity, so EF cannot model it; it is added in raw SQL.
            migrationBuilder.Sql(
                """
                ALTER TABLE invitations
                    ADD CONSTRAINT fk_invitations_users_invited_by_id
                    FOREIGN KEY (invited_by_id) REFERENCES users (id) ON DELETE RESTRICT;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitations");
        }
    }
}
