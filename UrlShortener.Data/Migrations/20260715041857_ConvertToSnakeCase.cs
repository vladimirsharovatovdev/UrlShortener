using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConvertToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Managers_ManagerId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Urls_Managers_ManagerId",
                table: "Urls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Urls",
                table: "Urls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Managers",
                table: "Managers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "Urls",
                newName: "urls");

            migrationBuilder.RenameTable(
                name: "Managers",
                newName: "managers");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "refresh_tokens");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "urls",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ShortUrl",
                table: "urls",
                newName: "short_url");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "urls",
                newName: "manager_id");

            migrationBuilder.RenameColumn(
                name: "LongUrl",
                table: "urls",
                newName: "long_url");

            migrationBuilder.RenameIndex(
                name: "IX_Urls_ManagerId",
                table: "urls",
                newName: "ix_urls_manager_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "managers",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "managers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "managers",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "refresh_tokens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "refresh_tokens",
                newName: "refresh_token");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "refresh_tokens",
                newName: "manager_id");

            migrationBuilder.RenameColumn(
                name: "AuthToken",
                table: "refresh_tokens",
                newName: "auth_token");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_ManagerId",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_manager_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_urls",
                table: "urls",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_managers",
                table: "managers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_refresh_tokens",
                table: "refresh_tokens",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_refresh_tokens_managers_manager_id",
                table: "refresh_tokens",
                column: "manager_id",
                principalTable: "managers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_urls_managers_manager_id",
                table: "urls",
                column: "manager_id",
                principalTable: "managers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_refresh_tokens_managers_manager_id",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "fk_urls_managers_manager_id",
                table: "urls");

            migrationBuilder.DropPrimaryKey(
                name: "pk_urls",
                table: "urls");

            migrationBuilder.DropPrimaryKey(
                name: "pk_managers",
                table: "managers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_refresh_tokens",
                table: "refresh_tokens");

            migrationBuilder.RenameTable(
                name: "urls",
                newName: "Urls");

            migrationBuilder.RenameTable(
                name: "managers",
                newName: "Managers");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                newName: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Urls",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "short_url",
                table: "Urls",
                newName: "ShortUrl");

            migrationBuilder.RenameColumn(
                name: "manager_id",
                table: "Urls",
                newName: "ManagerId");

            migrationBuilder.RenameColumn(
                name: "long_url",
                table: "Urls",
                newName: "LongUrl");

            migrationBuilder.RenameIndex(
                name: "ix_urls_manager_id",
                table: "Urls",
                newName: "IX_Urls_ManagerId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Managers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Managers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "Managers",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "RefreshTokens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "refresh_token",
                table: "RefreshTokens",
                newName: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "manager_id",
                table: "RefreshTokens",
                newName: "ManagerId");

            migrationBuilder.RenameColumn(
                name: "auth_token",
                table: "RefreshTokens",
                newName: "AuthToken");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_manager_id",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_ManagerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Urls",
                table: "Urls",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Managers",
                table: "Managers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Managers_ManagerId",
                table: "RefreshTokens",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Urls_Managers_ManagerId",
                table: "Urls",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
