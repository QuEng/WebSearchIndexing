using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenAndSecurityFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "login_histories",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_successful = table.Column<bool>(type: "boolean", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    device_info = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_histories", x => x.id);
                    table.ForeignKey(
                        name: "FK_login_histories_Users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_login_histories_ip_login_at",
                schema: "identity",
                table: "login_histories",
                columns: new[] { "ip_address", "login_at" });

            migrationBuilder.CreateIndex(
                name: "ix_login_histories_login_at",
                schema: "identity",
                table: "login_histories",
                column: "login_at");

            migrationBuilder.CreateIndex(
                name: "ix_login_histories_user_id",
                schema: "identity",
                table: "login_histories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_login_histories_user_login_at",
                schema: "identity",
                table: "login_histories",
                columns: new[] { "user_id", "login_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "login_histories",
                schema: "identity");
        }
    }
}
