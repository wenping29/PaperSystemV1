using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentService.DTOs;
using PaymentService.Entities;

namespace PaymentService.Interfaces
{
    public interface IIPaymentService
    {
        // 支付相关
        Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, Guid userId, string? userEmail = null);
        Task<PaymentQueryResponse> QueryPaymentAsync(string transactionNo, Guid userId);
        Task<PaymentQueryResponse> QueryPaymentByGatewayAsync(string gatewayTransactionNo);
        Task<bool> UpdatePaymentStatusAsync(string transactionNo, string status, string? gatewayTransactionNo = null, DateTime? paidAt = null);
        Task<List<PaymentTransaction>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<PaymentTransaction?> GetPaymentByTransactionNoAsync(string transactionNo);

        // 退款相关
        Task<CreateRefundResponse> CreateRefundAsync(CreateRefundRequest request, Guid userId);
        Task<RefundQueryResponse> QueryRefundAsync(string refundNo, Guid userId);
        Task<bool> UpdateRefundStatusAsync(string refundNo, string status, string? gatewayRefundNo = null, DateTime? refundedAt = null);
        Task<List<RefundTransaction>> GetPaymentRefundsAsync(long paymentTransactionId);
        Task<List<RefundTransaction>> GetUserRefundsAsync(Guid userId, int page = 1, int pageSize = 20);

        // 回调处理
        Task<PaymentCallbackResponse> ProcessPaymentCallbackAsync(PaymentCallbackRequest request);
        Task<bool> VerifyPaymentSignatureAsync(Dictionary<string, string> parameters, string gateway);

        // 统计与报表
        Task<decimal> GetUserTotalPaidAsync(Guid userId);
        Task<decimal> GetUserTotalRefundedAsync(Guid userId);
        Task<Dictionary<string, decimal>> GetPaymentStatsAsync(DateTime startDate, DateTime endDate);

        // 工具方法
        Task<string> GenerateTransactionNoAsync();
        Task<string> GenerateRefundNoAsync();
        Task<bool> ValidatePaymentRequestAsync(CreatePaymentRequest request);
    }
}