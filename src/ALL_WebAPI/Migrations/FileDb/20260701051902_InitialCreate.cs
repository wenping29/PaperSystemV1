using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaperSystemApi.Migrations.FileDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileMetadata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SizeInBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    FileType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UploadedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    MetadataJson = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FileHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    AccessCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileMetadata", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileMetadata_CreatedAt",
                table: "FileMetadata",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FileMetadata_ExpiresAt",
                table: "FileMetadata",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_FileMetadata_FileHash",
                table: "FileMetadata",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_FileMetadata_FileId",
                table: "FileMetadata",
                column: "FileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileMetadata_FileType",
                table: "FileMetadata",
                column: "FileType");

            migrationBuilder.CreateIndex(
                name: "IX_FileMetadata_LastAccessedAt",
                table: "FileMetadata",
                column: "LastAccessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FileMetadata_Status",
                table: "FileMetadata",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FileMetadata_UploadedByUserId",
                table: "FileMetadata",
                column: "UploadedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileMetadata");
        }
    }
}
