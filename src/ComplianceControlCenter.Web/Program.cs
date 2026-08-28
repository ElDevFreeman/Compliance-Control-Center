using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ApexCharts;
using ComplianceControlCenter.Web.Components;
using ComplianceControlCenter.Web.Core.Identity;
using ComplianceControlCenter.Web.Core.Data;
using ComplianceControlCenter.Web.Modules.Oea.Export;
using ComplianceControlCenter.Web.Modules.Oea.Hubs;
using ComplianceControlCenter.Web.Core.Services;
using ComplianceControlCenter.Web.Modules.Oea.Services;
using ComplianceControlCenter.Web.Modules.Ctpat.Export;
using ComplianceControlCenter.Web.Modules.Ctpat.Hubs;
using ComplianceControlCenter.Web.Modules.Ctpat.Services;
using ComplianceControlCenter.Web.Core.State;
using ComplianceControlCenter.Web.Modules.Ctpat.State;
using ComplianceControlCenter.Web.Modules.Oea.State;

var builder = WebApplication.CreateBuilder(args);

// ────────────────────────────────────────────────────────────────
// Razor Components (Blazor Server) + SignalR
// ────────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

// ────────────────────────────────────────────────────────────────
// Reverse proxy: X-Forwarded-* headers.
//
// DESACTIVADO en base a la comparación con FDS.Web (proyecto
// hermano Blazor Server que se despliega igual en /FDS bajo el
// mismo servidor IIS y funciona sin este middleware).
//
// Con ANCM en modo InProcess, IIS y Kestrel comparten proceso y
// los headers X-Forwarded-* típicamente no se envían (IIS ya
// informa el scheme correcto vía la conexión interna). Registrar
// UseForwardedHeaders en este escenario puede REESCRIBIR
// Request.Scheme basándose en headers de un proxy externo (F5,
// ARR) que ponen "X-Forwarded-Proto: http" en el hop interno,
// haciendo que Request.IsHttps quede en false durante el POST
// del login y disparando validaciones de antiforgery que
// terminan en HTTP 400.
//
// Si se necesita re-habilitar en el futuro (p. ej. Kestrel
// standalone detrás de nginx), pasar KnownProxies explícitamente.
// ────────────────────────────────────────────────────────────────
// builder.Services.Configure<ForwardedHeadersOptions>(options =>
// {
//     options.ForwardedHeaders =
//         ForwardedHeaders.XForwardedFor |
//         ForwardedHeaders.XForwardedProto |
//         ForwardedHeaders.XForwardedHost;
//     options.KnownNetworks.Clear();
//     options.KnownProxies.Clear();
// });

// ────────────────────────────────────────────────────────────────
// Data Protection: persistir llaves de encriptación.
//
// SIN esto, cada reciclaje del AppPool (o incluso cada worker
// process nuevo dentro del mismo pool) genera llaves distintas.
// Consecuencia: la cookie antiforgery emitida en el prerender NO
// puede descifrarse en el POST → HTTP 400. Es LA causa más
// común de "login funciona local, falla en producción con IIS".
//
// Estrategia:
//   1. Ruta se toma de appsettings.json → "DataProtection:KeysPath".
//   2. Fallback: %PROGRAMDATA%\ComplianceControlCenter\dp-keys
//      (fuera del sitio publicado; escritura garantizada al
//      AppPool si se dieron permisos correctos).
//   3. Si NO se puede escribir en la ruta elegida, la app FALLA AL
//      ARRANCAR con un mensaje claro (mejor eso que quedar
//      silenciosamente con llaves en memoria que se pierden).
//
// Permisos requeridos en producción (una sola vez):
//   icacls "C:\ProgramData\ComplianceControlCenter\dp-keys" ^
//     /grant "IIS AppPool\<NombreDelAppPool>:(OI)(CI)F" /T
// ────────────────────────────────────────────────────────────────
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (string.IsNullOrWhiteSpace(keysPath))
{
    var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    keysPath = Path.Combine(programData, "ComplianceControlCenter", "dp-keys");
}

try
{
    Directory.CreateDirectory(keysPath);

    // Prueba real de escritura: crear y eliminar un archivo canario.
    // Directory.CreateDirectory NO falla si la carpeta ya existe pero
    // no tenemos permisos de escritura dentro.
    var canary = Path.Combine(keysPath, $".write-test-{Guid.NewGuid():N}.tmp");
    File.WriteAllText(canary, "ok");
    File.Delete(canary);
}
catch (Exception ex)
{
    throw new InvalidOperationException(
        $"No se puede escribir en la carpeta de DataProtection '{keysPath}'. " +
        $"Sin permisos de escritura, las llaves se generan en memoria y se " +
        $"pierden al reciclar el AppPool, causando HTTP 400 en el login. " +
        $"Ejecuta en el servidor: " +
        $"icacls \"{keysPath}\" /grant \"IIS AppPool\\<NombreDelAppPool>:(OI)(CI)F\" /T",
        ex);
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("ComplianceControlCenter");

// ────────────────────────────────────────────────────────────────
// Cookie policy: detrás de un proxy con TLS, "SameAsRequest" evita
// que el navegador descarte la cookie cuando el server ve HTTP pero
// el cliente usa HTTPS (mientras UseForwardedHeaders corrige el scheme).
// ────────────────────────────────────────────────────────────────
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.SameAsRequest;
});

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

// Ajusta las cookies de Identity para el reverse proxy + subruta.
// IMPORTANTE: NO fijamos Cookie.Path aquí. ASP.NET Core lo establece
// automáticamente al PathBase actual del request (p. ej. "/CCC"), lo
// cual es correcto. LoginPath/AccessDeniedPath/LogoutPath se resuelven
// contra el PathBase también (el framework los prefija con el PathBase
// del request en el redirect 302), por eso se dejan como rutas
// relativas al pathbase (empezando en "/").
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.HttpOnly = true;

    options.LoginPath        = "/Account/Login";
    options.LogoutPath       = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ────────────────────────────────────────────────────────────────
// Antiforgery: alineamos la cookie con el reverse-proxy + subruta.
// - Cookie.SameSite = Lax  para permitir el POST del form de login.
// - Cookie.SecurePolicy = SameAsRequest para no perder la cookie
//   cuando el server ve HTTP interno pero el cliente usa HTTPS.
// NO se fija Cookie.Path: el framework la emite con Path = PathBase
// actual (p. ej. "/CCC/"), lo cual es lo correcto para que el
// navegador la reenvíe SOLO en el POST a /CCC/Account/Login.
// ────────────────────────────────────────────────────────────────
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SameSite     = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.HttpOnly     = true;
});

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

// Módulo CTPAT
builder.Services.AddScoped<ICtpatCatalogService, CtpatCatalogService>();
builder.Services.AddScoped<ICtpatReviewService,  CtpatReviewService>();
builder.Services.AddScoped<ICtpatFileService,    CtpatFileService>();

// State (scoped porque Blazor Server tiene un scope por circuit)
builder.Services.AddScoped<ThemeState>();
builder.Services.AddScoped<UserSessionState>();
builder.Services.AddScoped<LoginModalState>();
builder.Services.AddScoped<CtpatPanelState>();
builder.Services.AddScoped<OeaChecklistPanelState>();

// Blazor-ApexCharts
builder.Services.AddApexCharts();

// ────────────────────────────────────────────────────────────────
var app = builder.Build();

// ────────────────────────────────────────────────────────────────
// PRIMER middleware: X-Forwarded-* (DESACTIVADO — ver comentario
// arriba en el bloque de Configure<ForwardedHeadersOptions>).
// ────────────────────────────────────────────────────────────────
// app.UseForwardedHeaders();

// ────────────────────────────────────────────────────────────────
// Path base (para despliegue en subruta de IIS, p. ej. "/CCC").
// Se configura vía appsettings.json → "PathBase": "/CCC".
//
// Nota: aunque IIS registre la app como "Application" con un
// Application Path, ANCM (AspNetCoreModuleV2) NO establece
// automáticamente Request.PathBase en el pipeline de Kestrel-in-IIS
// para todos los escenarios (comprobado con la app hermana
// FDS.Southbound.Web, que aplica UsePathBase explícitamente y
// funciona). Por eso lo aplicamos SIEMPRE que "PathBase" esté en
// config, sin importar el hosting model.
// ────────────────────────────────────────────────────────────────
var pathBase = app.Configuration["PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
    app.Use((ctx, next) =>
    {
        ctx.Request.PathBase = pathBase;
        return next();
    });
}

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

// En IIS con HTTPS terminado en el frontend, evita el redirect que
// puede romper la carga inicial detrás de un reverse-proxy.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Razor + SignalR
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<ChecklistHub>(ChecklistHub.HubPath);
app.MapHub<CtpatHub>(CtpatHub.HubPath);

// Identity /Account Razor components
app.MapAdditionalIdentityEndpoints();

// ────────────────────────────────────────────────────────────────
// CTPAT: endpoints HTTP para adjuntos (upload / download / delete)
// ────────────────────────────────────────────────────────────────
var ctpatFiles = app.MapGroup("/api/ctpat/files");

ctpatFiles.MapPost("/{reviewId:int}", async (int reviewId, HttpRequest req, ICtpatFileService svc, IHubContext<CtpatHub> hub, CancellationToken ct) =>
{
    if (!req.HasFormContentType) return Results.BadRequest(new { error = "multipart/form-data expected" });
    var form = await req.ReadFormAsync(ct);
    var file = form.Files.FirstOrDefault();
    if (file is null || file.Length == 0) return Results.BadRequest(new { error = "No file uploaded" });

    var user = form["user"].ToString();
    if (string.IsNullOrWhiteSpace(user)) user = req.Headers["X-User"].ToString();
    if (string.IsNullOrWhiteSpace(user)) user = "anonymous";

    await using var stream = file.OpenReadStream();
    try
    {
        var entity = await svc.UploadAsync(reviewId, file.FileName, file.ContentType, stream, user, ct);
        await hub.Clients.All.SendAsync(CtpatHub.FileAdded, entity.ReviewId, entity.Id, user, ct);
        return Results.Json(new
        {
            id = entity.ExternalId,
            name = entity.FileName,
            size = entity.Size,
            type = entity.ContentType,
            uploadedAt = entity.UploadedAt,
            uploadedBy = entity.UploadedBy,
            url = $"/api/ctpat/files/{entity.ExternalId}"
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

ctpatFiles.MapGet("/{externalId}", async (string externalId, ICtpatFileService svc, CancellationToken ct) =>
{
    var f = await svc.GetPhysicalPathAsync(externalId, ct);
    if (f is null) return Results.NotFound();
    var (abs, name, mime) = f.Value;
    var isInline = mime is not null && (mime.StartsWith("image/") || mime == "application/pdf");
    return Results.File(abs, mime ?? "application/octet-stream", name, enableRangeProcessing: true);
});

ctpatFiles.MapDelete("/{fileId:int}", async (int fileId, ICtpatFileService svc, IHubContext<CtpatHub> hub, HttpRequest req, CancellationToken ct) =>
{
    await svc.DeleteAsync(fileId, ct);
    await hub.Clients.All.SendAsync(CtpatHub.FileDeleted, fileId, ct);
    return Results.Ok(new { ok = true });
});

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
// CTPAT export endpoints
// ────────────────────────────────────────────────────────────────
var ctpatExports = app.MapGroup("/api/ctpat/export");

ctpatExports.MapGet("/matrix.csv", async (
    int year,
    string? criterio,
    ICtpatCatalogService catalog,
    ICtpatReviewService reviews,
    CancellationToken ct) =>
{
    var questions = await catalog.GetQuestionsAsync(ct);
    var reviewMap = await reviews.GetReviewsForYearAsync(year, ct);
    var bytes = CtpatExportGenerator.MatrixToCsv(questions, reviewMap, year, criterio);

    var suffix = string.IsNullOrWhiteSpace(criterio)
        ? ""
        : "-" + SanitizeForFileName(criterio!);
    return Results.File(bytes, "text/csv", $"ctpat-matriz-{year:0000}{suffix}.csv");
});

static string SanitizeForFileName(string s)
{
    var invalid = Path.GetInvalidFileNameChars();
    var clean = new string(s.Where(c => !invalid.Contains(c) && c != ' ').ToArray());
    return string.IsNullOrEmpty(clean) ? "filtro" : clean;
}

await DbInitializer.InitializeAsync(app.Services);

app.Run();
