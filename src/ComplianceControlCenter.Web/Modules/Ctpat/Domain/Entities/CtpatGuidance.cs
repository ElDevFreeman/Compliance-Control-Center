namespace ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;

/// <summary>
/// Guía de revisión asociada a un criterio CTPAT (bloque referencial).
///
/// Estructura tomada del JSON original bajo la clave <c>guidance</c>. No hay
/// FK dura hacia <c>CtpatQuestion</c>: la relación es semántica (por número
/// de criterio) y se resuelve en runtime como en el HTML original.
///
/// Tabla <c>CCC_CTPAT_Guidance</c>.
/// </summary>
public class CtpatGuidance
{
    public int Id { get; set; }

    /// <summary>Nombre del bloque temático (ej. "Physical Security").</summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>Sub-criterio numérico (ej. "1.1.7 Declaración de apoyo").</summary>
    public string Criterio { get; set; } = string.Empty;

    /// <summary>Respuesta tipo/esperada (opcional).</summary>
    public string? RespTip { get; set; }

    /// <summary>Qué revisar concretamente durante la auditoría.</summary>
    public string? Revisar { get; set; }

    /// <summary>Evidencia sugerida que el revisor debe pedir.</summary>
    public string? Evidencia { get; set; }
}
