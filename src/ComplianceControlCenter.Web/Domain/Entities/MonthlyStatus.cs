using ComplianceControlCenter.Web.Domain.Enums;

namespace ComplianceControlCenter.Web.Domain.Entities;

/// <summary>
/// Estado de cumplimiento de una actividad para un mes específico (Year+Month).
/// Reemplaza el objeto row.monthly[YYYY-MM] del proyecto Python original.
/// </summary>
public class MonthlyStatus
{
    public int Id { get; set; }

    public int ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;

    public int Year { get; set; }

    /// <summary>1..12</summary>
    public int Month { get; set; }

    public ComplianceStatus Status { get; set; } = ComplianceStatus.Pendiente;

    public DateOnly? DueDate { get; set; }

    public string? CorrectiveActions { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
}
