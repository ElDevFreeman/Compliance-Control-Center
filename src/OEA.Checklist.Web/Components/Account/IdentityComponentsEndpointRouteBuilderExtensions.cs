using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OEA.Checklist.Web.Data;

namespace Microsoft.AspNetCore.Routing;

internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
    // These endpoints are required by the Identity Razor components defined in the /Components/Account/Pages directory of this project.
    public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup("/Account");

        accountGroup.MapPost("/Logout", async (
            ClaimsPrincipal user,
            SignInManager<ApplicationUser> signInManager,
            [FromForm] string? returnUrl) =>
        {
            await signInManager.SignOutAsync();
            // Normalize returnUrl to avoid "~//" which fails LocalRedirect's IsLocalUrl check.
            var normalized = string.IsNullOrWhiteSpace(returnUrl)
                ? string.Empty
                : returnUrl.TrimStart('/', '\\');
            return TypedResults.LocalRedirect($"~/{normalized}");
        });

        return accountGroup;
    }
}
