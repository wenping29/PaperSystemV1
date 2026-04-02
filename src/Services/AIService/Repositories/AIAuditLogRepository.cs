using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AIService.Data;
using AIService.Entities;
using AIService.Interfaces;

namespace AIService.Repositories
{
    public class AIAuditLogRepository : IAIAuditLogRepository
    {
        private readonly AIDbContext _context;

        public AIAuditLogRepository(AIDbContext context)
        {
            _context = context;
        }

        public async Task<AIAuditLog> AddAsync(AIAuditLog auditLog)
        {
            auditLog.CreatedAt = DateTime.UtcNow;
            auditLog.RequestDate = auditLog.CreatedAt.Date;

            _context.AIAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return auditLog;
        }

        public async Task<AIAuditLog?> GetByIdAsync(long id)
        {
            return await _context.AIAuditLogs.FindAsync(id);
        }

        public async Task<IEnumerable<AIAuditLog>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _context.AIAuditLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<AIAuditLog>> GetByServiceTypeAsync(string serviceType, DateTime startDate, DateTime endDate)
        {
            return await _context.AIAuditLogs
                .Where(log => log.ServiceType == serviceType &&
                       log.CreatedAt >= startDate &&
                       log.CreatedAt <= endDate)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AIAuditLog>> GetByRequestTypeAsync(string requestType, int page = 1, int pageSize = 20)
        {
            return await _context.AIAuditLogs
                .Where(log => log.RequestType == requestType)
                .OrderByDescending(log => log.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<long> GetCountByUserIdAsync(Guid userId)
        {
            return await _context.AIAuditLogs
                .Where(log => log.UserId == userId)
                .LongCountAsync();
        }

        public async Task<long> GetCountByServiceTypeAsync(string serviceType, DateTime startDate, DateTime endDate)
        {
            return await _context.AIAuditLogs
                .Where(log => log.ServiceType == serviceType &&
                       log.CreatedAt >= startDate &&
                       log.CreatedAt <= endDate)
                .LongCountAsync();
        }

        public async Task<bool> DeleteOldLogsAsync(DateTime cutoffDate)
        {
            var oldLogs = await _context.AIAuditLogs
                .Where(log => log.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldLogs.Any())
            {
                _context.AIAuditLogs.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}