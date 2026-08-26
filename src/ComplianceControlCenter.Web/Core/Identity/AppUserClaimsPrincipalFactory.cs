using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ComplianceControlCenter.Web.Core.Data;

namespace ComplianceControlCenter.Web.Core.Identity;

/// <summary>
/// Añade el claim <c>DisplayName</c> a la <see cref="ClaimsPrincipal"/> generada
/// por Identity, para que la UI pueda mostrar el nombre visible del usuario
/// sin tener que consultar la base de datos.
/// </summary>
public sealed class AppUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public AppUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (!string.IsNullOrEmpty(user.DisplayName))
        {
            identity.AddClaim(new Claim("DisplayName", user.DisplayName));
        }
        return identity;
    }
}
