using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;
using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Enums;

namespace ComplianceControlCenter.Web.Modules.Ctpat.Services;

/// <summary>
/// Servicio de acceso al catálogo CTPAT (preguntas + guía).
///
/// El catálogo se siembra desde JSON pero se puede editar en runtime desde las
/// páginas admin. Los métodos de lectura (GetQuestionsAsync / GetGuidance…)
/// devuelven <b>sólo elementos activos</b> por default (uso desde la revisión anual);
/// los métodos <c>*ForAdminAsync</c> devuelven todo (activos + inactivos) para las
/// pantallas de administración.
/// </summary>
public interface ICtpatCatalogService
{
    // ── Lectura pública (revisión anual) ───────────────────────────────
    Task<IReadOnlyList<CtpatQuestion>> GetQuestionsAsync(CancellationToken ct = default);
    Task<CtpatQuestion?> GetQuestionByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetCriteriosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CtpatGuidance>> GetGuidanceForGroupAsync(string groupName, CancellationToken ct = default);

    // ── Lectura admin (incluye inactivos) ──────────────────────────────
    Task<IReadOnlyList<CtpatQuestion>> GetQuestionsForAdminAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CtpatGuidance>> GetAllGuidanceForAdminAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetAllGroupNamesAsync(CancellationToken ct = default);

    // ── CRUD Preguntas ─────────────────────────────────────────────────
    Task<CtpatQuestion> CreateQuestionAsync(CtpatQuestionInput input, string user, CancellationToken ct = default);
    Task UpdateQuestionAsync(int id, CtpatQuestionInput input, string user, CancellationToken ct = default);
    Task SetQuestionActiveAsync(int id, bool isActive, string user, CancellationToken ct = default);
    Task MoveQuestionAsync(int id, int delta, string user, CancellationToken ct = default);

    // ── CRUD Guías ─────────────────────────────────────────────────────
    Task<CtpatGuidance> CreateGuidanceAsync(CtpatGuidanceInput input, string user, CancellationToken ct = default);
    Task UpdateGuidanceAsync(int id, CtpatGuidanceInput input, string user, CancellationToken ct = default);
    Task SetGuidanceActiveAsync(int id, bool isActive, string user, CancellationToken ct = default);
}

/// <summary>
/// DTO de entrada para crear/actualizar una <see cref="CtpatQuestion"/>.
/// <c>ExternalId</c> es opcional: si viene vacío se autogenera (<c>custom_&lt;guid16&gt;</c>).
/// </summary>
public record CtpatQuestionInput(
    string? ExternalId,
    string Criterio,
    string Pregunta,
    string? Respuesta2025);

/// <summary>DTO de entrada para crear/actualizar una <see cref="CtpatGuidance"/>.</summary>
public record CtpatGuidanceInput(
    string GroupName,
    string Criterio,
    string? RespTip,
    string? Revisar,
    string? Evidencia);

/// <summary>
/// Estadísticas de avance por criterio para un año dado.
/// </summary>
public record CtpatCriterioStats(
    string Criterio,
    int Total,
    int Pendiente,
    int SinCambios,
    int ConCambios,
    int Revisado)
{
    public int Atendidas => Total - Pendiente;
    public int PctAvance => Total == 0 ? 0 : (int)Math.Round(Atendidas * 100.0 / Total);
}

/// <summary>
/// Servicio de acceso a las revisiones anuales del perfil CTPAT.
/// </summary>
public interface ICtpatReviewService
{
    /// <summary>Obtiene el mapa de reviews por questionId para un año dado. Si no existe review, no la crea.</summary>
    Task<IReadOnlyDictionary<int, CtpatReview>> GetReviewsForYearAsync(int year, CancellationToken ct = default);

    /// <summary>Obtiene (o crea si no existe) la review de una pregunta en un año.</summary>
    Task<CtpatReview> GetOrCreateAsync(int questionId, int year, CancellationToken ct = default);

    /// <summary>Actualiza un campo de la revisión (patch parcial).</summary>
    Task UpdateFieldAsync(int reviewId, string fieldName, string? value, string user, CancellationToken ct = default);

    /// <summary>Actualiza el estado.</summary>
    Task UpdateStatusAsync(int reviewId, CtpatReviewStatus status, string user, CancellationToken ct = default);

    /// <summary>Devuelve estadísticas de avance por criterio para un año dado.</summary>
    Task<IReadOnlyList<CtpatCriterioStats>> GetStatsByCriterioAsync(int year, CancellationToken ct = default);

    /// <summary>Obtiene todas las reviews de un año con la pregunta incluida, para la vista de Matriz.</summary>
    Task<IReadOnlyList<CtpatReview>> GetReviewsWithQuestionsAsync(int year, CancellationToken ct = default);
}

/// <summary>
/// Servicio de adjuntos de una revisión CTPAT.
/// El binario se persiste en disco bajo <c>wwwroot/uploads/ctpat/{ReviewId}/</c>.
/// </summary>
public interface ICtpatFileService
{
    /// <summary>Sube un archivo asociado a una review. Devuelve la entidad creada.</summary>
    Task<CtpatReviewFile> UploadAsync(int reviewId, string fileName, string? contentType, Stream content, string uploadedBy, CancellationToken ct = default);

    /// <summary>Elimina un archivo por su Id interno.</summary>
    Task DeleteAsync(int fileId, CancellationToken ct = default);

    /// <summary>Devuelve la ruta absoluta a disco para un archivo dado (para servir el binario).</summary>
    Task<(string AbsolutePath, string FileName, string? ContentType)?> GetPhysicalPathAsync(string externalId, CancellationToken ct = default);
}
