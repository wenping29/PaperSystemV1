using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaperSystemApi.AI.Entities;

namespace PaperSystemApi.AI.Interfaces
{
    public interface IAIAuditLogRepository
    {
        Task<AIAuditLog> AddAsync(AIAuditLog auditLog);
        Task<AIAuditLog?> GetByIdAsync(long id);
        Task<IEnumerable<AIAuditLog>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<IEnumerable<AIAuditLog>> GetByServiceTypeAsync(string serviceType, DateTime startDate, DateTime endDate);
        Task<IEnumerable<AIAuditLog>> GetByRequestTypeAsync(string requestType, int page = 1, int pageSize = 20);
        Task<long> GetCountByUserIdAsync(Guid userId);
        Task<long> GetCountByServiceTypeAsync(string serviceType, DateTime startDate, DateTime endDate);
        Task<bool> DeleteOldLogsAsync(DateTime cutoffDate);
    }
}