using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentService.DTOs;

namespace PaymentService.Interfaces
{
    public interface IPaymentGatewayClient
    {
        string GatewayName { get; }

        // 支付相关
        Task<GatewayPaymentResponse> CreatePaymentAsync(GatewayPaymentRequest request);
        Task<GatewayPaymentQueryResponse> QueryPaymentAsync(string transactionNo);
        Task<GatewayPaymentQueryResponse> QueryPaymentByGatewayNoAsync(string gatewayTransactionNo);
        Task<bool> ClosePaymentAsync(string transactionNo);

        // 退款相关
        Task<GatewayRefundResponse> CreateRefundAsync(GatewayRefundRequest request);
        Task<GatewayRefundQueryResponse> QueryRefundAsync(string refundNo);
        Task<GatewayRefundQueryResponse> QueryRefundByGatewayNoAsync(string gatewayRefundNo);

        // 回调验证
        Task<bool> VerifyCallbackAsync(Dictionary<string, string> callbackData);
        Task<Dictionary<string, string>> ParseCallbackAsync(Dictionary<string, string> callbackData);

        // 工具方法
        Task<bool> ValidateConfigAsync();
        string GenerateSignature(Dictionary<string, string> parameters);
    }

    public class GatewayPaymentRequest
    {
        public string TransactionNo { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "CNY";
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Channel { get; set; } = "web"; // web, wap, app
        public string ReturnUrl { get; set; } = string.Empty;
        public string NotifyUrl { get; set; } = string.Empty;
        public Dictionary<string, string>? ExtraParams { get; set; }
    }

    public class GatewayPaymentResponse
    {
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? GatewayTransactionNo { get; set; }
        public string? PaymentUrl { get; set; } // 支付页面URL
        public string? QrCode { get; set; } // 二维码内容或URL
        public Dictionary<string, object>? GatewayData { get; set; }
    }

    public class GatewayPaymentQueryResponse
    {
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Status { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? GatewayTransactionNo { get; set; }
        public DateTime? PaidAt { get; set; }
        public Dictionary<string, object>? GatewayData { get; set; }
    }

    public class GatewayRefundRequest
    {
        public string TransactionNo { get; set; } = string.Empty;
        public string? GatewayTransactionNo { get; set; }
        public string RefundNo { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "CNY";
        public string Reason { get; set; } = string.Empty;
        public Dictionary<string, string>? ExtraParams { get; set; }
    }

    public class GatewayRefundResponse
    {
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? GatewayRefundNo { get; set; }
        public Dictionary<string, object>? GatewayData { get; set; }
    }

    public class GatewayRefundQueryResponse
    {
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Status { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? GatewayRefundNo { get; set; }
        public DateTime? RefundedAt { get; set; }
        public Dictionary<string, object>? GatewayData { get; set; }
    }
}