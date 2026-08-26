# Configuración de Secretos y Variables de Entorno

## Seguridad: Protección de Credenciales

Este proyecto **NUNCA** debe commitear credenciales reales en git. La siguiente estructura explica cómo se protegen:

### Archivos Sensibles (Protegidos en .gitignore)

Los siguientes archivos están configurados en `.gitignore` y no deben ser commiteados:

- `appsettings.Development.Local.json` - **Credenciales locales de desarrollo** ⚠️
- `appsettings.Production.Local.json` - **Credenciales de producción**
- `appsettings.*.Local.json` - **Cualquier archivo local personalizado**
- `secrets.json` - **Secretos encriptados**
- `.env` - **Variables de entorno**

**NOTA**: `appsettings.json` y `appsettings.Development.json` **SÍ se commiten**, pero el `.gitignore` ignora los archivos `.Local.json` que contienen las credenciales reales.

## Estructura de Configuración (Opción B - Recomendada)

ASP.NET Core carga la configuración en este orden (la última sobrescribe las anteriores):

```
1. appsettings.json                  (base con credenciales de desarrollo)
2. appsettings.{Environment}.json    (ej: appsettings.Development.json)
3. appsettings.{Environment}.Local.json  (local, NO commiteado, con credenciales reales) ← Sobrescribe todo
```

### Cómo Funciona:

**Archivo commiteado:**
```
appsettings.json
├─ ConnectionString = "Server=RYCP1SQL20CP;Database=FDS_DEV_RYR9;User Id=RYAdminUser;Password=!9pDjcX01QysJc2ftpTs;..."
└─ Se commitea al repositorio (todos lo ven)
```

**Archivo local (NO commiteado):**
```
appsettings.Development.Local.json
├─ Sobrescribe los valores de appsettings.json
├─ NO se commitea (protegido por .gitignore)
└─ Solo existe en tu máquina local
```

## Configuración Local

### Opción B: appsettings.Development.Local.json (ACTUAL)

Ya está creado en el repositorio:

**Archivo:** `src/OEA.Checklist.Web/appsettings.Development.Local.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=RYCP1SQL20CP;Database=FDS_DEV_RYR9;User Id=RYAdminUser;Password=!9pDjcX01QysJc2ftpTs;TrustServerCertificate=True;"
  },
  "Seed": {
    "AdminPassword": "admin123"
  },
  "DetailedErrors": true,
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

**Ventajas:**
- ✅ Simple: Solo copia `appsettings.Development.Local.json` en tu máquina
- ✅ Seguro: Protegido automáticamente por `.gitignore`
- ✅ Flexible: Cada desarrollador puede tener credenciales diferentes
- ✅ Automático: ASP.NET Core carga el `.Local.json` automáticamente

### Para Nuevo Desarrollador:

1. Clonar el repositorio
2. **Copiar** `appsettings.example.json` → `appsettings.Development.Local.json`
3. **Editar** `appsettings.Development.Local.json` con tus credenciales reales
4. Ejecutar: `dotnet run`

```powershell
# Ejemplo (en Windows)
cd src\OEA.Checklist.Web
copy appsettings.example.json appsettings.Development.Local.json
# Editar appsettings.Development.Local.json con credenciales reales
dotnet run
```

### Alternativa: Variables de Entorno (Opcional)

Si prefieres no tener archivos locales, puedes usar variables de entorno:

```powershell
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection = "Server=RYCP1SQL20CP;Database=FDS_DEV_RYR9;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
$env:Seed__AdminPassword = "Admin!2025"

# O en CMD
set ConnectionStrings__DefaultConnection=Server=RYCP1SQL20CP;Database=FDS_DEV_RYR9;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;
```

## Archivos Ejemplo

Se incluyen dos archivos de ejemplo en el repositorio para referencia:

- **`appsettings.example.json`** - Muestra la estructura con valores placeholder
- **`appsettings.Development.example.json`** - Configuración recomendada para desarrollo

Puedes usar estos como punto de partida para crear tus archivos locales.

## Flujo de Desarrollo

```
1. Developer clona el repositorio
   ↓
2. Copia appsettings.example.json → appsettings.Development.Local.json
   ↓
3. Edita con credenciales reales (solo localmente)
   ↓
4. ASP.NET Core carga automáticamente:
   appsettings.json + appsettings.Development.json + appsettings.Development.Local.json
   ↓
5. Hace commit/push (appsettings.Development.Local.json NO se commitea)
```

## Para Producción

En producción, configura las credenciales usando:

- **Azure Key Vault** - Almacenamiento seguro en la nube
- **Environment Variables en el servidor** - IIS, Docker, App Service
- **appsettings.Production.Local.json** - En el servidor (NO en git)
- **Archivos de configuración encriptados** - Con `DataProtectionProvider`

## Checklist de Seguridad

Antes de cada commit, verifica:

- [ ] `appsettings.Development.Local.json` está en `.gitignore`
- [ ] No commiteaste `appsettings.Development.Local.json` por error
- [ ] Las credenciales reales están SOLO en archivos `.Local.json` o env vars
- [ ] `git status` no muestra ningún archivo `.Local.json`
- [ ] Los archivos `.example.json` están commiteados (sin credenciales)

```powershell
# Comando para verificar que no hay archivos locales en staging
git diff --cached --name-only | findstr "Local"
# Si no devuelve nada, está todo bien ✅
```

## Referencias

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration)
- [Safe storage of app secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Configuration providers in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers)
