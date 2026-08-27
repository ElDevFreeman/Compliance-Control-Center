using ComplianceControlCenter.Web.Modules.Oea.Domain.Entities;

namespace ComplianceControlCenter.Web.Modules.Oea.State;

/// <summary>
/// Coordina la apertura del slide-over panel de OEA Checklist desde <c>Checklist.razor</c>
/// hacia <c>OeaChecklistPanel</c>, que vive <b>fuera</b> del <c>drawer-content</c>
/// en <c>MainLayout.razor</c>, garantizando que <c>position: fixed</c> se ancle
/// al viewport real y no a un contenedor con <c>overflow != visible</c>.
///
/// Registrar como <b>Scoped</b> — un estado por circuit de Blazor Server.
/// </summary>
public sealed class OeaChecklistPanelState
{
    // ── Datos públicos del panel ────────────────────────────────────────────

    public Activity?             Activity        { get; private set; }
    public int                   ActiveTab       { get; set; }
    public bool                  LoadingHistory  { get; set; }
    public List<MonthlyStatus>   History         { get; set; } = new();
    public List<Comment>         Comments        { get; set; } = new();
    public string                NewComment      { get; set; } = "";
    public bool                  Saving          { get; set; }

    /// <summary>Contexto del usuario activo, inyectado por la página al abrir el panel.</summary>
    public string                CurrentUserName { get; set; } = "";
    public string                CurrentUserId   { get; set; } = "";
    public bool                  CanEdit         { get; set; }
    public bool                  IsAdmin         { get; set; }

    // ── Callbacks para que el panel pueda pedir acciones a la página ────────

    /// <summary>La página asigna este callback; el panel lo invoca al publicar comentario.</summary>
    public Func<Task>?           OnAddComment    { get; set; }

    /// <summary>La página asigna este callback; el panel lo invoca al eliminar un comentario.</summary>
    public Func<Comment, Task>?  OnDeleteComment { get; set; }

    /// <summary>La página asigna este callback; el panel lo invoca cuando se cierra.</summary>
    public Action?               OnClosed        { get; set; }

    /// <summary>Solicitar apertura del modal de login desde el panel.</summary>
    public Action?               OnRequestLogin  { get; set; }

    // ── Eventos ──────────────────────────────────────────────────────────────

    /// <summary>Se dispara al abrir, cerrar o mutar el panel para que el componente re-renderice.</summary>
    public event Action? OnChange;

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>Abre el panel con la actividad. El historial y los comentarios se cargan de forma diferida.</summary>
    public void Open(Activity activity)
    {
        Activity        = activity;
        ActiveTab       = 0;
        NewComment      = "";
        History         = new();
        Comments        = new();
        LoadingHistory  = true;
        Saving          = false;
        OnChange?.Invoke();
    }

    /// <summary>Reemplaza el historial cargado (y apaga el spinner).</summary>
    public void SetHistory(List<MonthlyStatus> history)
    {
        History        = history;
        LoadingHistory = false;
        OnChange?.Invoke();
    }

    /// <summary>Reemplaza la lista de comentarios.</summary>
    public void SetComments(List<Comment> comments)
    {
        Comments = comments;
        OnChange?.Invoke();
    }

    /// <summary>Cierra el panel.</summary>
    public void Close()
    {
        Activity = null;
        OnChange?.Invoke();
        OnClosed?.Invoke();
    }

    public bool IsOpen => Activity is not null;

    /// <summary>Notifica a los suscriptores sin cambiar estado.</summary>
    public void NotifyChanged() => OnChange?.Invoke();
}
