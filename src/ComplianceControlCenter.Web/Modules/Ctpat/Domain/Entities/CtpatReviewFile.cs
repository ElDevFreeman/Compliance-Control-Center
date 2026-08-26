namespace ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;

/// <summary>
/// Archivo adjunto a una revisión CTPAT (evidencia documental).
///
/// El archivo binario se persiste en disco bajo <c>wwwroot/uploads/ctpat/{ReviewId}/</c>
/// y aquí guardamos solo la metadata + la ruta relativa. Tabla <c>CCC_CTPAT_ReviewFiles</c>.
/// </summary>
public class CtpatReviewFile
{
    public int Id { get; set; }

    public int ReviewId { get; set; }
    public CtpatReview Review { get; set; } = null!;

    /// <summary>Id externo estable (formato "f_xxxxxxxxxxxxxxxx", igual que el Flask original).</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Nombre original del archivo tal como lo subió el usuario.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Tamaño en bytes.</summary>
    public long Size { get; set; }

    /// <summary>MIME type reportado por el navegador (fallback a application/octet-stream).</summary>
    public string? ContentType { get; set; }

    /// <summary>Ruta relativa dentro de wwwroot (para servir el archivo).</summary>
    public string RelativePath { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Nombre libre del usuario que subió el archivo.</summary>
    public string? UploadedBy { get; set; }
}
