namespace ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;

/// <summary>
/// Pregunta del cuestionario CTPAT (Perfil anual, ~139 items).
///
/// Cargada al arrancar desde <c>Modules/Ctpat/Data/ctpat_data.json</c> a través
/// del seeder. Tabla <c>CCC_CTPAT_Questions</c>.
/// </summary>
public class CtpatQuestion
{
    /// <summary>PK autonumérica.</summary>
    public int Id { get; set; }

    /// <summary>Id externo del catálogo JSON (formato "1.2,1", "2.3,5" …).</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Criterio CTPAT al que pertenece (ej. "Physical Security").</summary>
    public string Criterio { get; set; } = string.Empty;

    /// <summary>Texto de la pregunta (inglés, original).</summary>
    public string Pregunta { get; set; } = string.Empty;

    /// <summary>Respuesta oficial 2025 registrada (base line contra la que se revisa).</summary>
    public string Respuesta2025 { get; set; } = string.Empty;

    /// <summary>Orden estable para presentación (deducido del orden en el JSON).</summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Soft-delete flag. Cuando es <c>false</c>, la pregunta se oculta de la revisión anual
    /// pero se conservan sus <see cref="CtpatReview"/> históricos. Permite reactivarla luego.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Fecha UTC de creación de la fila.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Fecha UTC del último cambio de contenido.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Usuario (DisplayName o UserName) que hizo el último cambio.</summary>
    public string? UpdatedBy { get; set; }

    /// <summary>Reviews que se han hecho a esta pregunta (una por ciclo/año).</summary>
    public ICollection<CtpatReview> Reviews { get; set; } = new List<CtpatReview>();
}
