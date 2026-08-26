using ComplianceControlCenter.Web.Domain.Entities;
using ComplianceControlCenter.Web.Domain.Enums;

namespace ComplianceControlCenter.Web.Services;

/// <summary>
/// Servicio principal del checklist mensual: lectura y edición de actividades
/// y sus estados mensuales.
/// </summary>
public interface IChecklistService
{
    /// <summary>Devuelve las actividades activas con su MonthlyStatus del mes indicado.</summary>
    Task<IReadOnlyList<Activity>> GetActivitiesForMonthAsync(int year, int month, CancellationToken ct = default);

    /// <summary>Obtiene o crea el registro MonthlyStatus para (activityId, year, month).</summary>
    Task<MonthlyStatus> GetOrCreateMonthlyStatusAsync(int activityId, int year, int month, CancellationToken ct = default);

    /// <summary>Actualiza el estado de una actividad para un mes.</summary>
    Task UpdateStatusAsync(int activityId, int year, int month, ComplianceStatus status, string user, CancellationToken ct = default);

    /// <summary>Actualiza la fecha de vencimiento (por mes).</summary>
    Task UpdateDueDateAsync(int activityId, int year, int month, DateOnly? dueDate, string user, CancellationToken ct = default);

    /// <summary>Actualiza acciones correctivas (por mes).</summary>
    Task UpdateCorrectiveAsync(int activityId, int year, int month, string? corrective, string user, CancellationToken ct = default);

    /// <summary>Actualiza un campo compartido de la actividad (owner, description, etc.).</summary>
    Task UpdateActivityFieldAsync(int activityId, string fieldName, string? value, string user, CancellationToken ct = default);

    Task<Activity> CreateActivityAsync(Activity activity, string user, CancellationToken ct = default);
    Task DeleteActivityAsync(int activityId, string user, CancellationToken ct = default);
}
