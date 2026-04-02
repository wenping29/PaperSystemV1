using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Entities
{
    public enum RefundStatus
    {
        Pending,        // 等待处理
        Processing,     // 处理中
        Success,        // 退款成功
        Failed,         // 退款失败
        Cancelled       // 已取消
    }

    public enum RefundSource
    {
        UserRequest,    // 用户申请
        AdminManual,    // 管理员手动
        SystemAuto,     // 系统自动
        Dispute         // 争议处理
    }

    [Table("refund_transactions")]
    public class RefundTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string RefundNo { get; set; } = string.Empty; // 系统退款单号

        [MaxLength(100)]
        public string? GatewayRefundNo { get; set; } // 网关退款单号

        [Required]
        public long PaymentTransactionId { get; set; } // 关联的支付交易

        [Required]
        [MaxLength(50)]
        public string PaymentGateway { get; set; } = string.Empty; // 支付网关

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "CNY";

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // 退款金额

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Fee { get; set; } // 退款手续费

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = RefundStatus.Pending.ToString();

        [Required]
        [MaxLength(50)]
        public string Source { get; set; } = RefundSource.UserRequest.ToString();

        [Required]
        public Guid UserId { get; set; } // 申请退款的用户

        [MaxLength(100)]
        public string? UserEmail { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; } // 退款原因

        [MaxLength(1000)]
        public string? Description { get; set; } // 退款说明

        // 审核信息
        [MaxLength(100)]
        public string? ReviewerId { get; set; } // 审核人ID

        [MaxLength(100)]
        public string? ReviewerName { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [MaxLength(500)]
        public string? ReviewComment { get; set; }

        // 支付网关回调数据
        [Column(TypeName = "json")]
        public string? GatewayRequestData { get; set; }

        [Column(TypeName = "json")]
        public string? GatewayResponseData { get; set; }

        [Column(TypeName = "json")]
        public string? GatewayCallbackData { get; set; }

        // 时间戳
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? RefundedAt { get; set; } // 退款成功时间

        // 审计字段
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        [MaxLength(50)]
        public string? ErrorCode { get; set; }

        // 导航属性
        [ForeignKey("PaymentTransactionId")]
        public virtual PaymentTransaction? PaymentTransaction { get; set; }
    }
}