using Microsoft.AspNetCore.SignalR;

namespace ComplianceControlCenter.Web.Hubs;

/// <summary>
/// Hub SignalR para difundir cambios en tiempo real entre usuarios del checklist.
///
/// Los servicios llaman IHubContext&lt;ChecklistHub&gt; después de guardar
/// y emiten eventos como "ActivityUpdated" o "StatusChanged" que los componentes
/// Blazor pueden suscribirse para refrescar solo lo necesario.
/// </summary>
public class ChecklistHub : Hub
{
    public const string HubPath = "/hubs/checklist";

    // Nombres de eventos (constantes para tipado en cliente Blazor)
    public const string ActivityChanged = "ActivityChanged";
    public const string MonthlyStatusChanged = "MonthlyStatusChanged";
    public const string CommentAdded = "CommentAdded";
    public const string CommentDeleted = "CommentDeleted";
}
