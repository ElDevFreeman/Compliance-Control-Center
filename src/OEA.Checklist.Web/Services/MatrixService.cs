using Microsoft.EntityFrameworkCore;
using OEA.Checklist.Web.Data;
using OEA.Checklist.Web.Domain.Enums;

namespace OEA.Checklist.Web.Services;

public class MatrixService : IMatrixService
{
    private readonly AppDbContext _db;
    public MatrixService(AppDbContext db) => _db = db;

    public async Task<(IReadOnlyList<MatrixRow> Rows, IReadOnlyList<(int Year, int Month)> Months)>
        GetMatrixAsync(int fromYear, int fromMonth, int toYear, int toMonth,
                       string? owner = null, string? search = null,
                       CancellationToken ct = default)
    {
        // Genera secuencia de meses en el rango.
        var months = new List<(int Year, int Month)>();
        var cur = new DateTime(fromYear, fromMonth, 1);
        var end = new DateTime(toYear, toMonth, 1);
        while (cur <= end)
        {
            months.Add((cur.Year, cur.Month));
            cur = cur.AddMonths(1);
        }

        var q = _db.Activities.AsNoTracking().Where(a => a.IsActive);
        if (!string.IsNullOrWhiteSpace(owner)) q = q.Where(a => a.Owner == owner);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            q = q.Where(a =>
                a.Name.ToLower().Contains(s) ||
                a.Item.ToLower().Contains(s) ||
                a.Description.ToLower().Contains(s));
        }

        var activities = await q.OrderBy(a => a.SortOrder).ThenBy(a => a.Item).ToListAsync(ct);
        var ids = activities.Select(a => a.Id).ToList();

        var fromKey = fromYear * 100 + fromMonth;
        var toKey = toYear * 100 + toMonth;

        var statuses = await _db.MonthlyStatuses.AsNoTracking()
            .Where(m => ids.Contains(m.ActivityId)
                     && (m.Year * 100 + m.Month) >= fromKey
                     && (m.Year * 100 + m.Month) <= toKey)
            .ToListAsync(ct);

        var byActivity = statuses.GroupBy(s => s.ActivityId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => (x.Year, x.Month), x => (ComplianceStatus?)x.Status));

        var rows = activities.Select(a =>
        {
            var map = byActivity.TryGetValue(a.Id, out var m) ? m : new Dictionary<(int, int), ComplianceStatus?>();
            var rated = map.Values.Count(v => v is not null and not ComplianceStatus.NA);
            var completed = map.Values.Count(v => v == ComplianceStatus.Completado);
            var pct = rated == 0 ? 0 : Math.Round(100.0 * completed / rated, 1);
            return new MatrixRow(a.Id, a.Item, a.Name, a.Owner, map, rated, completed, pct);
        }).ToList();

        return (rows, months);
    }
}
