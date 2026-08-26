using ComplianceControlCenter.Web.Domain.Enums;

namespace ComplianceControlCenter.Web.Services;

/// <summary>Vista pivotada actividad × mes para la matriz mensual.</summary>
public record MatrixRow(
    int ActivityId,
    string Item,
    string Name,
    string Owner,
    IReadOnlyDictionary<(int Year, int Month), ComplianceStatus?> Statuses,
    int TotalRated,
    int Completed,
    double CompliancePct
);

public interface IMatrixService
{
    /// <summary>
    /// Devuelve la matriz de cumplimiento entre (fromYear, fromMonth) y (toYear, toMonth).
    /// Filtros opcionales por responsable y término de búsqueda.
    /// </summary>
    Task<(IReadOnlyList<MatrixRow> Rows, IReadOnlyList<(int Year, int Month)> Months)>
        GetMatrixAsync(int fromYear, int fromMonth, int toYear, int toMonth,
                       string? owner = null, string? search = null,
                       CancellationToken ct = default);
}
