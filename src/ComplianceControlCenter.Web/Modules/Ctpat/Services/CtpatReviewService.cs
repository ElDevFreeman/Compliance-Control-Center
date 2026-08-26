using Microsoft.EntityFrameworkCore;
using ComplianceControlCenter.Web.Core.Data;
using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;
using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Enums;

namespace ComplianceControlCenter.Web.Modules.Ctpat.Services;

public class CtpatReviewService : ICtpatReviewService
{
    private readonly AppDbContext _db;
    private readonly ILogger<CtpatReviewService> _logger;

    public CtpatReviewService(AppDbContext db, ILogger<CtpatReviewService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<int, CtpatReview>> GetReviewsForYearAsync(int year, CancellationToken ct = default)
    {
        var reviews = await _db.CtpatReviews
            .AsNoTracking()
            .Include(r => r.Files)
            .Where(r => r.Year == year)
            .ToListAsync(ct);
        return reviews.ToDictionary(r => r.QuestionId);
    }

    public async Task<CtpatReview> GetOrCreateAsync(int questionId, int year, CancellationToken ct = default)
    {
        var review = await _db.CtpatReviews
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.QuestionId == questionId && r.Year == year, ct);
        if (review is not null) return review;

        review = new CtpatReview
        {
            QuestionId = questionId,
            Year = year,
            Status = CtpatReviewStatus.Pendiente,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.CtpatReviews.Add(review);
        await _db.SaveChangesAsync(ct);
        return review;
    }

    public async Task UpdateFieldAsync(int reviewId, string fieldName, string? value, string user, CancellationToken ct = default)
    {
        var r = await _db.CtpatReviews.FirstOrDefaultAsync(x => x.Id == reviewId, ct);
        if (r is null) throw new InvalidOperationException($"Review {reviewId} not found");

        switch (fieldName)
        {
            case nameof(CtpatReview.EvidenciaRevisada): r.EvidenciaRevisada = value; break;
            case nameof(CtpatReview.CambiosDetectados): r.CambiosDetectados = value; break;
            case nameof(CtpatReview.RespuestaNueva):    r.RespuestaNueva    = value; break;
            case nameof(CtpatReview.Revisor):           r.Revisor           = value; break;
            case nameof(CtpatReview.Comentarios):       r.Comentarios       = value; break;
            case nameof(CtpatReview.FechaRevision):
                r.FechaRevision = DateOnly.TryParse(value, out var d) ? d : null;
                break;
            default:
                throw new ArgumentException($"Unknown or non-editable field '{fieldName}'", nameof(fieldName));
        }

        r.UpdatedAt = DateTime.UtcNow;
        r.UpdatedBy = user;
        if (string.IsNullOrEmpty(r.Revisor)) r.Revisor = user;

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateStatusAsync(int reviewId, CtpatReviewStatus status, string user, CancellationToken ct = default)
    {
        var r = await _db.CtpatReviews.FirstOrDefaultAsync(x => x.Id == reviewId, ct);
        if (r is null) throw new InvalidOperationException($"Review {reviewId} not found");

        r.Status = status;
        r.UpdatedAt = DateTime.UtcNow;
        r.UpdatedBy = user;
        if (string.IsNullOrEmpty(r.Revisor)) r.Revisor = user;

        await _db.SaveChangesAsync(ct);
    }
}
