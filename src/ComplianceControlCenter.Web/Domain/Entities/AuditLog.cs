namespace ComplianceControlCenter.Web.Domain.Entities;

/// <summary>
/// Registro de auditoría: quién hizo qué y cuándo.
/// Se llena automáticamente vía AuditSaveChangesInterceptor.
/// </summary>
public class AuditLog
{
    public long Id { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>UserName / DisplayName del usuario que originó el cambio.</summary>
    public string User { get; set; } = string.Empty;

    /// <summary>Created / Updated / Deleted.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Nombre de la entidad afectada (Activity, MonthlyStatus, Comment, ...).</summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>PK de la entidad afectada como string.</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>JSON con el diff de propiedades modificadas.</summary>
    public string? Changes { get; set; }
}
