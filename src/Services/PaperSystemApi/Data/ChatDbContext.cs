using Microsoft.EntityFrameworkCore;
using PaperSystemApi.Models;
using PaperSystemApi;

namespace PaperSystemApi.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatRoomMember> ChatRoomMembers { get; set; }
        public DbSet<UserMessageRead> UserMessageReads { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置Message实体
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SenderId);
                entity.HasIndex(e => e.ReceiverId);
                entity.HasIndex(e => e.ChatRoomId);
                entity.HasIndex(e => e.ParentMessageId);
                entity.HasIndex(e => e.SentAt);
                entity.HasIndex(e => new { e.ChatRoomId, e.SentAt });

                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.MessageType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FileUrl).HasMaxLength(500);
                entity.Property(e => e.FileType).HasMaxLength(100);
                entity.Property(e => e.Metadata).HasColumnType("json");

                // 外键关系：Message -> User (Sender)
                entity.HasOne(e => e.Sender)
                      .WithMany()
                      .HasForeignKey(e => e.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 外键关系：Message -> User (Receiver)
                entity.HasOne(e => e.Receiver)
                      .WithMany()
                      .HasForeignKey(e => e.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 外键关系：Message -> ChatRoom
                entity.HasOne(e => e.ChatRoom)
                      .WithMany(e => e.Messages)
                      .HasForeignKey(e => e.ChatRoomId)
                      .OnDelete(DeleteBehavior.Cascade);

                // 自引用关系：Message -> ParentMessage
                entity.HasOne(e => e.ParentMessage)
                      .WithMany()
                      .HasForeignKey(e => e.ParentMessageId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置ChatRoom实体
            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CreatorId);
                entity.HasIndex(e => e.RoomType);
                entity.HasIndex(e => e.IsPublic);
                entity.HasIndex(e => e.InviteCode).IsUnique();
                entity.HasIndex(e => e.LastActivityAt);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.RoomType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AvatarUrl).HasMaxLength(500);
                entity.Property(e => e.InviteCode).HasMaxLength(100);
                entity.Property(e => e.Metadata).HasColumnType("json");

                // 外键关系：ChatRoom -> User (Creator)
                entity.HasOne(e => e.Creator)
                      .WithMany()
                      .HasForeignKey(e => e.CreatorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置ChatRoomMember实体
            modelBuilder.Entity<ChatRoomMember>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ChatRoomId);
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.JoinedAt);
                entity.HasIndex(e => e.LastReadAt);

                // 复合唯一索引：用户不能在同一个聊天室重复加入
                entity.HasIndex(e => new { e.UserId, e.ChatRoomId }).IsUnique();

                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Nickname).HasMaxLength(50);
                entity.Property(e => e.Metadata).HasColumnType("json");

                // 外键关系：ChatRoomMember -> User
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // 外键关系：ChatRoomMember -> ChatRoom
                entity.HasOne(e => e.ChatRoom)
                      .WithMany(e => e.Members)
                      .HasForeignKey(e => e.ChatRoomId)
                      .OnDelete(DeleteBehavior.Cascade);

                // 外键关系：ChatRoomMember -> Message (LastReadMessage)
                entity.HasOne(e => e.LastReadMessage)
                      .WithMany()
                      .HasForeignKey(e => e.LastReadMessageId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置UserMessageRead实体
            modelBuilder.Entity<UserMessageRead>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.MessageId);
                entity.HasIndex(e => e.ReadAt);

                // 复合唯一索引：用户不能重复标记同一条消息为已读
                entity.HasIndex(e => new { e.UserId, e.MessageId }).IsUnique();

                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);

                // 外键关系：UserMessageRead -> User
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // 外键关系：UserMessageRead -> Message
                entity.HasOne(e => e.Message)
                      .WithMany()
                      .HasForeignKey(e => e.MessageId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}