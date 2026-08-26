namespace OEA.Checklist.Web.Domain.Entities;

/// <summary>
/// Actividad del checklist OEA (una fila del checklist original).
/// Los datos compartidos (fundamento, actividad, responsable, ...) viven aquí.
/// Los datos operativos por mes viven en MonthlyStatus.
/// </summary>
public class Activity
{
    public int Id { get; set; }

    /// <summary>Identificador legible: "RGCE 7.1.1-I", "E3 · 1.1", etc.</summary>
    public string Item { get; set; } = string.Empty;

    /// <summary>Fundamento legal completo.</summary>
    public string Legal { get; set; } = string.Empty;

    /// <summary>Nombre corto de la actividad.</summary>
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string Documents { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Related { get; set; } = string.Empty;

    /// <summary>Frecuencia: Mensual, Trimestral, Anual, "Cuando cambie", etc.</summary>
    public string Frequency { get; set; } = string.Empty;

    /// <summary>Orden de aparición en el checklist.</summary>
    public int SortOrder { get; set; }

    /// <summary>Soft-delete flag. Las actividades inactivas no aparecen en el checklist activo.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public ICollection<MonthlyStatus> MonthlyStatuses { get; set; } = new List<MonthlyStatus>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
