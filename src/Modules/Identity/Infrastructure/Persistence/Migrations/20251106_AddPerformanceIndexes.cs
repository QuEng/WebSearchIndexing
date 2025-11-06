using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Users table - optimize authentication queries
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_IsActive",
                schema: "identity",
                table: "Users",
                columns: new[] { "Email", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsEmailVerified",
                schema: "identity",
                table: "Users",
                column: "IsEmailVerified");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastLoginAt",
                schema: "identity",
                table: "Users",
                column: "LastLoginAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastPasswordChangeAt",
                schema: "identity",
                table: "Users",
                column: "LastPasswordChangeAt");

            // RefreshTokens table - optimize token lookup and cleanup
            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt_IsRevoked",
                schema: "identity",
                table: "RefreshTokens",
                columns: new[] { "ExpiresAt", "IsRevoked" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked",
                schema: "identity",
                table: "RefreshTokens",
                columns: new[] { "UserId", "IsRevoked" });

            // Roles table - optimize role queries
            migrationBuilder.CreateIndex(
                name: "IX_Roles_Type_Name",
                schema: "identity",
                table: "Roles",
                columns: new[] { "Type", "Name" });

            // UserTenants table - optimize tenant switching queries
            migrationBuilder.CreateIndex(
                name: "IX_UserTenants_UserId_TenantId_IsActive",
                schema: "identity",
                table: "UserTenants",
                columns: new[] { "UserId", "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTenants_TenantId_IsActive",
                schema: "identity",
                table: "UserTenants",
                columns: new[] { "TenantId", "IsActive" });

            // UserDomains table - optimize domain access queries
            migrationBuilder.CreateIndex(
                name: "IX_UserDomains_UserId_DomainId_IsActive",
                schema: "identity",
                table: "UserDomains",
                columns: new[] { "UserId", "DomainId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDomains_DomainId_IsActive",
                schema: "identity",
                table: "UserDomains",
                columns: new[] { "DomainId", "IsActive" });

            // UserInvitations table - optimize invitation lookups
            migrationBuilder.CreateIndex(
                name: "IX_UserInvitations_Email_Status",
                schema: "identity",
                table: "UserInvitations",
                columns: new[] { "Email", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UserInvitations_ExpiresAt_Status",
                schema: "identity",
                table: "UserInvitations",
                columns: new[] { "ExpiresAt", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop Users indexes
            migrationBuilder.DropIndex(
                name: "IX_Users_Email_IsActive",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsEmailVerified",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastLoginAt",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastPasswordChangeAt",
                schema: "identity",
                table: "Users");

            // Drop RefreshTokens indexes
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_ExpiresAt_IsRevoked",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked",
                schema: "identity",
                table: "RefreshTokens");

            // Drop Roles indexes
            migrationBuilder.DropIndex(
                name: "IX_Roles_Type_Name",
                schema: "identity",
                table: "Roles");

            // Drop UserTenants indexes
            migrationBuilder.DropIndex(
                name: "IX_UserTenants_UserId_TenantId_IsActive",
                schema: "identity",
                table: "UserTenants");

            migrationBuilder.DropIndex(
                name: "IX_UserTenants_TenantId_IsActive",
                schema: "identity",
                table: "UserTenants");

            // Drop UserDomains indexes
            migrationBuilder.DropIndex(
                name: "IX_UserDomains_UserId_DomainId_IsActive",
                schema: "identity",
                table: "UserDomains");

            migrationBuilder.DropIndex(
                name: "IX_UserDomains_DomainId_IsActive",
                schema: "identity",
                table: "UserDomains");

            // Drop UserInvitations indexes
            migrationBuilder.DropIndex(
                name: "IX_UserInvitations_Email_Status",
                schema: "identity",
                table: "UserInvitations");

            migrationBuilder.DropIndex(
                name: "IX_UserInvitations_ExpiresAt_Status",
                schema: "identity",
                table: "UserInvitations");
        }
    }
}
