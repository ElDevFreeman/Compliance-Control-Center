# Compliance Control Center — .NET 8 · Blazor Server · EF Core

Aplicación web multiusuario para el seguimiento de cumplimiento normativo del
Warehouse. Integra dos programas de certificación en una sola solución:

- **OEA** — Operador Económico Autorizado (RGCE 7.1.1 / Perfil E3)
- **CTPAT** — Customs Trade Partnership Against Terrorism (139 preguntas)

Migrado de un prototipo Python original a un stack empresarial de Microsoft.

---

## Stack

| Capa       | Tecnología                                                                   |
| ---------- | ---------------------------------------------------------------------------- |
| Runtime    | .NET 8 (LTS)                                                                 |
| UI         | **Blazor Server** con render mode `InteractiveServer` global                 |
| ORM        | **Entity Framework Core 8** (SQL Server provider)                            |
| BD         | SQL Server compartido — server `RYCP1SQL20CP`, base `FDS_DEV_RYR9`           |
| Auth       | ASP.NET Core Identity con roles `Admin` / `User`                             |
| Realtime   | SignalR — `ChecklistHub` (OEA) y `CtpatHub` (CTPAT)                          |
| Estilos    | **Tailwind CSS v4 + DaisyUI v5** (temas `winter` claro / `dim` oscuro)       |
| Charts     | Blazor-ApexCharts                                                            |
| Export     | ClosedXML (Excel) + CSV                                                      |

---

## Estructura de la solución

```
ComplianceControlCenter/
├── ComplianceControlCenter.sln
├── SQL/                                          (scripts SQL opcionales)
├── AGENTS.md                                     (guía para agentes / colaboradores)
└── src/
    └── ComplianceControlCenter.Web/              (proyecto único, monolítico)
        ├── Core/
        │   ├── Data/                             AppDbContext, ApplicationUser, DbInitializer, Migrations/
        │   ├── Domain/                           Entidades compartidas (AuditLog)
        │   ├── Identity/                         Claim factory, revalidating auth state provider
        │   ├── Services/                         AuditSaveChangesInterceptor
        │   └── State/                            ThemeState, UserSessionState, LoginModalState (scoped)
        ├── Modules/
        │   ├── Oea/                              Módulo OEA (Activities, MonthlyStatus, Comments, Matrix, Export)
        │   └── Ctpat/                            Módulo CTPAT (Catalog, Reviews, FileUploads, Hub)
        │       └── Data/ctpat_data.json          Catálogo de 139 preguntas (copiado a output)
        ├── Components/                           App shell, Account pages (Identity), Pages/, Routes.razor
        ├── Shared/                               Layout (MainLayout, NavMenu), componentes compartidos
        ├── Styles/app.css                        Tailwind + DaisyUI (fuente)
        ├── wwwroot/
        │   ├── css/site.css                      Generado por Tailwind (no se commitea)
        │   └── uploads/ctpat/<reviewId>/         Archivos subidos por revisión CTPAT
        └── Program.cs
```

Existen dos `_Imports.razor`: `Components/_Imports.razor` y `Shared/_Imports.razor`.
Ambos son relevantes al agregar nuevos `@using`.

---

## Convención de tablas

Todas las tablas llevan prefijo para poder convivir sin chocar en la base
compartida del Warehouse:

- `CCC_*` — núcleo compartido (Identity, `CCC_AuditLog`)
- `CCC_OEA_*` — módulo OEA (`CCC_OEA_Activities`, `CCC_OEA_MonthlyStatus`, `CCC_OEA_Comments`)
- `CCC_CTPAT_*` — módulo CTPAT (catálogo, revisiones, archivos)

Las tablas de ASP.NET Identity se renombran en `AppDbContext.OnModelCreating`
(por ejemplo `AspNetUsers` → `CCC_Users`).

Al agregar una tabla nueva:

1. Aplicar el prefijo `CCC_<MODULE>_` en `OnModelCreating`.
2. Si pertenece al módulo CTPAT, registrarla también en
   `CtpatDbContextExtensions.cs` (`builder.AddCtpatModule()`).

---

## Identity / autenticación

- `UserName` es el **número de empleado** (solo dígitos, no email).
  `RequireUniqueEmail = false`.
- Política de contraseñas relajada intencionalmente: 6–20 caracteres,
  sin requisitos de complejidad.
- Roles: `Admin` y `User`.
- Credenciales sembradas por defecto: empleado `0` / contraseña `admin123`
  (configurable en `appsettings.json → Seed`).
- Ruta de login: `/Account/Login`.

---

## Setup — primera vez

### 1. Requisitos

- **.NET 8 SDK**
- **Node.js 18+** con **pnpm** (para el pipeline de Tailwind)
- Acceso al SQL Server `RYCP1SQL20CP`, base `FDS_DEV_RYR9`
- **EF Core tools** globales: `dotnet tool install --global dotnet-ef`

### 2. Restaurar dependencias

Desde la raíz del repo:

```powershell
dotnet restore
```

Desde `src\ComplianceControlCenter.Web\`:

```powershell
pnpm install
```

> `node_modules` debe existir antes del primer `dotnet build`.
> Si falta, el target `EnsureNodeModules` del `.csproj` corre `pnpm install`
> automáticamente.

### 3. Configurar la conexión (opcional)

La cadena de conexión vive en `appsettings.json`. Para sobrescribirla
localmente sin tocar el archivo commiteado, usa una de estas opciones:

- `appsettings.Development.Local.json` (ignorado por git)
- User Secrets: `dotnet user-secrets set ...`
  (`UserSecretsId: 85fd7f56-0236-4afe-a45c-9bee098cb7b4`)

Ver `CONFIGURATION_SECRETS.md` antes de agregar nuevas claves.

### 4. Compilar y correr

```powershell
# Desde src\ComplianceControlCenter.Web\
dotnet run
```

El proyecto escucha en:

- HTTP:  `http://localhost:5137`
- HTTPS: `https://localhost:7264`

Al arrancar, `DbInitializer.InitializeAsync` hace, en orden:

1. Aplica migraciones pendientes (`Database.MigrateAsync()`).
2. Crea los roles `Admin` y `User` si no existen.
3. Crea el usuario admin sembrado (empleado `0` / `admin123`).
4. Siembra las 33 actividades OEA por defecto (solo si la tabla está vacía).
5. Siembra el catálogo CTPAT desde `Modules/Ctpat/Data/ctpat_data.json`
   de forma idempotente (por `ExternalId`).

---

## Pipeline de Tailwind / CSS

- Fuente: `Styles/app.css` → salida: `wwwroot/css/site.css` (generado, no
  se commitea).
- Tailwind v4 + DaisyUI v5. Temas `winter` (default) y `dim`. El toggle en
  runtime lo maneja `ThemeState` (servicio Blazor scoped).
- **No editar `wwwroot/css/site.css` a mano** — se regenera en cada build.

`dotnet build` corre automáticamente:

1. `EnsureNodeModules` → `pnpm install` si falta `node_modules`.
2. `BuildTailwind` → regenera `site.css` desde `Styles/app.css` y las
   clases usadas en los `.razor`. La verificación incremental compara
   `@(TailwindWatchInputs)` contra el `.css` de salida; si nada cambió,
   el target se omite.
3. En `Release` se añade `--minify` automáticamente.

Scripts manuales (durante desarrollo):

```powershell
# Desde src\ComplianceControlCenter.Web\
pnpm run css:watch      # rebuild en cada cambio
pnpm run css:build      # rebuild único minificado
```

`tailwind.extension.json` es solo para la extensión "Tailwind CSS for
Visual Studio" (usa `UseCli: false`, modo extensión, no CLI).

---

## Migraciones EF Core

Cuando cambien las entidades:

```powershell
# Desde src\ComplianceControlCenter.Web\
dotnet ef migrations add NombreDescriptivo
dotnet ef database update
```

En arranque, `DbInitializer` ya corre `Database.MigrateAsync()`, así que
un `dotnet run` también aplica migraciones pendientes.

---

## Módulos

### OEA

- Checklist mensual: un `MonthlyStatus` por `(ActivityId, Year, Month)` — con
  índice único.
- `MatrixService` genera la matriz de cumplimiento pivotada por mes.
- `ChecklistHub` en `/hubs/checklist` — hace broadcast a todos los clientes
  cuando cambia el status de una actividad.
- Endpoints de export:
  - `GET /api/export/checklist.csv`
  - `GET /api/export/checklist.xlsx`
  - `GET /api/export/matrix.xlsx`

### CTPAT

- Catálogo de 139 preguntas cargado al inicio desde
  `Modules/Ctpat/Data/ctpat_data.json` (copiado al output). El seeder es
  idempotente por `ExternalId`.
- Uploads guardados en `wwwroot/uploads/ctpat/<reviewId>/`. Límite: **25 MB
  por archivo**.
- Endpoints REST:
  - `POST /api/ctpat/files/{reviewId}`
  - `GET  /api/ctpat/files/{externalId}`
  - `DELETE /api/ctpat/files/{fileId}`
- `CtpatHub` — notifica eventos `FileAdded` / `FileDeleted`.

---

## Convenciones de código

- Render mode global: `InteractiveServer`. **No hay WebAssembly**.
- Todos los servicios de estado (`ThemeState`, `UserSessionState`,
  `LoginModalState`) son `AddScoped` — una instancia por circuito Blazor.
- `AuditSaveChangesInterceptor` intercepta cada `SaveChanges` para escribir
  al `CCC_AuditLog`. **No hacer bypass a EF Core** en entidades auditadas.
- Nuevas tablas → prefijo `CCC_<MODULE>_` en `OnModelCreating`.

---

## Publicación (deploy)

```powershell
# Desde src\ComplianceControlCenter.Web\
dotnet publish -c Release
```

En Release, Tailwind se compila con `--minify`. El target `EnsureLogsFolder`
crea la carpeta `logs/` en el output (necesaria para ANCM cuando
`stdoutLogEnabled=true` en IIS).

Destinos soportados:

- **IIS** (con módulo ASP.NET Core V2)
- **Azure App Service** (.NET 8)
- Docker: `Dockerfile` pendiente

---

## Rutas principales

| Ruta                        | Acceso                        | Descripción                   |
| --------------------------- | ----------------------------- | ----------------------------- |
| `/`                         | Authenticated                 | Dashboard / Checklist OEA     |
| `/matrix`                   | Authenticated                 | Matriz OEA pivotada por mes   |
| `/history`                  | `Admin`                       | Auditoría (`CCC_AuditLog`)    |
| `/users`                    | `Admin`                       | Gestión de usuarios           |
| `/ctpat/*`                  | Authenticated                 | Revisiones CTPAT              |
| `/hubs/checklist`           | Authenticated                 | Hub SignalR OEA               |
| `/ctpat-hub`                | Authenticated                 | Hub SignalR CTPAT             |
| `/Account/*`                | Anonymous / Authenticated     | Identity (login, logout, ...) |

---

## Testing

No hay tests automatizados aún. Verificación manual mínima antes de un PR:

1. `pnpm run css:build` termina sin errores.
2. `dotnet build` termina sin errores.
3. `dotnet run` arranca y `DbInitializer` loguea migración + seed OK.
4. Login con empleado `0` / `admin123` funciona.

---

## Referencias en el repo

- `AGENTS.md` — guía para agentes de IA / colaboradores nuevos.
- `SOUTHBOUND_REFERENCE_GUIDE.md` — patrones DaisyUI/Tailwind del proyecto
  de referencia.
- `DAISYUI_COMPONENTS_EXAMPLES.md` — snippets copy-paste de componentes.
- `SETUP_GUIDE.md` — pasos de setup (algunas secciones son históricas; el
  `.csproj` ahora automatiza Tailwind).
- `CONFIGURATION_SECRETS.md` — notas de secrets/config.
- `ANALYSIS_SUMMARY.md` / `INDEX.md` — análisis y navegación del repo.
