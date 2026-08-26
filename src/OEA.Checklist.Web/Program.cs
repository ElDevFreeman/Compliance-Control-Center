using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ApexCharts;
using OEA.Checklist.Web.Components;
using OEA.Checklist.Web.Components.Account;
using OEA.Checklist.Web.Data;
using OEA.Checklist.Web.Export;
using OEA.Checklist.Web.Hubs;
using OEA.Checklist.Web.Services;
using OEA.Checklist.Web.State;

var builder = WebApplication.CreateBuilder(args);

// ────────────────────────────────────────────────────────────────
// Razor Components (Blazor Server) + SignalR
// ────────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

// ────────────────────────────────────────────────────────────────
// Authentication (ASP.NET Core Identity con roles)
// ────────────────────────────────────────────────────────────────
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddAuthorization();

// ────────────────────────────────────────────────────────────────
// EF Core (SQL Server) + Audit Interceptor
// ────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddScoped<AuditSaveChangesInterceptor>();

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        // Sesión: sin confirmación de correo (no se usa correo).
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        // Los usuarios se identifican por número de empleado (UserName).
        // Se permiten dígitos únicamente.
        options.User.RequireUniqueEmail = false;
        options.User.AllowedUserNameCharacters = "0123456789";

        // Contraseña simple: 6–20 caracteres, sin requisitos de complejidad.
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Añade el claim DisplayName a la ClaimsPrincipal.
builder.Services.AddScoped<
    IUserClaimsPrincipalFactory<ApplicationUser>,
    AppUserClaimsPrincipalFactory>();

// ────────────────────────────────────────────────────────────────
// Application services
// ────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IChecklistService, ChecklistService>();
builder.Services.AddScoped<IMatrixService, MatrixService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IChecklistNotifier, ChecklistNotifier>();

// State (scoped porque Blazor Server tiene un scope por circuit)
builder.Services.AddScoped<ThemeState>();
builder.Services.AddScoped<UserSessionState>();
builder.Services.AddScoped<LoginModalState>();

// Blazor-ApexCharts
builder.Services.AddApexCharts();

// ────────────────────────────────────────────────────────────────
var app = builder.Build();

// ────────────────────────────────────────────────────────────────
// Middleware pipeline
// ────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Razor + SignalR
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<ChecklistHub>(ChecklistHub.HubPath);

// Identity /Account Razor components
app.MapAdditionalIdentityEndpoints();

// ────────────────────────────────────────────────────────────────
// Export endpoints (CSV / XLSX) — públicos: cualquiera puede descargar.
// ────────────────────────────────────────────────────────────────
var exports = app.MapGroup("/api/export");

exports.MapGet("/checklist.csv", async (int year, int month, IChecklistService svc) =>
{
    var activities = await svc.GetActivitiesForMonthAsync(year, month);
    var bytes = ExportGenerator.ChecklistToCsv(activities, year, month);
    return Results.File(bytes, "text/csv", $"checklist-{year:0000}-{month:00}.csv");
});

exports.MapGet("/checklist.xlsx", async (int year, int month, IChecklistService svc) =>
{
    var activities = await svc.GetActivitiesForMonthAsync(year, month);
    var bytes = ExportGenerator.ChecklistToXlsx(activities, year, month);
    return Results.File(bytes,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"checklist-{year:0000}-{month:00}.xlsx");
});

exports.MapGet("/matrix.xlsx", async (
    int fromYear, int fromMonth, int toYear, int toMonth,
    string? owner, string? search,
    IMatrixService svc) =>
{
    var result = await svc.GetMatrixAsync(fromYear, fromMonth, toYear, toMonth, owner, search);
    var bytes = ExportGenerator.MatrixToXlsx(result.Rows, result.Months);
    return Results.File(bytes,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"matriz-{fromYear:0000}{fromMonth:00}-{toYear:0000}{toMonth:00}.xlsx");
});

// ────────────────────────────────────────────────────────────────
// Inicialización DB (migrate + seed roles + seed activities)
// ────────────────────────────────────────────────────────────────
await DbInitializer.InitializeAsync(app.Services);

app.Run();
