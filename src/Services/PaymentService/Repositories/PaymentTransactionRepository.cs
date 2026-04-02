using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Entities;
using PaymentService.Interfaces;

namespace PaymentService.Repositories
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentTransactionRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentTransaction> AddAsync(PaymentTransaction transaction)
        {
            transaction.CreatedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;

            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<PaymentTransaction?> GetByIdAsync(long id)
        {
            return await _context.PaymentTransactions.FindAsync(id);
        }

        public async Task<PaymentTransaction?> GetByTransactionNoAsync(string transactionNo)
        {
            return await _context.PaymentTransactions
                .FirstOrDefaultAsync(t => t.TransactionNo == transactionNo);
        }

        public async Task<PaymentTransaction?> GetByGatewayTransactionNoAsync(string gatewayTransactionNo)
        {
            return await _context.PaymentTransactions
                .FirstOrDefaultAsync(t => t.GatewayTransactionNo == gatewayTransactionNo);
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _context.PaymentTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByStatusAsync(string status, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.PaymentTransactions
                .Where(t => t.Status == status);

            if (startDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= endDate.Value);
            }

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByGatewayAsync(string gateway, DateTime startDate, DateTime endDate)
        {
            return await _context.PaymentTransactions
                .Where(t => t.PaymentGateway == gateway &&
                       t.CreatedAt >= startDate &&
                       t.CreatedAt <= endDate)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(PaymentTransaction transaction)
        {
            transaction.UpdatedAt = DateTime.UtcNow;
            _context.PaymentTransactions.Update(transaction);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStatusAsync(long id, string status, string? gatewayTransactionNo = null, DateTime? paidAt = null)
        {
            var transaction = await GetByIdAsync(id);
            if (transaction == null)
                return false;

            transaction.Status = status;
            transaction.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(gatewayTransactionNo))
            {
                transaction.GatewayTransactionNo = gatewayTransactionNo;
            }

            if (paidAt.HasValue)
            {
                transaction.PaidAt = paidAt.Value;
            }

            _context.PaymentTransactions.Update(transaction);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateRefundedAmountAsync(long id, decimal refundedAmount)
        {
            var transaction = await GetByIdAsync(id);
            if (transaction == null)
                return false;

            transaction.RefundedAmount = refundedAmount;
            transaction.UpdatedAt = DateTime.UtcNow;

            // 如果退款金额等于支付金额，更新状态为已退款
            if (refundedAmount >= transaction.Amount)
            {
                transaction.Status = PaymentStatus.Refunded.ToString();
            }
            else if (refundedAmount > 0)
            {
                transaction.Status = PaymentStatus.PartiallyRefunded.ToString();
            }

            _context.PaymentTransactions.Update(transaction);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<long> GetCountByUserIdAsync(Guid userId)
        {
            return await _context.PaymentTransactions
                .Where(t => t.UserId == userId)
                .LongCountAsync();
        }

        public async Task<decimal> GetTotalAmountByUserIdAsync(Guid userId)
        {
            var result = await _context.PaymentTransactions
                .Where(t => t.UserId == userId && t.Status == PaymentStatus.Success.ToString())
                .SumAsync(t => t.Amount);

            return result;
        }

        public async Task<bool> ExistsTransactionNoAsync(string transactionNo)
        {
            return await _context.PaymentTransactions
                .AnyAsync(t => t.TransactionNo == transactionNo);
        }

        public async Task<IEnumerable<PaymentTransaction>> GetExpiredPaymentsAsync(DateTime cutoffTime)
        {
            return await _context.PaymentTransactions
                .Where(t => t.Status == PaymentStatus.Pending.ToString() &&
                       t.ExpiredAt.HasValue &&
                       t.ExpiredAt.Value <= cutoffTime)
                .ToListAsync();
        }
    }
}