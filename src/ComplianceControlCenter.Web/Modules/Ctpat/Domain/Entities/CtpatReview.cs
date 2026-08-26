using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Enums;

namespace ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;

/// <summary>
/// Revisión de una pregunta CTPAT en un ciclo anual dado.
///
/// El modelo original de Flask guardaba un único blob JSON en la tabla
/// <c>state</c>. Aquí lo normalizamos: una fila por (Question, Year), única
/// por índice, con los campos editables del formulario.
///
/// Tabla <c>CCC_CTPAT_Reviews</c>.
/// </summary>
public class CtpatReview
{
    public int Id { get; set; }

    public int QuestionId { get; set; }
    public CtpatQuestion Question { get; set; } = null!;

    /// <summary>Año del ciclo de revisión (ej. 2026).</summary>
    public int Year { get; set; }

    public CtpatReviewStatus Status { get; set; } = CtpatReviewStatus.Pendiente;

    /// <summary>Lista o descripción libre de la evidencia documental revisada.</summary>
    public string? EvidenciaRevisada { get; set; }

    /// <summary>Descripción de qué cambió respecto a la respuesta 2025.</summary>
    public string? CambiosDetectados { get; set; }

    /// <summary>Redacción actualizada de la respuesta (si aplica).</summary>
    public string? RespuestaNueva { get; set; }

    /// <summary>Fecha en la que el revisor cerró/actualizó la revisión.</summary>
    public DateOnly? FechaRevision { get; set; }

    /// <summary>Nombre libre del revisor (heredado del modelo Flask, sin FK a Users).</summary>
    public string? Revisor { get; set; }

    /// <summary>Notas internas para el equipo.</summary>
    public string? Comentarios { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }

    public ICollection<CtpatReviewFile> Files { get; set; } = new List<CtpatReviewFile>();
}
