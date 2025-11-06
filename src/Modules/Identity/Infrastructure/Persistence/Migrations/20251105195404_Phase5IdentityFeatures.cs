using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase5IdentityFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_invitations",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    domain_id = table.Column<Guid>(type: "uuid", nullable: true),
                    role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    invitation_token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    accepted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_invitations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_invitations_domain_id",
                schema: "identity",
                table: "user_invitations",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_invitations_email",
                schema: "identity",
                table: "user_invitations",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_user_invitations_pending",
                schema: "identity",
                table: "user_invitations",
                columns: new[] { "email", "is_used", "is_revoked", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_invitations_tenant_id",
                schema: "identity",
                table: "user_invitations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ux_user_invitations_token",
                schema: "identity",
                table: "user_invitations",
                column: "invitation_token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_invitations",
                schema: "identity");
        }
    }
}
