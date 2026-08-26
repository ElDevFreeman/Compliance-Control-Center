using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;
using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Enums;

namespace ComplianceControlCenter.Web.Modules.Ctpat.Services;

/// <summary>
/// Servicio de acceso al catálogo CTPAT (preguntas + guía).
/// El catálogo se considera lectura (sembrado desde JSON).
/// </summary>
public interface ICtpatCatalogService
{
    Task<IReadOnlyList<CtpatQuestion>> GetQuestionsAsync(CancellationToken ct = default);
    Task<CtpatQuestion?> GetQuestionByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetCriteriosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CtpatGuidance>> GetGuidanceForGroupAsync(string groupName, CancellationToken ct = default);
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
