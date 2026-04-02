using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Entities
{
    public enum PaymentStatus
    {
        Pending,        // 等待支付
        Processing,     // 处理中
        Success,        // 支付成功
        Failed,         // 支付失败
        Cancelled,      // 已取消
        Refunded,       // 已退款
        PartiallyRefunded // 部分退款
    }

    public enum PaymentGateway
    {
        Alipay,
        WeChatPay,
        PayPal,
        Stripe,
        Manual // 手动支付（测试用）
    }

    public enum PaymentType
    {
        Donation,       // 打赏
        Reward,         // 奖励
        Subscription,   // 订阅
        Purchase,       // 购买
        Refund          // 退款
    }

    [Table("payment_transactions")]
    public class PaymentTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TransactionNo { get; set; } = string.Empty; // 系统交易号

        [MaxLength(100)]
        public string? GatewayTransactionNo { get; set; } // 网关交易号（支付宝/微信交易号）

        [Required]
        public Guid UserId { get; set; }

        [MaxLength(100)]
        public string? UserEmail { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentGateway { get; set; } = string.Empty; // 支付网关

        [Required]
        [MaxLength(50)]
        public string PaymentType { get; set; } = string.Empty; // 支付类型

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "CNY";

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Fee { get; set; } // 手续费

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RefundedAmount { get; set; } // 已退款金额

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = PaymentStatus.Pending.ToString();

        [MaxLength(500)]
        public string? Description { get; set; }

        // 商品/服务信息
        [MaxLength(100)]
        public string? ProductId { get; set; }

        [MaxLength(200)]
        public string? ProductName { get; set; }

        [MaxLength(500)]
        public string? ProductDescription { get; set; }

        // 打赏/捐赠特定字段
        [MaxLength(100)]
        public string? TargetUserId { get; set; } // 打赏目标用户ID

        [MaxLength(200)]
        public string? TargetUserName { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; } // 打赏留言

        [MaxLength(50)]
        public string? Anonymous { get; set; } = "false"; // 是否匿名

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

        public DateTime? PaidAt { get; set; } // 支付成功时间

        public DateTime? ExpiredAt { get; set; } // 支付过期时间

        // 客户端信息
        [MaxLength(50)]
        public string? ClientIp { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [MaxLength(100)]
        public string? DeviceInfo { get; set; }

        // 扩展字段
        [MaxLength(100)]
        public string? Channel { get; set; } // 支付渠道（如：app, web, wap）

        [MaxLength(100)]
        public string? ReturnUrl { get; set; } // 前端回调地址

        [MaxLength(100)]
        public string? NotifyUrl { get; set; } // 后端通知地址

        [MaxLength(100)]
        public string? Attach { get; set; } // 附加数据

        // 审计字段
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        [MaxLength(50)]
        public string? ErrorCode { get; set; }

        // 导航属性（如果需要）
        // public virtual ICollection<RefundTransaction> Refunds { get; set; }
    }
}