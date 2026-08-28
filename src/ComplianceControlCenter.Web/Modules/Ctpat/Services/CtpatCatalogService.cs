using Microsoft.EntityFrameworkCore;
using ComplianceControlCenter.Web.Core.Data;
using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;

namespace ComplianceControlCenter.Web.Modules.Ctpat.Services;

/// <summary>
/// Acceso al catálogo CTPAT (preguntas + guías).
///
/// Las lecturas "públicas" (usadas por la revisión anual) filtran <c>IsActive == true</c>.
/// Las lecturas admin incluyen todo. Las escrituras aplican una regla de auditoría
/// mínima (<c>UpdatedAt</c> / <c>UpdatedBy</c>) además del <c>AuditSaveChangesInterceptor</c>
/// global.
/// </summary>
public class CtpatCatalogService : ICtpatCatalogService
{
    private readonly AppDbContext _db;

    public CtpatCatalogService(AppDbContext db) => _db = db;

    // ══════════════════════════════════════════════════════════════════
    //  Lectura pública (revisión anual): sólo activos
    // ══════════════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<CtpatQuestion>> GetQuestionsAsync(CancellationToken ct = default)
    {
        return await _db.CtpatQuestions
            .AsNoTracking()
            .Where(q => q.IsActive)
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
            .Where(q => q.IsActive)
            .Select(q => q.Criterio)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CtpatGuidance>> GetGuidanceForGroupAsync(string groupName, CancellationToken ct = default)
    {
        return await _db.CtpatGuidance
            .AsNoTracking()
            .Where(g => g.GroupName == groupName && g.IsActive)
            .OrderBy(g => g.Criterio)
            .ToListAsync(ct);
    }

    // ══════════════════════════════════════════════════════════════════
    //  Lectura admin: incluye inactivos
    // ══════════════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<CtpatQuestion>> GetQuestionsForAdminAsync(CancellationToken ct = default)
    {
        return await _db.CtpatQuestions
            .AsNoTracking()
            .OrderBy(q => q.Criterio).ThenBy(q => q.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CtpatGuidance>> GetAllGuidanceForAdminAsync(CancellationToken ct = default)
    {
        return await _db.CtpatGuidance
            .AsNoTracking()
            .OrderBy(g => g.GroupName).ThenBy(g => g.Criterio)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetAllGroupNamesAsync(CancellationToken ct = default)
    {
        // Une los nombres de grupo/criterio conocidos por ambas tablas para que el
        // admin pueda tipear con autocomplete sin descuadrar la relación por string.
        var fromQuestions = await _db.CtpatQuestions.AsNoTracking()
            .Select(q => q.Criterio).Distinct().ToListAsync(ct);
        var fromGuidance = await _db.CtpatGuidance.AsNoTracking()
            .Select(g => g.GroupName).Distinct().ToListAsync(ct);

        return fromQuestions
            .Union(fromGuidance, StringComparer.Ordinal)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // ══════════════════════════════════════════════════════════════════
    //  CRUD Preguntas
    // ══════════════════════════════════════════════════════════════════

    public async Task<CtpatQuestion> CreateQuestionAsync(CtpatQuestionInput input, string user, CancellationToken ct = default)
    {
        ValidateQuestion(input);

        var externalId = string.IsNullOrWhiteSpace(input.ExternalId)
            ? GenerateExternalId()
            : input.ExternalId!.Trim();

        // Unicidad del ExternalId
        var exists = await _db.CtpatQuestions.AnyAsync(q => q.ExternalId == externalId, ct);
        if (exists)
            throw new InvalidOperationException($"Ya existe una pregunta con ExternalId '{externalId}'.");

        // SortOrder = último dentro del criterio + 10 (para poder reordenar)
        var maxSort = await _db.CtpatQuestions
            .Where(q => q.Criterio == input.Criterio)
            .Select(q => (int?)q.SortOrder)
            .MaxAsync(ct) ?? 0;

        var now = DateTime.UtcNow;
        var entity = new CtpatQuestion
        {
            ExternalId    = externalId,
            Criterio      = input.Criterio.Trim(),
            Pregunta      = input.Pregunta.Trim(),
            Respuesta2025 = input.Respuesta2025?.Trim() ?? string.Empty,
            SortOrder     = maxSort + 10,
            IsActive      = true,
            CreatedAt     = now,
            UpdatedAt     = now,
            UpdatedBy     = user
        };

        _db.CtpatQuestions.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateQuestionAsync(int id, CtpatQuestionInput input, string user, CancellationToken ct = default)
    {
        ValidateQuestion(input);

        var entity = await _db.CtpatQuestions.FirstOrDefaultAsync(q => q.Id == id, ct)
            ?? throw new KeyNotFoundException($"CtpatQuestion {id} no existe.");

        // Si se envía un ExternalId distinto, validar unicidad
        if (!string.IsNullOrWhiteSpace(input.ExternalId) && input.ExternalId.Trim() != entity.ExternalId)
        {
            var newExt = input.ExternalId.Trim();
            var clash = await _db.CtpatQuestions.AnyAsync(q => q.Id != id && q.ExternalId == newExt, ct);
            if (clash)
                throw new InvalidOperationException($"Ya existe otra pregunta con ExternalId '{newExt}'.");
            entity.ExternalId = newExt;
        }

        entity.Criterio      = input.Criterio.Trim();
        entity.Pregunta      = input.Pregunta.Trim();
        entity.Respuesta2025 = input.Respuesta2025?.Trim() ?? string.Empty;
        entity.UpdatedAt     = DateTime.UtcNow;
        entity.UpdatedBy     = user;

        await _db.SaveChangesAsync(ct);
    }

    public async Task SetQuestionActiveAsync(int id, bool isActive, string user, CancellationToken ct = default)
    {
        var entity = await _db.CtpatQuestions.FirstOrDefaultAsync(q => q.Id == id, ct)
            ?? throw new KeyNotFoundException($"CtpatQuestion {id} no existe.");
        if (entity.IsActive == isActive) return;

        entity.IsActive  = isActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = user;
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Mueve la pregunta arriba/abajo dentro de su criterio intercambiando SortOrder
    /// con la vecina inmediata. <paramref name="delta"/>: <c>-1</c> = subir, <c>+1</c> = bajar.
    /// </summary>
    public async Task MoveQuestionAsync(int id, int delta, string user, CancellationToken ct = default)
    {
        if (delta == 0) return;

        var entity = await _db.CtpatQuestions.FirstOrDefaultAsync(q => q.Id == id, ct)
            ?? throw new KeyNotFoundException($"CtpatQuestion {id} no existe.");

        var neighborsQuery = _db.CtpatQuestions.Where(q => q.Criterio == entity.Criterio && q.Id != id);
        CtpatQuestion? neighbor = delta < 0
            ? await neighborsQuery.Where(q => q.SortOrder < entity.SortOrder)
                                  .OrderByDescending(q => q.SortOrder).FirstOrDefaultAsync(ct)
            : await neighborsQuery.Where(q => q.SortOrder > entity.SortOrder)
                                  .OrderBy(q => q.SortOrder).FirstOrDefaultAsync(ct);

        if (neighbor is null) return; // ya está en el extremo

        (entity.SortOrder, neighbor.SortOrder) = (neighbor.SortOrder, entity.SortOrder);
        var now = DateTime.UtcNow;
        entity.UpdatedAt   = now; entity.UpdatedBy   = user;
        neighbor.UpdatedAt = now; neighbor.UpdatedBy = user;

        await _db.SaveChangesAsync(ct);
    }

    // ══════════════════════════════════════════════════════════════════
    //  CRUD Guías
    // ══════════════════════════════════════════════════════════════════

    public async Task<CtpatGuidance> CreateGuidanceAsync(CtpatGuidanceInput input, string user, CancellationToken ct = default)
    {
        ValidateGuidance(input);

        // Regla de negocio: no permitir duplicados exactos (GroupName + Criterio).
        var group = input.GroupName.Trim();
        var crit  = input.Criterio.Trim();
        var duplicate = await _db.CtpatGuidance
            .AnyAsync(g => g.GroupName == group && g.Criterio == crit, ct);
        if (duplicate)
            throw new InvalidOperationException(
                $"Ya existe una guía para '{group}' con criterio '{crit}'. Edita la existente.");

        var now = DateTime.UtcNow;
        var entity = new CtpatGuidance
        {
            GroupName = group,
            Criterio  = crit,
            RespTip   = string.IsNullOrWhiteSpace(input.RespTip)   ? null : input.RespTip.Trim(),
            Revisar   = string.IsNullOrWhiteSpace(input.Revisar)   ? null : input.Revisar.Trim(),
            Evidencia = string.IsNullOrWhiteSpace(input.Evidencia) ? null : input.Evidencia.Trim(),
            IsActive  = true,
            CreatedAt = now,
            UpdatedAt = now,
            UpdatedBy = user
        };

        _db.CtpatGuidance.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateGuidanceAsync(int id, CtpatGuidanceInput input, string user, CancellationToken ct = default)
    {
        ValidateGuidance(input);

        var entity = await _db.CtpatGuidance.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new KeyNotFoundException($"CtpatGuidance {id} no existe.");

        var group = input.GroupName.Trim();
        var crit  = input.Criterio.Trim();

        // Si (GroupName, Criterio) cambia, revalidar unicidad
        if (entity.GroupName != group || entity.Criterio != crit)
        {
            var clash = await _db.CtpatGuidance
                .AnyAsync(g => g.Id != id && g.GroupName == group && g.Criterio == crit, ct);
            if (clash)
                throw new InvalidOperationException(
                    $"Ya existe otra guía para '{group}' con criterio '{crit}'.");
        }

        entity.GroupName = group;
        entity.Criterio  = crit;
        entity.RespTip   = string.IsNullOrWhiteSpace(input.RespTip)   ? null : input.RespTip.Trim();
        entity.Revisar   = string.IsNullOrWhiteSpace(input.Revisar)   ? null : input.Revisar.Trim();
        entity.Evidencia = string.IsNullOrWhiteSpace(input.Evidencia) ? null : input.Evidencia.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = user;

        await _db.SaveChangesAsync(ct);
    }

    public async Task SetGuidanceActiveAsync(int id, bool isActive, string user, CancellationToken ct = default)
    {
        var entity = await _db.CtpatGuidance.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new KeyNotFoundException($"CtpatGuidance {id} no existe.");
        if (entity.IsActive == isActive) return;

        entity.IsActive  = isActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = user;
        await _db.SaveChangesAsync(ct);
    }

    // ══════════════════════════════════════════════════════════════════
    //  Helpers privados
    // ══════════════════════════════════════════════════════════════════

    private static void ValidateQuestion(CtpatQuestionInput input)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));
        if (string.IsNullOrWhiteSpace(input.Criterio))
            throw new ArgumentException("El criterio es obligatorio.", nameof(input));
        if (string.IsNullOrWhiteSpace(input.Pregunta))
            throw new ArgumentException("El texto de la pregunta es obligatorio.", nameof(input));
        if (input.Criterio.Length > 128)
            throw new ArgumentException("El criterio no puede exceder 128 caracteres.", nameof(input));
        if (!string.IsNullOrWhiteSpace(input.ExternalId) && input.ExternalId.Length > 64)
            throw new ArgumentException("El ExternalId no puede exceder 64 caracteres.", nameof(input));
    }

    private static void ValidateGuidance(CtpatGuidanceInput input)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));
        if (string.IsNullOrWhiteSpace(input.GroupName))
            throw new ArgumentException("El grupo es obligatorio.", nameof(input));
        if (string.IsNullOrWhiteSpace(input.Criterio))
            throw new ArgumentException("El criterio es obligatorio.", nameof(input));
        if (input.GroupName.Length > 128)
            throw new ArgumentException("El grupo no puede exceder 128 caracteres.", nameof(input));
        if (input.Criterio.Length > 256)
            throw new ArgumentException("El criterio no puede exceder 256 caracteres.", nameof(input));
    }

    /// <summary>Genera un ExternalId estable para preguntas creadas desde la UI.</summary>
    private static string GenerateExternalId()
        => "custom_" + Guid.NewGuid().ToString("N").Substring(0, 16);
}
