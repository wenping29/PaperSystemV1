using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaperSystemApi.Migrations.AIDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServiceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RequestType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RequestData = table.Column<string>(type: "json", nullable: true),
                    ResponseData = table.Column<string>(type: "json", nullable: true),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ProcessingTimeMs = table.Column<long>(type: "INTEGER", nullable: false),
                    TokenCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Cost = table.Column<decimal>(type: "TEXT", nullable: false),
                    ModelUsed = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClientIp = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIAuditLogs_CreatedAt",
                table: "AIAuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIAuditLogs_RequestType",
                table: "AIAuditLogs",
                column: "RequestType");

            migrationBuilder.CreateIndex(
                name: "IX_AIAuditLogs_ServiceType",
                table: "AIAuditLogs",
                column: "ServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_AIAuditLogs_ServiceType_RequestType_CreatedAt",
                table: "AIAuditLogs",
                columns: new[] { "ServiceType", "RequestType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AIAuditLogs_UserId",
                table: "AIAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAuditLogs_UserId_CreatedAt",
                table: "AIAuditLogs",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIAuditLogs");
        }
    }
}
