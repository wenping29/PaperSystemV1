using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaperSystemApi.Migrations.FriendshipDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FriendRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequesterId = table.Column<long>(type: "INTEGER", nullable: false),
                    ReceiverId = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResponseMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    FriendId = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Alias = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    InteractionScore = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastInteractedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    MetadataJson = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PrivacySettings = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_CreatedAt",
                table: "FriendRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_ExpiresAt",
                table: "FriendRequests",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_ReceiverId",
                table: "FriendRequests",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_RequesterId",
                table: "FriendRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_RequesterId_ReceiverId_Status",
                table: "FriendRequests",
                columns: new[] { "RequesterId", "ReceiverId", "Status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_Status",
                table: "FriendRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_CreatedAt",
                table: "Friendships",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_FriendId",
                table: "Friendships",
                column: "FriendId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_LastInteractedAt",
                table: "Friendships",
                column: "LastInteractedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_Status",
                table: "Friendships",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserId",
                table: "Friendships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserId_FriendId",
                table: "Friendships",
                columns: new[] { "UserId", "FriendId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserId_IsFavorite",
                table: "Friendships",
                columns: new[] { "UserId", "IsFavorite" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FriendRequests");

            migrationBuilder.DropTable(
                name: "Friendships");
        }
    }
}
