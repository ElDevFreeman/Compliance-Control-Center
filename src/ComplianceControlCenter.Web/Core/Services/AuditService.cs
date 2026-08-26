using Microsoft.EntityFrameworkCore;
using ComplianceControlCenter.Web.Core.Data;
using ComplianceControlCenter.Web.Core.Domain.Entities;
using ComplianceControlCenter.Web.Modules.Oea.Domain.Entities;

namespace ComplianceControlCenter.Web.Core.Services;

public interface IAuditService
{
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int limit = 50, CancellationToken ct = default);
}

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;
    public AuditService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int limit = 50, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 500);
        return await _db.AuditLogs.AsNoTracking()
            .OrderByDescending(a => a.Id)
            .Take(limit)
            .ToListAsync(ct);
    }
}
