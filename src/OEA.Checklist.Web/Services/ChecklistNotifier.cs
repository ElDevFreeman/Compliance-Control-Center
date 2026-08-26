using Microsoft.AspNetCore.SignalR;
using OEA.Checklist.Web.Hubs;

namespace OEA.Checklist.Web.Services;

/// <summary>
/// Abstracción sobre SignalR para que los servicios de dominio puedan notificar
/// cambios sin acoplarse directamente al Hub.
/// </summary>
public interface IChecklistNotifier
{
    Task ActivityChangedAsync(int activityId, string action, string user);
    Task MonthlyStatusChangedAsync(int activityId, int year, int month, string user);
    Task CommentAddedAsync(int activityId, int commentId, string user);
    Task CommentDeletedAsync(int activityId, int commentId);
}

public class ChecklistNotifier : IChecklistNotifier
{
    private readonly IHubContext<ChecklistHub> _hub;

    public ChecklistNotifier(IHubContext<ChecklistHub> hub) => _hub = hub;

    public Task ActivityChangedAsync(int activityId, string action, string user) =>
        _hub.Clients.All.SendAsync(ChecklistHub.ActivityChanged, new { activityId, action, user });

    public Task MonthlyStatusChangedAsync(int activityId, int year, int month, string user) =>
        _hub.Clients.All.SendAsync(ChecklistHub.MonthlyStatusChanged, new { activityId, year, month, user });

    public Task CommentAddedAsync(int activityId, int commentId, string user) =>
        _hub.Clients.All.SendAsync(ChecklistHub.CommentAdded, new { activityId, commentId, user });

    public Task CommentDeletedAsync(int activityId, int commentId) =>
        _hub.Clients.All.SendAsync(ChecklistHub.CommentDeleted, new { activityId, commentId });
}
