using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs
{
    public class CreatePaymentRequest
    {
        [Required]
        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "CNY";

        [Required]
        public string PaymentGateway { get; set; } = "Alipay";

        [Required]
        public string PaymentType { get; set; } = "Donation";

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ProductDescription { get; set; }

        [StringLength(100)]
        public string? ProductId { get; set; }

        // 打赏/捐赠特定字段
        [StringLength(100)]
        public string? TargetUserId { get; set; }

        [StringLength(200)]
        public string? TargetUserName { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }

        public bool Anonymous { get; set; } = false;

        // 客户端信息
        [StringLength(100)]
        public string? Channel { get; set; } // app, web, wap

        [StringLength(500)]
        public string? ReturnUrl { get; set; } // 前端回调地址

        [StringLength(500)]
        public string? Attach { get; set; } // 附加数据

        // 扩展字段
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class CreatePaymentResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public PaymentData? Data { get; set; }
        public string? ErrorCode { get; set; }
    }

    public class PaymentData
    {
        public long TransactionId { get; set; }
        public string TransactionNo { get; set; } = string.Empty;
        public string PaymentGateway { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "CNY";
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }

        // 支付网关返回的数据（如支付宝的支付页面URL、二维码等）
        public string? GatewayPaymentUrl { get; set; }
        public string? GatewayQrCode { get; set; }
        public Dictionary<string, object>? GatewayResponse { get; set; }
    }

    public class PaymentQueryRequest
    {
        [Required]
        public string TransactionNo { get; set; } = string.Empty;

        public bool IncludeGatewayData { get; set; } = false;
    }

    public class PaymentQueryResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public PaymentDetail? Data { get; set; }
        public string? ErrorCode { get; set; }
    }

    public class PaymentDetail
    {
        public long Id { get; set; }
        public string TransactionNo { get; set; } = string.Empty;
        public string? GatewayTransactionNo { get; set; }
        public Guid UserId { get; set; }
        public string? UserEmail { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal? Fee { get; set; }
        public decimal? RefundedAmount { get; set; }
        public string Currency { get; set; } = "CNY";
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public string? TargetUserId { get; set; }
        public string? TargetUserName { get; set; }
        public string? Message { get; set; }
        public bool Anonymous { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public Dictionary<string, object>? GatewayData { get; set; }
    }

    public class CreateRefundRequest
    {
        [Required]
        public string TransactionNo { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool FullRefund { get; set; } = true; // 是否全额退款
    }

    public class CreateRefundResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public RefundData? Data { get; set; }
        public string? ErrorCode { get; set; }
    }

    public class RefundData
    {
        public long RefundId { get; set; }
        public string RefundNo { get; set; } = string.Empty;
        public long PaymentTransactionId { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "CNY";
        public string Status { get; set; } = "Pending";
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
    }

    public class RefundQueryRequest
    {
        [Required]
        public string RefundNo { get; set; } = string.Empty;
    }

    public class RefundQueryResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public RefundDetail? Data { get; set; }
        public string? ErrorCode { get; set; }
    }

    public class RefundDetail
    {
        public long Id { get; set; }
        public string RefundNo { get; set; } = string.Empty;
        public string? GatewayRefundNo { get; set; }
        public long PaymentTransactionId { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal? Fee { get; set; }
        public string Currency { get; set; } = "CNY";
        public string Status { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewComment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
    }

    public class PaymentCallbackRequest
    {
        public string TransactionNo { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public string? GatewayTransactionNo { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public Dictionary<string, string>? GatewayData { get; set; }
        public string? Signature { get; set; }
    }

    public class PaymentCallbackResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ProcessedAt { get; set; }
    }
}