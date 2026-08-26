# Checklist OEA — .NET 8 · Blazor Server · EF Core

Aplicación web multiusuario para seguimiento del cumplimiento OEA
(RGCE 7.1.1, Perfil E3). Migración del proyecto Python original a un
stack empresarial de Microsoft.

## Stack

| Capa | Tecnología |
|---|---|
| Runtime | .NET 8 (LTS) |
| UI | **Blazor Server** con render mode `InteractiveServer` |
| ORM | **Entity Framework Core 8** (SQL Server provider) |
| BD | SQL Server (server compartido `RYCP1SQL20CP`, base `FDS_DEV_RYR9`) |
| Auth | ASP.NET Core Identity con roles `Admin` / `Editor` / `Reader` |
| Realtime | SignalR (`ChecklistHub`) para push entre usuarios |
| Estilos | **Tailwind CSS v4 + DaisyUI v5** (temas `winter` / `dim`) |
| Charts | Blazor-ApexCharts |
| Export | ClosedXML (Excel) |

## Convención de tablas

TODAS las tablas llevan prefijo `OEA_` para poder convivir sin chocar en la
base de datos compartida del Warehouse. Esto aplica también a las tablas
generadas por ASP.NET Identity.

**Tablas de negocio:**

- `OEA_Activities`
- `OEA_MonthlyStatus`
- `OEA_Comments`
- `OEA_AuditLog`

**Tablas de Identity (renombradas en `AppDbContext.OnModelCreating`):**

- `OEA_Users` (AspNetUsers)
- `OEA_Roles` (AspNetRoles)
- `OEA_UserRoles` · `OEA_UserClaims` · `OEA_UserLogins` · `OEA_UserTokens` · `OEA_RoleClaims`

## Estructura de la solución

```
OEA.Checklist/
├── OEA.Checklist.sln
├── SQL/                                 (scripts SQL opcionales)
└── src/
    └── OEA.Checklist.Web/               (proyecto único, monolítico)
        ├── Components/
        │   ├── Account/                 (páginas de Identity generadas)
        │   ├── Layout/                  (MainLayout, NavMenu, ThemeToggle)
        │   ├── Pages/                   (Checklist, Matrix, History, Users, Error)
        │   └── Shared/                  (StatusBadge, KpiCard, ...)
        ├── Data/
        │   ├── AppDbContext.cs          ← DbContext con prefijos OEA_
        │   ├── ApplicationUser.cs       ← extiende IdentityUser
        │   ├── DbInitializer.cs         ← migrate + seed roles/admin/actividades
        │   └── Migrations/              ← generadas por EF Core
        ├── Domain/
        │   ├── Entities/                (Activity, MonthlyStatus, Comment, AuditLog)
        │   └── Enums/                   (ComplianceStatus)
        ├── Services/                    (Checklist, Matrix, Comment, Audit)
        │   └── AuditSaveChangesInterceptor.cs
        ├── Hubs/                        (ChecklistHub — SignalR)
        ├── State/                       (ThemeState, UserSessionState)
        ├── Styles/app.css               ← Tailwind + DaisyUI
        ├── wwwroot/css/site.css         ← generado por Tailwind
        └── Program.cs
```

## Setup — primera vez

### 1. Requisitos

- **.NET 8 SDK**
- **Node.js 18+** (para compilar Tailwind) — recomendado con **pnpm**
- Acceso al SQL Server `RYCP1SQL20CP`, base `FDS_DEV_RYR9`
- **EF Core tools** globales: `dotnet tool install --global dotnet-ef`

### 2. Restaurar dependencias

```powershell
cd C:\Users\freemanc6\Desktop\Projects\Warehouse\MicroProjects\OEA.Checklist
dotnet restore

cd src\OEA.Checklist.Web
pnpm install                # o: npm install
```

### 3. Compilar Tailwind (una vez o en modo watch)

```powershell
# build único
pnpm run css:build

# durante desarrollo (watch)
pnpm run css:watch
```

### 4. Aplicar migraciones

La migración inicial se creó con el proyecto. Para aplicarla:

```powershell
cd src\OEA.Checklist.Web
dotnet ef database update
```

> El `DbInitializer` también corre `Database.MigrateAsync()` al arranque
> del app, así que si prefieres puedes solo hacer `dotnet run`.

### 5. Correr el proyecto

```powershell
dotnet run
```

O desde Visual Studio 2022 con F5.

Al arrancar por primera vez, `DbInitializer` hace:

1. Aplica migraciones pendientes.
2. Crea roles `Admin`, `Editor`, `Reader`.
3. Crea usuario admin: **admin@oea.local / Admin!2025** (cambiable en `appsettings.json` → `Seed`).
4. Siembra las 33 actividades OEA por defecto.

Login inicial: `admin@oea.local` / `Admin!2025`.

## Crear nueva migración

Cuando modifiques entidades:

```powershell
cd src\OEA.Checklist.Web
dotnet ef migrations add NombreDescriptivo
dotnet ef database update
```

## Deploy

- **IIS**: publicar con `dotnet publish -c Release` y hospedar en un
  sitio con módulo ASP.NET Core.
- **Docker**: pendiente de agregar `Dockerfile`.
- **Azure App Service**: compatible con el runtime de .NET 8.

## Endpoints principales (SignalR + páginas)

| Ruta | Roles | Descripción |
|---|---|---|
| `/` | Authenticated | Checklist mensual |
| `/matrix` | Authenticated | Matriz de cumplimiento por mes |
| `/history` | Admin | Auditoría (`OEA_AuditLog`) |
| `/users` | Admin | Gestión de usuarios |
| `/hubs/checklist` | Authenticated | Hub SignalR |
| `/Account/*` | Anonymous / Authenticated | Login / registro / gestión de cuenta |

## Notas de migración desde el proyecto Python

- El **payload JSON** de `state` en `oea.db` puede portarse con un migrador
  puntual (aún no implementado).
- El modelo `monthly[YYYY-MM]` del original se normalizó a la tabla
  `OEA_MonthlyStatus` con `(ActivityId, Year, Month)` único.
- La contraseña compartida `APP_PASSWORD` se reemplazó por autenticación
  real (Identity + roles).
- El polling cada 30 s se reemplazó por push SignalR (el Hub ya está
  registrado; falta cablear notificaciones en los servicios).

## Pendientes / next steps

- [ ] Completar UI editable de `Checklist.razor` (edit inline por celda).
- [ ] Implementar `Matrix.razor` completo (tabla pivotada + gráfica ApexCharts).
- [ ] Componente `ActivityDrawer.razor` con comentarios e historial mensual.
- [ ] Broadcast SignalR desde `ChecklistService` tras cada save.
- [ ] Export CSV / XLSX de checklist y matriz.
- [ ] Página `Users.razor` con alta / edición de roles.
- [ ] Migrador one-off desde `oea.db` (Python) hacia SQL Server.
