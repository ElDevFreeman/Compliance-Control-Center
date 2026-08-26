namespace ComplianceControlCenter.Web.Modules.Ctpat.Domain.Enums;

/// <summary>
/// Estado de revisión de una pregunta del perfil CTPAT.
///
/// Refleja los 4 estados del formulario original en el HTML/Flask app:
///   pendiente / sin-cambios / con-cambios / revisado
/// </summary>
public enum CtpatReviewStatus
{
    /// <summary>Aún no ha sido revisada.</summary>
    Pendiente = 0,

    /// <summary>La respuesta 2025 sigue vigente, sin actualizaciones necesarias.</summary>
    SinCambios = 1,

    /// <summary>Hay cambios respecto a 2025 que requieren actualizar la respuesta.</summary>
    ConCambios = 2,

    /// <summary>Revisada y cerrada por el revisor.</summary>
    Revisado = 3
}
