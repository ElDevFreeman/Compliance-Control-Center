using Microsoft.AspNetCore.Identity;

namespace ComplianceControlCenter.Web.Data;

/// <summary>
/// Usuario de la aplicación. La identidad de acceso es el número de empleado
/// (se persiste en <see cref="IdentityUser.UserName"/>). El correo es opcional.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Nombre visible en comentarios y auditoría.</summary>
    public string? DisplayName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Alias legible del número de empleado (mismo valor que UserName).</summary>
    public string EmployeeNumber => UserName ?? string.Empty;
}
