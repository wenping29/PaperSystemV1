using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentService.Entities;

namespace PaymentService.Interfaces
{
    public interface IPaymentTransactionRepository
    {
        Task<PaymentTransaction> AddAsync(PaymentTransaction transaction);
        Task<PaymentTransaction?> GetByIdAsync(long id);
        Task<PaymentTransaction?> GetByTransactionNoAsync(string transactionNo);
        Task<PaymentTransaction?> GetByGatewayTransactionNoAsync(string gatewayTransactionNo);
        Task<IEnumerable<PaymentTransaction>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<IEnumerable<PaymentTransaction>> GetByStatusAsync(string status, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<PaymentTransaction>> GetByGatewayAsync(string gateway, DateTime startDate, DateTime endDate);
        Task<bool> UpdateAsync(PaymentTransaction transaction);
        Task<bool> UpdateStatusAsync(long id, string status, string? gatewayTransactionNo = null, DateTime? paidAt = null);
        Task<bool> UpdateRefundedAmountAsync(long id, decimal refundedAmount);
        Task<long> GetCountByUserIdAsync(Guid userId);
        Task<decimal> GetTotalAmountByUserIdAsync(Guid userId);
        Task<bool> ExistsTransactionNoAsync(string transactionNo);
        Task<IEnumerable<PaymentTransaction>> GetExpiredPaymentsAsync(DateTime cutoffTime);
    }
}