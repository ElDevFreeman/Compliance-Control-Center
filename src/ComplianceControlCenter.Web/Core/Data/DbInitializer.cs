using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ComplianceControlCenter.Web.Modules.Oea.Domain.Entities;
using ComplianceControlCenter.Web.Core.State;

namespace ComplianceControlCenter.Web.Core.Data;

/// <summary>
/// Inicializador de la base de datos:
///  - Aplica migraciones pendientes.
///  - Siembra roles (Admin, Editor, Reader).
///  - Crea un usuario Admin por defecto si no existe (configurable en appsettings).
///  - Siembra las ~33 actividades OEA por defecto la primera vez.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // 1) Migraciones
        await db.Database.MigrateAsync();

        // 2) Roles (modelo simple: Admin + User)
        foreach (var role in new[] { Constants.Roles.Admin, Constants.Roles.User })
        {
            if (!await roleMgr.RoleExistsAsync(role))
                await roleMgr.CreateAsync(new IdentityRole(role));
        }

        // 3) Usuario Admin inicial (número de empleado)
        var adminEmp = config["Seed:AdminEmployeeNumber"] ?? "0";
        var adminPwd = config["Seed:AdminPassword"] ?? "admin123";
        var admin = await userMgr.FindByNameAsync(adminEmp);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmp,
                EmailConfirmed = true,
                DisplayName = "Administrador"
            };
            var result = await userMgr.CreateAsync(admin, adminPwd);
            if (result.Succeeded)
            {
                await userMgr.AddToRoleAsync(admin, Constants.Roles.Admin);
                logger.LogInformation("Admin user seeded: employee #{Emp}", adminEmp);
            }
            else
            {
                logger.LogWarning("Could not seed admin: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // 4) Actividades default (solo si la tabla está vacía)
        if (!await db.Activities.AnyAsync())
        {
            db.Activities.AddRange(SeedActivities);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} default OEA activities", SeedActivities.Length);
        }
    }

    /// <summary>
    /// 33 actividades OEA default extraídas del proyecto original (index.html · DEFAULT_DATA).
    /// Fundamento: RGCE 7.1.1 fracciones I–XX, 7.1.6, 7.1.11, 7.2.1, 7.2.3, 7.2.4-5, y estándares Perfil E3.
    /// </summary>
    private static readonly Activity[] SeedActivities = new[]
    {
        new Activity { SortOrder = 10,  Item = "RGCE 7.1.1-I",    Legal = "RGCE 7.1.1 fracción I",    Name = "Alta y baja del personal",         Description = "Actualizar el listado del personal contratado directa e indirectamente",   Documents = "Listado de personal",           Owner = "RH",                Related = "Legal",         Frequency = "Mensual" },
        new Activity { SortOrder = 20,  Item = "RGCE 7.1.1-II",   Legal = "RGCE 7.1.1 fracción II",   Name = "Domicilios y establecimientos",     Description = "Actualizar domicilios donde se realizan operaciones de comercio exterior",  Documents = "Aviso de domicilios",           Owner = "Legal",             Related = "Operaciones",   Frequency = "Cuando cambie" },
        new Activity { SortOrder = 30,  Item = "RGCE 7.1.1-III",  Legal = "RGCE 7.1.1 fracción III",  Name = "Socios y accionistas",              Description = "Reportar cambios en socios, accionistas y representantes legales",           Documents = "Acta constitutiva actualizada", Owner = "Legal",             Related = "Dirección",     Frequency = "Cuando cambie" },
        new Activity { SortOrder = 40,  Item = "RGCE 7.1.1-IV",   Legal = "RGCE 7.1.1 fracción IV",   Name = "Proveedores",                       Description = "Mantener lista actualizada de proveedores nacionales y extranjeros",         Documents = "Padrón de proveedores",         Owner = "Compras",           Related = "Operaciones",   Frequency = "Mensual" },
        new Activity { SortOrder = 50,  Item = "RGCE 7.1.1-V",    Legal = "RGCE 7.1.1 fracción V",    Name = "Clientes",                          Description = "Actualizar listado de clientes principales",                                  Documents = "Padrón de clientes",            Owner = "Ventas",            Related = "Operaciones",   Frequency = "Mensual" },
        new Activity { SortOrder = 60,  Item = "RGCE 7.1.1-VI",   Legal = "RGCE 7.1.1 fracción VI",   Name = "Transportistas",                    Description = "Verificar transportistas autorizados y sus vehículos",                        Documents = "Contratos y RFC",               Owner = "Logística",         Related = "Seguridad",     Frequency = "Trimestral" },
        new Activity { SortOrder = 70,  Item = "RGCE 7.1.1-VII",  Legal = "RGCE 7.1.1 fracción VII",  Name = "Agentes aduanales",                 Description = "Mantener actualizada relación de agentes/apoderados aduanales",               Documents = "Poderes vigentes",              Owner = "Comercio Exterior", Related = "Legal",         Frequency = "Semestral" },
        new Activity { SortOrder = 80,  Item = "RGCE 7.1.1-VIII", Legal = "RGCE 7.1.1 fracción VIII", Name = "Mercancías",                        Description = "Descripción detallada de mercancías importadas/exportadas",                   Documents = "Catálogo de mercancías",        Owner = "Operaciones",       Related = "CE",            Frequency = "Mensual" },
        new Activity { SortOrder = 90,  Item = "RGCE 7.1.1-IX",   Legal = "RGCE 7.1.1 fracción IX",   Name = "Programas de fomento",              Description = "Vigencia de IMMEX, PROSEC y otros programas",                                 Documents = "Autorizaciones",                Owner = "Comercio Exterior", Related = "Legal",         Frequency = "Anual" },
        new Activity { SortOrder = 100, Item = "RGCE 7.1.1-X",    Legal = "RGCE 7.1.1 fracción X",    Name = "Infracciones y sanciones",          Description = "Notificar infracciones o sanciones en materia fiscal/aduanera",               Documents = "Resoluciones",                  Owner = "Legal",             Related = "Dirección",     Frequency = "Cuando ocurra" },
        new Activity { SortOrder = 110, Item = "RGCE 7.1.1-XI",   Legal = "RGCE 7.1.1 fracción XI",   Name = "Cambios operativos",                Description = "Comunicar cambios sustanciales en la operación",                              Documents = "Aviso al SAT",                  Owner = "Dirección",         Related = "Legal",         Frequency = "Cuando cambie" },
        new Activity { SortOrder = 120, Item = "RGCE 7.1.1-XII",  Legal = "RGCE 7.1.1 fracción XII",  Name = "Auditorías internas",               Description = "Realizar auditorías internas de cumplimiento OEA",                            Documents = "Reporte de auditoría",          Owner = "Compliance",        Related = "Todos",         Frequency = "Mensual" },
        new Activity { SortOrder = 130, Item = "RGCE 7.1.1-XIII", Legal = "RGCE 7.1.1 fracción XIII", Name = "Capacitación",                      Description = "Capacitar al personal en temas de seguridad y comercio exterior",             Documents = "Constancias",                   Owner = "RH",                Related = "Seguridad",     Frequency = "Trimestral" },
        new Activity { SortOrder = 140, Item = "RGCE 7.1.1-XIV",  Legal = "RGCE 7.1.1 fracción XIV",  Name = "Seguridad física",                  Description = "Verificar controles de seguridad en instalaciones",                           Documents = "Reporte de seguridad",          Owner = "Seguridad",         Related = "Operaciones",   Frequency = "Mensual" },
        new Activity { SortOrder = 150, Item = "RGCE 7.1.1-XV",   Legal = "RGCE 7.1.1 fracción XV",   Name = "Control de accesos",                Description = "Bitácora de accesos a áreas restringidas",                                    Documents = "Bitácora",                      Owner = "Seguridad",         Related = "RH",            Frequency = "Mensual" },
        new Activity { SortOrder = 160, Item = "RGCE 7.1.1-XVI",  Legal = "RGCE 7.1.1 fracción XVI",  Name = "Seguridad de contenedores",         Description = "Inspección de 7 puntos en contenedores",                                      Documents = "Checklist de inspección",       Owner = "Logística",         Related = "Seguridad",     Frequency = "Por embarque" },
        new Activity { SortOrder = 170, Item = "RGCE 7.1.1-XVII", Legal = "RGCE 7.1.1 fracción XVII", Name = "Sellos de seguridad",               Description = "Uso y control de sellos ISO 17712",                                           Documents = "Bitácora de sellos",            Owner = "Logística",         Related = "Seguridad",     Frequency = "Por embarque" },
        new Activity { SortOrder = 180, Item = "RGCE 7.1.1-XVIII",Legal = "RGCE 7.1.1 fracción XVIII",Name = "Tecnología de información",         Description = "Respaldos y seguridad informática",                                           Documents = "Política TI",                   Owner = "TI",                Related = "Compliance",    Frequency = "Mensual" },
        new Activity { SortOrder = 190, Item = "RGCE 7.1.1-XIX",  Legal = "RGCE 7.1.1 fracción XIX",  Name = "Manejo de crisis",                  Description = "Plan de continuidad y respuesta a incidentes",                                Documents = "Plan de contingencia",          Owner = "Dirección",         Related = "Todos",         Frequency = "Semestral" },
        new Activity { SortOrder = 200, Item = "RGCE 7.1.1-XX",   Legal = "RGCE 7.1.1 fracción XX",   Name = "Análisis de riesgos",               Description = "Matriz de riesgos actualizada",                                               Documents = "Matriz de riesgos",             Owner = "Compliance",        Related = "Todos",         Frequency = "Trimestral" },
        new Activity { SortOrder = 210, Item = "RGCE 7.1.6",      Legal = "RGCE 7.1.6",               Name = "Reporte de cumplimiento",           Description = "Presentar reporte anual de cumplimiento OEA",                                 Documents = "Reporte anual",                 Owner = "Compliance",        Related = "Dirección",     Frequency = "Anual" },
        new Activity { SortOrder = 220, Item = "RGCE 7.1.11",     Legal = "RGCE 7.1.11",              Name = "Renovación OEA",                    Description = "Iniciar proceso de renovación 6 meses antes",                                 Documents = "Solicitud de renovación",       Owner = "Compliance",        Related = "Dirección",     Frequency = "Cada 3 años" },
        new Activity { SortOrder = 230, Item = "RGCE 7.2.1",      Legal = "RGCE 7.2.1",               Name = "Registro de operaciones",           Description = "Bitácora electrónica de operaciones de CE",                                   Documents = "Sistema/bitácora",              Owner = "Comercio Exterior", Related = "TI",            Frequency = "Diaria" },
        new Activity { SortOrder = 240, Item = "RGCE 7.2.3",      Legal = "RGCE 7.2.3",               Name = "Conservación de documentos",        Description = "Archivo de pedimentos y anexos 5 años",                                       Documents = "Archivo físico/digital",        Owner = "Comercio Exterior", Related = "TI",            Frequency = "Continua" },
        new Activity { SortOrder = 250, Item = "RGCE 7.2.4-5",    Legal = "RGCE 7.2.4 y 7.2.5",       Name = "Discrepancias en operaciones",      Description = "Procedimiento para corregir discrepancias",                                   Documents = "Procedimiento",                 Owner = "Comercio Exterior", Related = "Legal",         Frequency = "Cuando ocurra" },
        new Activity { SortOrder = 260, Item = "E3 · 1.1",        Legal = "Perfil E3 · Estándar 1.1", Name = "Socio comercial",                    Description = "Evaluación y selección de socios comerciales",                                Documents = "Cuestionario de socio",         Owner = "Compras/Ventas",    Related = "Seguridad",     Frequency = "Anual" },
        new Activity { SortOrder = 270, Item = "E3 · 1.4",        Legal = "Perfil E3 · Estándar 1.4", Name = "Contratos con socios",               Description = "Cláusulas de seguridad en contratos",                                         Documents = "Contratos",                     Owner = "Legal",             Related = "Compras",       Frequency = "Cuando cambie" },
        new Activity { SortOrder = 280, Item = "E3 · 3.1",        Legal = "Perfil E3 · Estándar 3.1", Name = "Seguridad del contenedor",           Description = "Inspección previa al llenado",                                                Documents = "Formato de inspección",         Owner = "Logística",         Related = "Seguridad",     Frequency = "Por embarque" },
        new Activity { SortOrder = 290, Item = "E3 · 4",          Legal = "Perfil E3 · Estándar 4",   Name = "Controles de acceso físico",         Description = "Identificación de empleados, visitantes y vendedores",                        Documents = "Bitácora accesos",              Owner = "Seguridad",         Related = "RH",            Frequency = "Diaria" },
        new Activity { SortOrder = 300, Item = "E3 · 7",          Legal = "Perfil E3 · Estándar 7",   Name = "Seguridad de procedimientos",        Description = "Manejo seguro de carga entrante/saliente",                                    Documents = "Procedimientos",                Owner = "Logística",         Related = "Seguridad",     Frequency = "Mensual" },
        new Activity { SortOrder = 310, Item = "E3 · 8.1",        Legal = "Perfil E3 · Estándar 8.1", Name = "Seguridad física de instalaciones", Description = "Cercas, iluminación, cerraduras",                                              Documents = "Inspección",                    Owner = "Seguridad",         Related = "Mantenimiento", Frequency = "Mensual" },
        new Activity { SortOrder = 320, Item = "E3 · 8",          Legal = "Perfil E3 · Estándar 8",   Name = "Seguridad TI",                       Description = "Passwords, respaldos, antivirus",                                              Documents = "Política TI",                   Owner = "TI",                Related = "Compliance",    Frequency = "Mensual" },
        new Activity { SortOrder = 330, Item = "E3 · 10",         Legal = "Perfil E3 · Estándar 10",  Name = "Capacitación en seguridad",          Description = "Programa anual de capacitación",                                              Documents = "Constancias",                   Owner = "RH",                Related = "Seguridad",     Frequency = "Anual" }
    };
}
