using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WritingService.Entities
{
    public class WorkCollaborator
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long WorkId { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Collaborator"; // 协作角色：作者、编辑、审阅者等

        public DateTime InvitedAt { get; set; } = DateTime.UtcNow;

        public DateTime? JoinedAt { get; set; }

        public CollaboratorStatus Status { get; set; } = CollaboratorStatus.Pending;

        // 导航属性
        public virtual Work? Work { get; set; }
    }

    public enum CollaboratorStatus
    {
        Pending,    // 等待接受邀请
        Active,     // 已接受，活跃协作
        Inactive,   // 已接受，但不活跃
        Rejected,   // 拒绝邀请
        Removed     // 被移除
    }
}