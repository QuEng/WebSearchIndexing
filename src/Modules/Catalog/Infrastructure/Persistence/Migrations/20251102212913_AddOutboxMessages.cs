using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UrlBatches_Sites_SiteId",
                table: "UrlBatches");

            migrationBuilder.DropIndex(
                name: "IX_UrlBatches_SiteId",
                table: "UrlBatches");

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    data = table.Column<string>(type: "text", nullable: false),
                    occurred_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    error = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_occurred_on",
                table: "outbox_messages",
                column: "occurred_on");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_status",
                table: "outbox_messages",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_tenant_status_occurred",
                table: "outbox_messages",
                columns: new[] { "tenant_id", "status", "occurred_on" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.CreateIndex(
                name: "IX_UrlBatches_SiteId",
                table: "UrlBatches",
                column: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_UrlBatches_Sites_SiteId",
                table: "UrlBatches",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
