using Microsoft.EntityFrameworkCore;
using ComplianceControlCenter.Web.Core.Data;
using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;

namespace ComplianceControlCenter.Web.Modules.Ctpat.Services;

public class CtpatCatalogService : ICtpatCatalogService
{
    private readonly AppDbContext _db;

    public CtpatCatalogService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CtpatQuestion>> GetQuestionsAsync(CancellationToken ct = default)
    {
        return await _db.CtpatQuestions
            .AsNoTracking()
            .OrderBy(q => q.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<CtpatQuestion?> GetQuestionByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.CtpatQuestions
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id, ct);
    }

    public async Task<IReadOnlyList<string>> GetCriteriosAsync(CancellationToken ct = default)
    {
        return await _db.CtpatQuestions
            .AsNoTracking()
            .Select(q => q.Criterio)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CtpatGuidance>> GetGuidanceForGroupAsync(string groupName, CancellationToken ct = default)
    {
        return await _db.CtpatGuidance
            .AsNoTracking()
            .Where(g => g.GroupName == groupName)
            .OrderBy(g => g.Criterio)
            .ToListAsync(ct);
    }
}
