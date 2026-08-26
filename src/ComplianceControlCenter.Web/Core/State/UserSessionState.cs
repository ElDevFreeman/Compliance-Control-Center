using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ComplianceControlCenter.Web.Core.State;

/// <summary>
/// Wrapper cómodo alrededor de AuthenticationStateProvider para exponer al
/// usuario actual (nombre visible, id, roles) desde cualquier componente Blazor.
/// </summary>
public class UserSessionState
{
    private readonly AuthenticationStateProvider _authProvider;

    public UserSessionState(AuthenticationStateProvider authProvider)
    {
        _authProvider = authProvider;
    }

    public async Task<CurrentUser> GetCurrentAsync()
    {
        var state = await _authProvider.GetAuthenticationStateAsync();
        var user = state.User;
        if (user.Identity?.IsAuthenticated != true)
            return CurrentUser.Anonymous;

        return new CurrentUser(
            UserId: user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
            UserName: user.Identity.Name ?? "",
            DisplayName: user.FindFirst("DisplayName")?.Value ?? user.Identity.Name ?? "",
            Roles: user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
        );
    }
}

public record CurrentUser(string UserId, string UserName, string DisplayName, string[] Roles)
{
    public static readonly CurrentUser Anonymous = new("", "", "Anónimo", Array.Empty<string>());
    public bool IsAuthenticated => !string.IsNullOrEmpty(UserId);
    public bool IsAdmin => Roles.Contains(Constants.Roles.Admin);
    /// <summary>Cualquier usuario autenticado (User o Admin) puede editar el checklist.</summary>
    public bool CanEdit => IsAuthenticated;
}

public static class Constants
{
    /// <summary>
    /// Modelo de roles simple: Admin (todo + gestión de usuarios) y User (edita + audita).
    /// Todo usuario autenticado tiene al menos el rol User.
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}
