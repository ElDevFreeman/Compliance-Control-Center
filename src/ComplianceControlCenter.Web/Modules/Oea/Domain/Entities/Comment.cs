namespace ComplianceControlCenter.Web.Modules.Oea.Domain.Entities;

/// <summary>
/// Comentario asociado a una actividad. Aparecen en el drawer del checklist.
/// </summary>
public class Comment
{
    public int Id { get; set; }

    public int ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;

    /// <summary>DisplayName o UserName del autor.</summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>Id del usuario en OEA_Users (nullable para comentarios legacy).</summary>
    public string? AuthorUserId { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
