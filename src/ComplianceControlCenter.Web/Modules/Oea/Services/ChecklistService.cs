using Microsoft.EntityFrameworkCore;
using ComplianceControlCenter.Web.Core.Data;
using ComplianceControlCenter.Web.Modules.Oea.Domain.Entities;
using ComplianceControlCenter.Web.Modules.Oea.Domain.Enums;

namespace ComplianceControlCenter.Web.Modules.Oea.Services;

public class ChecklistService : IChecklistService
{
    private readonly AppDbContext _db;
    private readonly IChecklistNotifier _notifier;

    public ChecklistService(AppDbContext db, IChecklistNotifier notifier)
    {
        _db = db;
        _notifier = notifier;
    }

    public async Task<IReadOnlyList<Activity>> GetActivitiesForMonthAsync(int year, int month, CancellationToken ct = default)
    {
        // Cargamos actividades activas + solo el MonthlyStatus del mes pedido.
        var activities = await _db.Activities
            .AsNoTracking()
            .Where(a => a.IsActive)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Item)
            .ToListAsync(ct);

        var ids = activities.Select(a => a.Id).ToList();

        var statuses = await _db.MonthlyStatuses
            .AsNoTracking()
            .Where(m => ids.Contains(m.ActivityId) && m.Year == year && m.Month == month)
            .ToListAsync(ct);

        var byId = statuses.ToDictionary(s => s.ActivityId);
        foreach (var a in activities)
        {
            if (byId.TryGetValue(a.Id, out var s))
                a.MonthlyStatuses = new List<MonthlyStatus> { s };
        }
        return activities;
    }

    public async Task<MonthlyStatus> GetOrCreateMonthlyStatusAsync(int activityId, int year, int month, CancellationToken ct = default)
    {
        var existing = await _db.MonthlyStatuses
            .FirstOrDefaultAsync(m => m.ActivityId == activityId && m.Year == year && m.Month == month, ct);
        if (existing is not null) return existing;

        var created = new MonthlyStatus
        {
            ActivityId = activityId,
            Year = year,
            Month = month,
            Status = ComplianceStatus.Pendiente,
            UpdatedAt = DateTime.UtcNow
        };
        _db.MonthlyStatuses.Add(created);
        await _db.SaveChangesAsync(ct);
        return created;
    }

    public async Task UpdateStatusAsync(int activityId, int year, int month, ComplianceStatus status, string user, CancellationToken ct = default)
    {
        var s = await GetOrCreateMonthlyStatusAsync(activityId, year, month, ct);
        s.Status = status;
        s.UpdatedAt = DateTime.UtcNow;
        s.UpdatedBy = user;
        await _db.SaveChangesAsync(ct);
        await _notifier.MonthlyStatusChangedAsync(activityId, year, month, user);
    }

    public async Task UpdateDueDateAsync(int activityId, int year, int month, DateOnly? dueDate, string user, CancellationToken ct = default)
    {
        var s = await GetOrCreateMonthlyStatusAsync(activityId, year, month, ct);
        s.DueDate = dueDate;
        s.UpdatedAt = DateTime.UtcNow;
        s.UpdatedBy = user;

        // Regla: si vence en el pasado y no está Completado/NA -> Vencido.
        if (dueDate is { } d
            && d < DateOnly.FromDateTime(DateTime.Today)
            && s.Status is not ComplianceStatus.Completado and not ComplianceStatus.NA)
        {
            s.Status = ComplianceStatus.Vencido;
        }

        await _db.SaveChangesAsync(ct);
        await _notifier.MonthlyStatusChangedAsync(activityId, year, month, user);
    }

    public async Task UpdateCorrectiveAsync(int activityId, int year, int month, string? corrective, string user, CancellationToken ct = default)
    {
        var s = await GetOrCreateMonthlyStatusAsync(activityId, year, month, ct);
        s.CorrectiveActions = corrective;
        s.UpdatedAt = DateTime.UtcNow;
        s.UpdatedBy = user;
        await _db.SaveChangesAsync(ct);
        await _notifier.MonthlyStatusChangedAsync(activityId, year, month, user);
    }

    public async Task UpdateActivityFieldAsync(int activityId, string fieldName, string? value, string user, CancellationToken ct = default)
    {
        var a = await _db.Activities.FirstOrDefaultAsync(x => x.Id == activityId, ct)
            ?? throw new InvalidOperationException($"Activity {activityId} not found");

        switch (fieldName)
        {
            case nameof(Activity.Item): a.Item = value ?? ""; break;
            case nameof(Activity.Legal): a.Legal = value ?? ""; break;
            case nameof(Activity.Name): a.Name = value ?? ""; break;
            case nameof(Activity.Description): a.Description = value ?? ""; break;
            case nameof(Activity.Documents): a.Documents = value ?? ""; break;
            case nameof(Activity.Owner): a.Owner = value ?? ""; break;
            case nameof(Activity.Related): a.Related = value ?? ""; break;
            case nameof(Activity.Frequency): a.Frequency = value ?? ""; break;
            default: throw new ArgumentException($"Field '{fieldName}' is not editable", nameof(fieldName));
        }
        a.UpdatedAt = DateTime.UtcNow;
        a.UpdatedBy = user;
        await _db.SaveChangesAsync(ct);
        await _notifier.ActivityChangedAsync(activityId, "Updated", user);
    }

    public async Task<Activity> CreateActivityAsync(Activity activity, string user, CancellationToken ct = default)
    {
        activity.CreatedAt = DateTime.UtcNow;
        activity.CreatedBy = user;
        activity.IsActive = true;
        if (activity.SortOrder == 0)
        {
            activity.SortOrder = (await _db.Activities.MaxAsync(a => (int?)a.SortOrder, ct) ?? 0) + 10;
        }
        _db.Activities.Add(activity);
        await _db.SaveChangesAsync(ct);
        await _notifier.ActivityChangedAsync(activity.Id, "Created", user);
        return activity;
    }

    public async Task DeleteActivityAsync(int activityId, string user, CancellationToken ct = default)
    {
        // Soft-delete: marcar IsActive=false para preservar historial.
        var a = await _db.Activities.FirstOrDefaultAsync(x => x.Id == activityId, ct);
        if (a is null) return;
        a.IsActive = false;
        a.UpdatedAt = DateTime.UtcNow;
        a.UpdatedBy = user;
        await _db.SaveChangesAsync(ct);
        await _notifier.ActivityChangedAsync(activityId, "Deleted", user);
    }
}
