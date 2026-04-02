using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentService.Entities;

namespace PaymentService.Interfaces
{
    public interface IRefundTransactionRepository
    {
        Task<RefundTransaction> AddAsync(RefundTransaction refund);
        Task<RefundTransaction?> GetByIdAsync(long id);
        Task<RefundTransaction?> GetByRefundNoAsync(string refundNo);
        Task<RefundTransaction?> GetByGatewayRefundNoAsync(string gatewayRefundNo);
        Task<IEnumerable<RefundTransaction>> GetByPaymentTransactionIdAsync(long paymentTransactionId);
        Task<IEnumerable<RefundTransaction>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<IEnumerable<RefundTransaction>> GetByStatusAsync(string status, DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> UpdateAsync(RefundTransaction refund);
        Task<bool> UpdateStatusAsync(long id, string status, string? gatewayRefundNo = null, DateTime? refundedAt = null);
        Task<long> GetCountByUserIdAsync(Guid userId);
        Task<decimal> GetTotalRefundedAmountByUserIdAsync(Guid userId);
        Task<decimal> GetTotalRefundedAmountByPaymentIdAsync(long paymentTransactionId);
        Task<bool> ExistsRefundNoAsync(string refundNo);
    }
}