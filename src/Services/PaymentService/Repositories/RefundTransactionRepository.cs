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
    public class RefundTransactionRepository : IRefundTransactionRepository
    {
        private readonly PaymentDbContext _context;

        public RefundTransactionRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<RefundTransaction> AddAsync(RefundTransaction refund)
        {
            refund.CreatedAt = DateTime.UtcNow;
            refund.UpdatedAt = DateTime.UtcNow;

            _context.RefundTransactions.Add(refund);
            await _context.SaveChangesAsync();
            return refund;
        }

        public async Task<RefundTransaction?> GetByIdAsync(long id)
        {
            return await _context.RefundTransactions.FindAsync(id);
        }

        public async Task<RefundTransaction?> GetByRefundNoAsync(string refundNo)
        {
            return await _context.RefundTransactions
                .FirstOrDefaultAsync(r => r.RefundNo == refundNo);
        }

        public async Task<RefundTransaction?> GetByGatewayRefundNoAsync(string gatewayRefundNo)
        {
            return await _context.RefundTransactions
                .FirstOrDefaultAsync(r => r.GatewayRefundNo == gatewayRefundNo);
        }

        public async Task<IEnumerable<RefundTransaction>> GetByPaymentTransactionIdAsync(long paymentTransactionId)
        {
            return await _context.RefundTransactions
                .Where(r => r.PaymentTransactionId == paymentTransactionId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefundTransaction>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _context.RefundTransactions
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefundTransaction>> GetByStatusAsync(string status, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.RefundTransactions
                .Where(r => r.Status == status);

            if (startDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= endDate.Value);
            }

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(RefundTransaction refund)
        {
            refund.UpdatedAt = DateTime.UtcNow;
            _context.RefundTransactions.Update(refund);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStatusAsync(long id, string status, string? gatewayRefundNo = null, DateTime? refundedAt = null)
        {
            var refund = await GetByIdAsync(id);
            if (refund == null)
                return false;

            refund.Status = status;
            refund.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(gatewayRefundNo))
            {
                refund.GatewayRefundNo = gatewayRefundNo;
            }

            if (refundedAt.HasValue)
            {
                refund.RefundedAt = refundedAt.Value;
            }

            _context.RefundTransactions.Update(refund);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<long> GetCountByUserIdAsync(Guid userId)
        {
            return await _context.RefundTransactions
                .Where(r => r.UserId == userId)
                .LongCountAsync();
        }

        public async Task<decimal> GetTotalRefundedAmountByUserIdAsync(Guid userId)
        {
            var result = await _context.RefundTransactions
                .Where(r => r.UserId == userId && r.Status == RefundStatus.Success.ToString())
                .SumAsync(r => r.Amount);

            return result;
        }

        public async Task<decimal> GetTotalRefundedAmountByPaymentIdAsync(long paymentTransactionId)
        {
            var result = await _context.RefundTransactions
                .Where(r => r.PaymentTransactionId == paymentTransactionId &&
                       r.Status == RefundStatus.Success.ToString())
                .SumAsync(r => r.Amount);

            return result;
        }

        public async Task<bool> ExistsRefundNoAsync(string refundNo)
        {
            return await _context.RefundTransactions
                .AnyAsync(r => r.RefundNo == refundNo);
        }
    }
}