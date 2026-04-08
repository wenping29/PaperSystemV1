using System;
using System.ComponentModel.DataAnnotations;

namespace PaperSystemApi.Models
{
    public class UserActivityLog
    {
        public long Id { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public ActivityType ActivityType { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Details { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(50)]
        public string? ResourceType { get; set; }

        public long? ResourceId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        public virtual User? User { get; set; }
    }

    public enum ActivityType
    {
        Login,              // 用户登录
        Logout,             // 用户登出
        ProfileUpdate,      // 资料更新
        PasswordChange,     // 密码修改
        RoleChanged,        // 角色变更
        UserCreated,        // 用户创建
        UserUpdated,        // 用户更新
        UserDeleted,        // 用户删除
        FollowUser,         // 关注用户
        UnfollowUser,       // 取消关注
        PostCreated,        // 作品创建
        PostUpdated,        // 作品更新
        PostDeleted,        // 作品删除
        CommentCreated,     // 评论创建
        CommentUpdated,     // 评论更新
        CommentDeleted,     // 评论删除
        LikeCreated,        // 点赞创建
        LikeDeleted,        // 点赞删除
        CollectionCreated,  // 收藏创建
        CollectionDeleted,  // 收藏删除
        PasswordReset,      // 密码重置
        EmailVerified,      // 邮箱验证
        PhoneVerified,      // 手机验证
        SystemOperation     // 系统操作
    }
}