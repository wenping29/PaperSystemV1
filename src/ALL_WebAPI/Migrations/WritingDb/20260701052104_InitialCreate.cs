using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaperSystemApi.Migrations.WritingDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ParentCategoryId = table.Column<long>(type: "INTEGER", nullable: true),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AuthorId = table.Column<long>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<long>(type: "INTEGER", nullable: true),
                    TagsJson = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Likes = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Templates_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Works",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Excerpt = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AuthorId = table.Column<long>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<long>(type: "INTEGER", nullable: true),
                    TagsJson = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    WordCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Views = table.Column<int>(type: "INTEGER", nullable: false),
                    Likes = table.Column<int>(type: "INTEGER", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Works", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Works_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WorkCollaborators",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    InvitedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCollaborators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkCollaborators_Works_WorkId",
                        column: x => x.WorkId,
                        principalTable: "Works",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkVersions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkId = table.Column<long>(type: "INTEGER", nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Excerpt = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ChangeDescription = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    WordCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkVersions_Works_WorkId",
                        column: x => x.WorkId,
                        principalTable: "Works",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsActive",
                table: "Categories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_SortOrder",
                table: "Categories",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_AuthorId",
                table: "Templates",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_CategoryId",
                table: "Templates",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_IsPublic",
                table: "Templates",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Type",
                table: "Templates",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_UsageCount",
                table: "Templates",
                column: "UsageCount");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCollaborators_Status",
                table: "WorkCollaborators",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCollaborators_UserId",
                table: "WorkCollaborators",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCollaborators_WorkId",
                table: "WorkCollaborators",
                column: "WorkId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCollaborators_WorkId_UserId",
                table: "WorkCollaborators",
                columns: new[] { "WorkId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Works_AuthorId",
                table: "Works",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Works_CategoryId",
                table: "Works",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Works_CreatedAt",
                table: "Works",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Works_IsPublished",
                table: "Works",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_Works_Slug",
                table: "Works",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Works_Status",
                table: "Works",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkVersions_CreatedAt",
                table: "WorkVersions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkVersions_VersionNumber",
                table: "WorkVersions",
                column: "VersionNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WorkVersions_WorkId",
                table: "WorkVersions",
                column: "WorkId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkVersions_WorkId_VersionNumber",
                table: "WorkVersions",
                columns: new[] { "WorkId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "WorkCollaborators");

            migrationBuilder.DropTable(
                name: "WorkVersions");

            migrationBuilder.DropTable(
                name: "Works");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
