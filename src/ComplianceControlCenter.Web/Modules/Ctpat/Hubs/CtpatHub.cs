using Microsoft.AspNetCore.SignalR;

namespace ComplianceControlCenter.Web.Modules.Ctpat.Hubs;

/// <summary>
/// Hub SignalR para notificaciones en tiempo real del módulo CTPAT.
/// </summary>
public class CtpatHub : Hub
{
    /// <summary>Ruta HTTP del hub.</summary>
    public const string HubPath = "/hubs/ctpat";

    // Eventos que el hub emite:
    public const string ReviewChanged     = nameof(ReviewChanged);
    public const string FileAdded         = nameof(FileAdded);
    public const string FileDeleted       = nameof(FileDeleted);
}
