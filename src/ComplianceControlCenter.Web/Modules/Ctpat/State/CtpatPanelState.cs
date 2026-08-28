using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;

namespace ComplianceControlCenter.Web.Modules.Ctpat.State;

/// <summary>
/// Coordina la apertura del slide-over panel de CTPAT desde <c>Ctpat.razor</c>
/// hacia <c>CtpatSlidePanel</c>, que vive <b>fuera</b> del <c>drawer-content</c>
/// en <c>MainLayout.razor</c>, garantizando que <c>position: fixed</c> se ancle
/// al viewport real y no a un contenedor con <c>overflow != visible</c>.
///
/// Registrar como <b>Scoped</b> — un estado por circuit de Blazor Server.
/// </summary>
public sealed class CtpatPanelState
{
    // ── Datos públicos del panel ────────────────────────────────────────────

    public CtpatQuestion?              Question       { get; private set; }
    public CtpatReview?                Review         { get; private set; }
    public CtpatPanelDraft             Draft          { get; private set; } = new();
    public int                         ActiveTab      { get; set; }
    public bool                        Saving         { get; set; }
    public bool                        SaveSuccess    { get; set; }
    public string?                     SaveError      { get; set; }
    public string?                     UploadError    { get; set; }
    public List<Microsoft.AspNetCore.Components.Forms.IBrowserFile> UploadingFiles { get; } = new();
    public IReadOnlyList<CtpatGuidance>? Guidance     { get; set; }
    public bool                        IsDragging     { get; set; }
    public (int Id, string Name)?      FileToDelete   { get; set; }
    public bool                        DeletingFile   { get; set; }

    /// <summary>Nombre del usuario activo, inyectado por <c>Ctpat.razor</c> al abrir el panel.</summary>
    public string                      CurrentUser    { get; set; } = "anonymous";

    /// <summary>¿El usuario puede editar? (autenticado). Inyectado por <c>Ctpat.razor</c>.</summary>
    public bool                        CanEdit        { get; set; }

    /// <summary>¿El usuario es administrador? Inyectado por <c>Ctpat.razor</c>.</summary>
    public bool                        IsAdmin        { get; set; }

    /// <summary>Callback para que el panel solicite abrir el modal de login.</summary>
    public Action?                     OnRequestLogin { get; set; }

    // ── Callbacks para que el panel pueda pedir acciones a la página ────────

    /// <summary>
    /// La página (<c>Ctpat.razor</c>) asigna este callback; el panel lo invoca al guardar.
    /// </summary>
    public Func<Task>? OnSaveRequested  { get; set; }

    /// <summary>
    /// La página asigna este callback; el panel lo invoca al eliminar un archivo.
    /// </summary>
    public Func<Task>? OnDeleteFileConfirmed { get; set; }

    /// <summary>
    /// La página asigna este callback; el panel lo invoca al subir archivos.
    /// </summary>
    public Func<IReadOnlyList<Microsoft.AspNetCore.Components.Forms.IBrowserFile>, Task>? OnUploadFiles { get; set; }

    // ── Eventos ──────────────────────────────────────────────────────────────

    /// <summary>Se dispara al abrir o cerrar el panel para que el componente re-renderice.</summary>
    public event Action? OnChange;

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>Abre el panel con la pregunta y la revisión ya cargadas.</summary>
    public void Open(CtpatQuestion question, CtpatReview review, CtpatPanelDraft draft)
    {
        Question    = question;
        Review      = review;
        Draft       = draft;
        ActiveTab   = 0;
        Saving      = false;
        SaveSuccess = false;
        SaveError   = null;
        UploadError = null;
        UploadingFiles.Clear();
        Guidance    = null;
        IsDragging  = false;
        FileToDelete = null;
        DeletingFile = false;
        OnChange?.Invoke();
    }

    /// <summary>Actualiza la revisión visible (tras guardar o subir archivos).</summary>
    public void UpdateReview(CtpatReview review)
    {
        Review = review;
        OnChange?.Invoke();
    }

    /// <summary>Cierra el panel.</summary>
    public void Close()
    {
        Question = null;
        Review   = null;
        OnChange?.Invoke();
    }

    public bool IsOpen => Question is not null;

    /// <summary>Notifica a los suscriptores sin cambiar estado (p. ej. al actualizar Guidance).</summary>
    public void NotifyChanged() => OnChange?.Invoke();
}

/// <summary>Copia mutable de los campos editables de una <see cref="CtpatReview"/>.</summary>
public sealed class CtpatPanelDraft
{
    public Domain.Enums.CtpatReviewStatus Status             { get; set; } = Domain.Enums.CtpatReviewStatus.Pendiente;
    public string?                         EvidenciaRevisada  { get; set; }
    public string?                         CambiosDetectados  { get; set; }
    public string?                         RespuestaNueva     { get; set; }
    public string?                         Comentarios        { get; set; }
    public string?                         Revisor            { get; set; }
    public DateOnly?                       FechaRevision      { get; set; }
}
