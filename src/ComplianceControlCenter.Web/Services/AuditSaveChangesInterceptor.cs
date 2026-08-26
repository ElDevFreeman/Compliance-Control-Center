using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ComplianceControlCenter.Web.Data;
using ComplianceControlCenter.Web.Domain.Entities;

namespace ComplianceControlCenter.Web.Services;

/// <summary>
/// EF Core interceptor que escribe automáticamente entradas de OEA_AuditLog
/// cada vez que se guardan cambios en OEA_Activities, OEA_MonthlyStatus u OEA_Comments.
///
/// El usuario se resuelve desde HttpContext.User (DisplayName si existe, si no UserName).
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is AppDbContext db)
        {
            AppendAuditEntries(db);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AppendAuditEntries(AppDbContext db)
    {
        var user = ResolveUser();
        var now = DateTime.UtcNow;

        // Snapshot antes de tocar el ChangeTracker
        var tracked = db.ChangeTracker.Entries()
            .Where(e => e.Entity is Activity or MonthlyStatus or Comment)
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in tracked)
        {
            var log = new AuditLog
            {
                Timestamp = now,
                User = user,
                Action = entry.State.ToString(),
                EntityName = entry.Entity.GetType().Name,
                EntityId = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())
                                ?.CurrentValue?.ToString() ?? string.Empty,
                Changes = SerializeChanges(entry)
            };
            db.AuditLogs.Add(log);
        }
    }

    private static string? SerializeChanges(EntityEntry entry)
    {
        try
        {
            var changes = new Dictionary<string, object?>();
            foreach (var p in entry.Properties)
            {
                if (entry.State == EntityState.Modified && !p.IsModified) continue;
                changes[p.Metadata.Name] = new
                {
                    old = entry.State == EntityState.Added ? null : p.OriginalValue,
                    @new = entry.State == EntityState.Deleted ? null : p.CurrentValue
                };
            }
            return JsonSerializer.Serialize(changes);
        }
        catch
        {
            return null;
        }
    }

    private string ResolveUser()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx?.User?.Identity?.IsAuthenticated == true)
        {
            return ctx.User.Identity.Name ?? "unknown";
        }
        return "system";
    }
}
