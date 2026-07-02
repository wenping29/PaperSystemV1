using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaperSystemApi.Migrations.ChatDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AvatarUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatRooms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RoomType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatorId = table.Column<long>(type: "INTEGER", nullable: false),
                    AvatarUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    MaxMembers = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    InviteEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    InviteCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    InviteExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastActivityAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Metadata = table.Column<string>(type: "json", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatRooms_User_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    MessageType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SenderId = table.Column<long>(type: "INTEGER", nullable: false),
                    ReceiverId = table.Column<long>(type: "INTEGER", nullable: true),
                    ChatRoomId = table.Column<long>(type: "INTEGER", nullable: true),
                    ParentMessageId = table.Column<long>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FileUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: true),
                    FileType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "json", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_ChatRooms_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Messages_ParentMessageId",
                        column: x => x.ParentMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_User_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_User_SenderId",
                        column: x => x.SenderId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatRoomMembers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    ChatRoomId = table.Column<long>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastReadMessageId = table.Column<long>(type: "INTEGER", nullable: true),
                    LastReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsMuted = table.Column<bool>(type: "INTEGER", nullable: false),
                    MutedUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsBlocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    Metadata = table.Column<string>(type: "json", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoomMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatRoomMembers_ChatRooms_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatRoomMembers_Messages_LastReadMessageId",
                        column: x => x.LastReadMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatRoomMembers_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMessageReads",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    MessageId = table.Column<long>(type: "INTEGER", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMessageReads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMessageReads_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMessageReads_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMembers_ChatRoomId",
                table: "ChatRoomMembers",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMembers_JoinedAt",
                table: "ChatRoomMembers",
                column: "JoinedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMembers_LastReadAt",
                table: "ChatRoomMembers",
                column: "LastReadAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMembers_LastReadMessageId",
                table: "ChatRoomMembers",
                column: "LastReadMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMembers_Role",
                table: "ChatRoomMembers",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMembers_UserId",
                table: "ChatRoomMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMembers_UserId_ChatRoomId",
                table: "ChatRoomMembers",
                columns: new[] { "UserId", "ChatRoomId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_CreatorId",
                table: "ChatRooms",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_InviteCode",
                table: "ChatRooms",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_IsPublic",
                table: "ChatRooms",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_LastActivityAt",
                table: "ChatRooms",
                column: "LastActivityAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_RoomType",
                table: "ChatRooms",
                column: "RoomType");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatRoomId",
                table: "Messages",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatRoomId_SentAt",
                table: "Messages",
                columns: new[] { "ChatRoomId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ParentMessageId",
                table: "Messages",
                column: "ParentMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SentAt",
                table: "Messages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessageReads_MessageId",
                table: "UserMessageReads",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessageReads_ReadAt",
                table: "UserMessageReads",
                column: "ReadAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessageReads_UserId",
                table: "UserMessageReads",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessageReads_UserId_MessageId",
                table: "UserMessageReads",
                columns: new[] { "UserId", "MessageId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatRoomMembers");

            migrationBuilder.DropTable(
                name: "UserMessageReads");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "ChatRooms");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
