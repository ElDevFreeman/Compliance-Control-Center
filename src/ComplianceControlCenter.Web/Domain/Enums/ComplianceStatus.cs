namespace ComplianceControlCenter.Web.Domain.Enums;

/// <summary>
/// Estados posibles de cumplimiento para una actividad en un mes dado.
/// El valor entero se persiste en BD (columna Status en OEA_MonthlyStatus).
/// </summary>
public enum ComplianceStatus
{
    Pendiente = 0,
    EnProgreso = 1,
    Completado = 2,
    Vencido = 3,
    NA = 4
}
