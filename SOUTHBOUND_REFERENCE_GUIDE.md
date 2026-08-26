# Southbound.Client - DaisyUI Reference Guide

Documento de referencia del proyecto **Southbound.Client** que utiliza **Tailwind CSS v4**, **DaisyUI v5** y **Blazor WebAssembly**. Este proyecto es la base para rediseñar ComplianceControlCenter.

---

## Tabla de Contenidos

1. [Estructura de Carpetas](#estructura-de-carpetas)
2. [Configuración de Herramientas](#configuración-de-herramientas)
3. [Sistema de Temas (Light/Dark)](#sistema-de-temas)
4. [Componentes DaisyUI Utilizados](#componentes-daisyui-utilizados)
5. [Ejemplos de Componentes Personalizados](#ejemplos-de-componentes-personalizados)
6. [Patrones y Mejores Prácticas](#patrones-y-mejores-prácticas)
7. [Scripts JavaScript](#scripts-javascript)

---

## Estructura de Carpetas

```
Southbound.Client/
├── App.razor                      # Enrutador principal
├── Program.cs                     # Configuración de servicios e inyección de dependencias
├── _Imports.razor                 # Imports globales (usando statements)
├── package.json                   # Dependencias npm (Tailwind, DaisyUI)
├── tailwind.extension.json        # Configuración de la extensión de Tailwind para VS
│
├── Pages/                         # Páginas Blazor (routed components)
│   ├── Home.razor                # Dashboard principal
│   ├── EntrySearch.razor         # Búsqueda de entradas
│   ├── CaptureImport.razor       # Captura/Importación
│   ├── Billing.razor             # Facturación IE
│   └── Discrepancies.razor       # Gestión de discrepancias
│
├── Layout/                        # Componentes de layout
│   ├── MainLayout.razor          # Layout principal (drawer + navbar)
│   └── NavMenu.razor             # Sidebar colapsable con navegación
│
├── Components/                    # Componentes reutilizables
│   ├── EmptyState.razor          # Placeholder para estados vacíos
│   ├── ModuleGuard.razor         # Gate de autenticación por módulo
│   └── ModuleLoginModal.razor    # Modal de login del módulo
│
├── State/                         # Gestión de estado (Blazor services)
│   ├── ThemeState.cs             # Control de tema (light/dark)
│   ├── LocalizationState.cs      # Control de idioma (en/es)
│   └── UserSessionState.cs       # Sesiones de usuario por módulo
│
├── Services/                      # Servicios HTTP
│   ├── AccessHttpService.cs      # Autenticación de módulos
│   ├── DashboardHttpService.cs   # Datos del dashboard
│   ├── EntrySearchHttpService.cs # Búsqueda de entradas
│   ├── CaptureHttpService.cs     # Captura/Importación
│   ├── BillingHttpService.cs     # Facturación
│   └── DiscrepancyHttpService.cs # Discrepancias
│
├── Styles/                        # Estilos CSS
│   └── app.css                   # Configuración de Tailwind + DaisyUI
│
├── wwwroot/                       # Activos estáticos
│   ├── index.html                # HTML base (incluye splash screen animado)
│   ├── css/
│   │   ├── site.css              # CSS compilado (compilado por Tailwind)
│   │   └── app.css               # CSS compilado adicional
│   └── js/
│       ├── download.js           # Utilidades de descarga + scripts de tema/sesión
│       └── _framework/           # Framework Blazor (auto-generado)
│
└── Properties/
    └── launchSettings.json       # Configuración de ejecución

```

---

## Configuración de Herramientas

### package.json

```json
{
  "name": "southbound-client",
  "version": "1.0.0",
  "private": true,
  "scripts": {
    "css:watch": "tailwindcss -i ./Styles/app.css -o ./wwwroot/css/site.css --watch",
    "css:build": "tailwindcss -i ./Styles/app.css -o ./wwwroot/css/site.css --minify"
  },
  "devDependencies": {
    "@tailwindcss/cli": "^4",
    "tailwindcss": "^4",
    "daisyui": "^5"
  }
}
```

**Instalación:**
```bash
pnpm install
# o npm install
```

**Compilación de estilos (desarrollo):**
```bash
pnpm run css:watch
```

**Compilación de estilos (producción):**
```bash
pnpm run css:build
```

### Styles/app.css

```css
@import "tailwindcss";
@plugin "daisyui" {
  themes: winter --default, dim --prefersdark;
}

@theme {
  --font-sans: "Inter", system-ui, sans-serif;
}

@source "../Pages/**/*.razor";
@source "../Layout/**/*.razor";
@source "../Components/**/*.razor";

@utility fade-in {
  animation: fadeSlideIn 0.3s ease-out;
}

@keyframes fadeSlideIn {
  from {
    opacity: 0;
    transform: translateY(8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
```

**Notas de configuración:**
- **Temas**: `winter` (por defecto, claro) y `dim` (preferencia por tema oscuro)
- **Font**: Inter (desde CDN o sistema)
- **Utility personalizada**: `fade-in` para animaciones suaves de entrada

### tailwind.extension.json

Configuración para la extensión Tailwind CSS en Visual Studio:

```json
{
  "$schema": "https://raw.githubusercontent.com/theron-wang/Tailwind-CSS-for-Visual-Studio/refs/heads/main/tailwind.extension.schema.json",
  "BuildFiles": [
    {
      "Input": "styles\\app.css",
      "Output": "",
      "Behavior": "Default"
    }
  ],
  "PackageConfigurationFile": null,
  "CustomRegexes": {
    "Razor": { "Override": false, "Values": [] },
    "HTML": { "Override": false, "Values": [] },
    "JavaScript": { "Override": false, "Values": [] }
  },
  "UseCli": false
}
```

### Program.cs - Inyección de Dependencias

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Southbound.Client;
using Southbound.Client.Services;
using Southbound.Client.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HTTP Client
builder.Services.AddScoped(sp =>
{
    var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:49677";
    return new HttpClient { BaseAddress = new Uri(apiBase) };
});

// ── State (Servicios singleton por circuito)
builder.Services.AddScoped<ThemeState>();
builder.Services.AddScoped<LocalizationState>();
builder.Services.AddScoped<UserSessionState>();

// ── HTTP Services
builder.Services.AddScoped<AccessHttpService>();
builder.Services.AddScoped<DashboardHttpService>();
builder.Services.AddScoped<EntrySearchHttpService>();
builder.Services.AddScoped<CaptureHttpService>();
builder.Services.AddScoped<BillingHttpService>();
builder.Services.AddScoped<DiscrepancyHttpService>();

await builder.Build().RunAsync();
```

---

## Sistema de Temas

### Cómo Funciona

El sistema de temas implementa un patrón **Light/Dark mode** con dos componentes principales:

1. **ThemeState.cs** - Gestión de estado en C#
2. **window.sbTheme** - API JavaScript para persistencia

### ThemeState.cs

```csharp
namespace Southbound.Client.State;

public class ThemeState
{
    public const string LightTheme = "winter";
    public const string DarkTheme = "dim";

    private string _theme = LightTheme;

    public string Theme
    {
        get => _theme;
        private set
        {
            if (_theme == value) return;
            _theme = value;
            OnChange?.Invoke();
        }
    }

    public bool IsDark => _theme == DarkTheme;

    public event Action? OnChange;

    public void Set(string theme) => Theme = theme;

    public void Toggle() => Theme = IsDark ? LightTheme : DarkTheme;
}
```

**Temas DaisyUI disponibles:**
- `winter` - Tema claro predeterminado
- `dim` - Tema oscuro con preferencia por dark mode

### JavaScript - window.sbTheme

Ubicado en `wwwroot/js/download.js`:

```javascript
window.sbTheme = {
    get: function () {
        try { return localStorage.getItem("sb_theme") || "winter"; }
        catch (e) { return "winter"; }
    },
    set: function (theme) {
        try { localStorage.setItem("sb_theme", theme); } catch (e) { }
        document.documentElement.setAttribute("data-theme", theme);
    }
};
```

**Proceso de aplicación:**
1. El tema se guarda en `localStorage` bajo la clave `sb_theme`
2. Se aplica al elemento raíz `<html>` mediante `data-theme`
3. DaisyUI detecta el atributo y aplica los colores correspondientes

### Aplicación Inmediata del Tema (Splash Screen)

En `wwwroot/index.html`, la página aplica el tema **antes de cargar Blazor** para evitar "flash" visual:

```html
<script>
    (function () {
        var theme = 'winter';
        try { theme = localStorage.getItem('sb_theme') || 'winter'; } catch (e) { }
        document.documentElement.setAttribute('data-theme', theme);
    })();
</script>
```

### Uso en Componentes

**NavMenu.razor** - Toggle de tema:

```razor
@inject ThemeState Theme
@inject IJSRuntime JS

<!-- Theme toggle button -->
<button @onclick="ToggleTheme" aria-label="Toggle theme">
    @if (isDark)
    {
        <!-- Sun icon (en modo oscuro, mostrar opción claro) -->
        <svg>...</svg>
    }
    else
    {
        <!-- Moon icon (en modo claro, mostrar opción oscuro) -->
        <svg>...</svg>
    }
</button>

@code {
    private bool isDark => Theme.IsDark;

    private async Task ToggleTheme()
    {
        Theme.Toggle();
        try
        {
            await JS.InvokeVoidAsync("sbTheme.set", Theme.Theme);
        }
        catch { }
    }
}
```

**Home.razor** - Reaccionar a cambios de tema:

```razor
@implements IDisposable
@inject ThemeState ThemeState

<ApexChart @key="@($"chart-{ThemeState.Theme}")">
    <!-- El componente se re-renderiza cuando cambia el tema -->
</ApexChart>

@code {
    protected override void OnInitialized()
    {
        ThemeState.OnChange += HandleThemeChanged;
    }

    private async void HandleThemeChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose() => ThemeState.OnChange -= HandleThemeChanged;
}
```

---

## Componentes DaisyUI Utilizados

### Componentes Principales

#### 1. **Drawer** (Sidebar)
Ubicado en: `MainLayout.razor`

```razor
<div class="drawer lg:drawer-open">
    <input id="sidebar-drawer" type="checkbox" class="drawer-toggle" />
    
    <div class="drawer-content flex flex-col h-screen bg-base-200">
        <!-- Contenido principal -->
    </div>
    
    <div class="drawer-side z-40">
        <label for="sidebar-drawer" aria-label="close sidebar" class="drawer-overlay"></label>
        <NavMenu />
    </div>
</div>
```

**Características:**
- Colapsable en móvil (`lg:drawer-open`)
- Toggle controlado por checkbox
- Overlay para cerrar en móvil

#### 2. **Navbar**
Ubicado en: `MainLayout.razor` (headerbar móvil)

```razor
<div class="navbar bg-white border-b border-base-300 lg:hidden shadow-sm">
    <div class="flex-none">
        <label for="sidebar-drawer" class="btn btn-square btn-ghost">
            <!-- Menu icon -->
        </label>
    </div>
    <div class="flex-1">
        <span class="text-lg font-semibold">Southbound</span>
    </div>
</div>
```

#### 3. **Menu / Sidebar**
Ubicado en: `NavMenu.razor`

```razor
<aside class="menu bg-base-100 border-base-300 text-base-content min-h-full border-r p-3">
    <ul class="space-y-1">
        <li class="tooltip tooltip-right" data-tip="Label">
            <NavLink class="flex items-center rounded-lg text-sm font-medium"
                     href="/" 
                     Match="NavLinkMatch.All"
                     ActiveClass="!bg-primary/10 !text-primary">
                <svg><!-- icon --></svg>
                <span>Dashboard</span>
            </NavLink>
        </li>
    </ul>
</aside>
```

**Características:**
- Colapsable (mini-rail en modo contraído)
- Tooltips en iconos
- Indicador de ruta activa
- Animación suave de ancho

#### 4. **Badge**
Se utiliza en múltiples lugares para etiquetar estados

```razor
<!-- Badge ghost (neutral) -->
<span class="badge badge-ghost badge-sm">Last loaded: 14:30</span>

<!-- Badge con color semántico -->
<span class="badge badge-soft badge-success">On Time</span>
<span class="badge badge-soft badge-error">Overdue</span>
<span class="badge badge-soft badge-warning">Tripwire</span>
```

#### 5. **Alert / Alert Soft**
Ubicado en: `ModuleLoginModal.razor`, `Home.razor`

```razor
<!-- Error alert -->
<div role="alert" class="alert alert-error">
    <svg><!-- icon --></svg>
    <span>Error message</span>
</div>

<!-- Soft variant (fondo translúcido) -->
<div role="alert" class="alert alert-soft alert-error">
    <span>Error message</span>
</div>
```

#### 6. **Card**
Ubicado en: Múltiples componentes

```razor
<!-- Card con borde -->
<div class="card card-border bg-base-100">
    <div class="card-body gap-3 p-4">
        <h2 class="card-title">Title</h2>
        <p>Content</p>
    </div>
</div>

<!-- Card compacto (stats) -->
<div class="bg-success text-success-content rounded-box p-5 shadow-sm">
    <div class="text-4xl font-bold">42</div>
    <div class="text-xs opacity-80">Label</div>
</div>
```

#### 7. **Modal**
Ubicado en: `ModuleLoginModal.razor`

```razor
<!-- Overlay oscuro -->
<div class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
    <!-- Card modal -->
    <div class="card bg-base-100 w-full max-w-md shadow-xl">
        <div class="card-body">
            <!-- Contenido -->
            <div class="card-actions mt-4 justify-end">
                <button class="btn btn-ghost btn-sm">Cancel</button>
                <button class="btn btn-primary btn-sm">Submit</button>
            </div>
        </div>
    </div>
</div>
```

#### 8. **Input**
Ubicado en: Formularios

```razor
<input type="text" class="input input-bordered w-full" placeholder="Enter text" />
<input type="password" class="input input-bordered w-full" />

<!-- Input con joined button -->
<div class="join w-full">
    <input type="text" class="input join-item flex-1" />
    <button class="btn btn-primary join-item">Search</button>
</div>
```

#### 9. **Button**
Ubicado en: Múltiples lugares

```razor
<!-- Botón primario -->
<button class="btn btn-primary">Primary</button>

<!-- Botón ghost (transparente) -->
<button class="btn btn-ghost">Ghost</button>

<!-- Botones con size -->
<button class="btn btn-sm">Small</button>
<button class="btn btn-lg">Large</button>

<!-- Botón cuadrado (solo icono) -->
<button class="btn btn-square btn-ghost">
    <svg><!-- icon --></svg>
</button>

<!-- Con loading spinner -->
<button class="btn btn-primary" disabled>
    <span class="loading loading-spinner loading-sm"></span>
</button>
```

#### 10. **Tabs**
Ubicado en: `Home.razor`, `EntrySearch.razor`

```razor
<!-- Tabs con borde inferior -->
<div role="tablist" class="tabs tabs-border">
    <a role="tab" class="tab @(active ? "tab-active" : "")" @onclick="OnClick">
        Summary
    </a>
    <a role="tab" class="tab @(active ? "tab-active" : "")" @onclick="OnClick">
        Inventory
    </a>
</div>

<!-- Tabs boxed -->
<div role="tablist" class="tabs tabs-box">
    <button role="tab" class="tab">By Entry</button>
    <button role="tab" class="tab tab-active">By Tracking</button>
</div>
```

#### 11. **Table**
Ubicado en: `Home.razor`

```razor
<div class="overflow-x-auto rounded-lg border border-base-300">
    <table class="table table-xs table-zebra table-pin-rows">
        <thead>
            <tr>
                <th>Column 1</th>
                <th>Column 2</th>
            </tr>
        </thead>
        <tbody>
            <tr class="hover">
                <td>Data 1</td>
                <td>Data 2</td>
            </tr>
        </tbody>
    </table>
</div>
```

**Variantes utilizadas:**
- `table-xs` - Compacta
- `table-zebra` - Filas alternadas
- `table-pin-rows` - Header fijo
- `hover` - Efecto hover

#### 12. **Join** (Group de botones/inputs)
Ubicado en: `EntrySearch.razor`, `Home.razor`

```razor
<div class="join w-full">
    <input type="text" class="input join-item flex-1" />
    <button class="btn btn-primary join-item">Button</button>
</div>

<!-- Paginación con join -->
<div class="join">
    <button class="join-item btn btn-sm">«</button>
    <button class="join-item btn btn-sm">1</button>
    <button class="join-item btn btn-sm btn-active">2</button>
    <button class="join-item btn btn-sm">3</button>
    <button class="join-item btn btn-sm">»</button>
</div>
```

#### 13. **Skeleton** (Loading placeholder)
Ubicado en: `Home.razor`

```razor
<!-- Skeleton for cards during loading -->
<div class="skeleton h-4 w-24"></div>
<div class="skeleton h-8 w-16"></div>

<!-- Skeleton for images -->
<div class="skeleton h-48 w-48 rounded-full"></div>
```

#### 14. **Loading Spinner**
Ubicado en: Botones, tabs

```razor
<span class="loading loading-spinner loading-sm"></span>
<span class="loading loading-spinner loading-md"></span>
<span class="loading loading-spinner loading-lg"></span>
```

#### 15. **Tooltip**
Ubicado en: `NavMenu.razor`

```razor
<div class="tooltip tooltip-right" data-tip="Tooltip text">
    <button>Hover me</button>
</div>

<div class="tooltip tooltip-bottom" data-tip="Bottom tooltip">
    <button>Hover me</button>
</div>
```

### Tokens de Diseño DaisyUI (Colors)

**Utilizados en el proyecto:**
- `base-100`, `base-200`, `base-300` - Colores base
- `base-content` - Texto en fondo base
- `primary`, `primary-content` - Color primario
- `success`, `success-content` - Verde para éxito
- `error`, `error-content` - Rojo para errores
- `warning`, `warning-content` - Amarillo para advertencias
- `neutral`, `neutral-content` - Neutro

**Opacidades:**
- `opacity-0` a `opacity-100`
- `text-base-content/50` - 50% opacity
- `text-base-content/40` - 40% opacity
- `bg-primary/10` - 10% opacity de fondo

---

## Ejemplos de Componentes Personalizados

### 1. EmptyState.razor

Componente reutilizable para estados vacíos:

```razor
@* EmptyState - consistent empty/no-data placeholder used across views *@

<div class="flex flex-col items-center justify-center gap-3 py-12 text-center">
    <div class="bg-base-200 flex h-14 w-14 items-center justify-center rounded-full">
        <svg xmlns="http://www.w3.org/2000/svg" class="text-base-content/30 h-7 w-7" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="@IconPath" />
        </svg>
    </div>
    <div>
        <p class="text-base-content/70 text-sm font-medium">@Title</p>
        @if (!string.IsNullOrEmpty(Description))
        {
            <p class="text-base-content/40 mt-1 text-xs">@Description</p>
        }
    </div>
    @ChildContent
</div>

@code {
    [Parameter] public string Title { get; set; } = "No data";
    [Parameter] public string? Description { get; set; }
    [Parameter] public string IconPath { get; set; } = "M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4";
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

**Uso:**
```razor
<EmptyState 
    Title="No inventory data available" 
    Description="There are no items to display right now."
    IconPath="..." />
```

### 2. ModuleGuard.razor

Gate de autenticación para módulos protegidos:

```razor
@inject UserSessionState Session
@inject LocalizationState L

@if (authenticated)
{
    <div class="flex h-full flex-col">
        <!-- Header con usuario y sign out -->
        <div class="border-base-300 bg-base-100 mb-3 flex items-center justify-end gap-3 rounded-lg border px-4 py-2">
            <div class="flex items-center gap-2 text-sm">
                <svg><!-- user icon --></svg>
                <span class="text-base-content font-medium">@CurrentSession?.EmployeeName</span>
                <span class="text-base-content/40">(@CurrentSession?.EmployeeNumber)</span>
            </div>
            <button class="btn btn-ghost btn-xs gap-1.5" @onclick="SignOut">
                <svg><!-- logout icon --></svg>
                @L["SignOut"]
            </button>
        </div>
        
        <!-- Contenido protegido -->
        <div class="min-h-0 flex-1">
            @ChildContent
        </div>
    </div>
}
else if (checkedStorage)
{
    <!-- Modal de login -->
    <ModuleLoginModal Module="Module" ModuleTitle="@ModuleTitle" />
}

@code {
    [Parameter, EditorRequired] public ProtectedModule Module { get; set; }
    [Parameter] public string ModuleTitle { get; set; } = string.Empty;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool checkedStorage;
    private bool authenticated => Session.IsAuthenticated(Module);
    private ModuleSession? CurrentSession => Session.GetSession(Module);

    // ... lógica de ciclo de vida y manejo de sesiones
}
```

### 3. ModuleLoginModal.razor

Modal de login del módulo:

```razor
<div class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
    <div class="card bg-base-100 w-full max-w-md shadow-xl">
        <div class="card-body">
            <!-- Header -->
            <div class="flex items-center gap-3">
                <div class="bg-primary/10 text-primary flex h-10 w-10 items-center justify-center rounded-lg">
                    <svg><!-- lock icon --></svg>
                </div>
                <div>
                    <h2 class="text-base-content text-lg font-semibold">@L["AccessRequired"]</h2>
                    <p class="text-base-content/50 text-xs">@ModuleTitle</p>
                </div>
            </div>

            <p class="text-base-content/60 mt-2 text-sm">@L["AccessRequiredDesc"]</p>

            <!-- Error alert -->
            @if (!string.IsNullOrEmpty(error))
            {
                <div role="alert" class="alert alert-error alert-soft mt-2">
                    <svg><!-- icon --></svg>
                    <span>@error</span>
                </div>
            }

            <!-- Form inputs -->
            <div class="mt-2 space-y-3">
                <div>
                    <label class="text-base-content/70 mb-1 block text-xs font-medium">@L["EmployeeNumber"]</label>
                    <input type="text" class="input input-bordered w-full" @bind="employeeNumber" />
                </div>
                <div>
                    <label class="text-base-content/70 mb-1 block text-xs font-medium">@L["Password"]</label>
                    <div class="relative">
                        <input type="@(isPasswordVisible ? "text" : "password")" class="input input-bordered w-full pr-10" @bind="password" />
                        <button type="button" class="absolute right-2 top-1/2 -translate-y-1/2 text-base-content/50 hover:text-base-content transition-colors" @onclick="TogglePasswordVisibility">
                            <svg><!-- eye/eye-off icon --></svg>
                        </button>
                    </div>
                </div>
            </div>

            <!-- Action buttons -->
            <div class="card-actions mt-4 justify-end">
                <a href="" class="btn btn-ghost btn-sm">@L["Cancel"]</a>
                <button class="btn btn-primary btn-sm" @onclick="Validate" disabled="@isValidating">
                    @if (isValidating)
                    {
                        <span class="loading loading-spinner loading-sm"></span>
                    }
                    else
                    {
                        <span>@L["Enter"]</span>
                    }
                </button>
            </div>
        </div>
    </div>
</div>
```

---

## Patrones y Mejores Prácticas

### 1. Grid Layout Responsivo

Patrón usado en todas las páginas:

```razor
<!-- Layout con grilla de 2 columnas, responsive -->
<div class="grid grid-cols-2 gap-4 lg:grid-cols-4">
    @foreach (var item in items)
    {
        <div class="card"><!-- content --></div>
    }
</div>

<!-- Layout grid con filas fijas -->
<div class="grid h-full gap-4 overflow-hidden" style="grid-template-rows: auto 1fr;">
    <div><!-- header, fixed height --></div>
    <div class="min-h-0 overflow-y-auto"><!-- scrollable content --></div>
</div>
```

### 2. Utilidad fade-in

Custom utility definida en `app.css`:

```razor
<!-- Elemento con entrada suave -->
<div class="fade-in">
    Content que entra suavemente
</div>
```

### 3. Responsive Text

```razor
<!-- Ocultar texto en móvil, mostrar en sm y superior -->
<span class="hidden sm:inline">Full Text</span>

<!-- Tamaño responsive -->
<h1 class="text-2xl">Title</h1>
```

### 4. State Management Pattern

```csharp
// Componente que observa cambios de estado
@implements IDisposable
@inject ThemeState Theme

@code {
    protected override void OnInitialized()
    {
        Theme.OnChange += HandleStateChanged;
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Theme.OnChange -= HandleStateChanged;
    }
}
```

### 5. Loading States

```razor
@if (isLoading)
{
    <!-- Skeleton placeholders -->
    <div class="space-y-3">
        <div class="skeleton h-4 w-full"></div>
        <div class="skeleton h-4 w-full"></div>
    </div>
}
else if (hasLoaded)
{
    <!-- Content -->
}
```

### 6. Error Handling

```razor
@if (!string.IsNullOrEmpty(errorMessage))
{
    <div role="alert" class="alert alert-error">
        <svg><!-- icon --></svg>
        <span>@errorMessage</span>
    </div>
}
```

### 7. Internationalization (i18n)

```razor
@inject LocalizationState L

<!-- Uso simple -->
<h1>@L["Dashboard"]</h1>

<!-- Con toggle de idioma -->
private async Task ToggleLanguage()
{
    L.Toggle();  // Cambia entre es/en
    try
    {
        await JS.InvokeVoidAsync("sbLang.set", L.Language);
    }
    catch { }
}
```

### 8. Persistencia con LocalStorage

```csharp
// Guardar en LocalStorage
await JS.InvokeVoidAsync("sbSession.set", key, json);

// Leer de LocalStorage
var value = await JS.InvokeAsync<string?>("sbSession.get", key);

// Eliminar de LocalStorage
await JS.InvokeVoidAsync("sbSession.remove", key);
```

---

## Scripts JavaScript

### wwwroot/js/download.js

Contiene tres APIs principales:

#### 1. window.downloadFileFromBase64

Descarga un archivo desde un string base64:

```javascript
window.downloadFileFromBase64 = function (base64, fileName, contentType) {
    const link = document.createElement("a");
    link.href = "data:" + contentType + ";base64," + base64;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
```

**Uso desde Blazor:**
```csharp
var bytes = await Service.ExportAsync();
var base64 = Convert.ToBase64String(bytes);
await JS.InvokeVoidAsync("downloadFileFromBase64", 
    base64, 
    "Export_20240101.xlsx",
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
```

#### 2. window.sbTheme

API para gestionar el tema (light/dark):

```javascript
window.sbTheme = {
    get: function () {
        try { return localStorage.getItem("sb_theme") || "winter"; }
        catch (e) { return "winter"; }
    },
    set: function (theme) {
        try { localStorage.setItem("sb_theme", theme); } catch (e) { }
        document.documentElement.setAttribute("data-theme", theme);
    }
};
```

#### 3. window.sbLang

API para gestionar el idioma:

```javascript
window.sbLang = {
    get: function () {
        try { return localStorage.getItem("sb_lang") || "es"; }
        catch (e) { return "es"; }
    },
    set: function (lang) {
        try { localStorage.setItem("sb_lang", lang); } catch (e) { }
    }
};
```

#### 4. window.sbSession

API para gestionar sesiones de usuario:

```javascript
window.sbSession = {
    get: function (key) {
        try { return localStorage.getItem(key); }
        catch (e) { return null; }
    },
    set: function (key, json) {
        try { localStorage.setItem(key, json); } catch (e) { }
    },
    remove: function (key) {
        try { localStorage.removeItem(key); } catch (e) { }
    }
};
```

### Splash Screen (wwwroot/index.html)

Animación de carga personalizada que se renderiza antes de que Blazor cargue:

```html
<style>
    .sb-splash {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        min-height: 100vh;
        background-color: var(--color-base-100, #ffffff);
        gap: 28px;
    }

    .sb-splash__logo {
        display: flex;
        align-items: baseline;
        gap: 2px;
    }

    .sb-splash__letter {
        font-family: 'Inter', system-ui, sans-serif;
        font-size: 52px;
        font-weight: 800;
        color: var(--color-primary, #3b82f6);
        opacity: 0;
        animation: sb-assemble 0.8s cubic-bezier(0.22, 1, 0.36, 1) forwards;
    }

    .sb-splash__letter:nth-child(1) { --x: -120px; --y: -80px; --r: -25deg; animation-delay: 0.1s; }
    /* ... etc */

    @keyframes sb-assemble {
        0% {
            opacity: 0;
            transform: translate(var(--x), var(--y)) rotate(var(--r)) scale(0.5);
        }
        100% {
            opacity: 1;
            transform: translate(0, 0) rotate(0deg) scale(1);
        }
    }
</style>
```

---

## Colores y Variables de Diseño

### Tokens DaisyUI en Uso

| Token | Valor (Light/Dark) | Uso |
|-------|-------------------|-----|
| `base-100` | #fff / #1f2937 | Fondo de cards, componentes |
| `base-200` | #f9fafb / #111827 | Fondo secundario |
| `base-300` | #eeeeee / #0f172a | Bordes, separadores |
| `base-content` | #000 / #fff | Texto principal |
| `primary` | #3b82f6 / #60a5fa | Color primario (botones, acentos) |
| `success` | #16a34a | Verde (éxito) |
| `error` | #dc2626 | Rojo (errores) |
| `warning` | #eab308 | Amarillo (advertencias) |
| `neutral` | #64748b | Gris neutro |

### Opacidades Comunes

- `bg-primary/10` - Fondo muy ligero
- `bg-primary/50` - Fondo semitransparente
- `text-base-content/50` - Texto 50% opaco
- `text-base-content/40` - Texto muy atenuado
- `text-base-content/30` - Texto muy atenuado

---

## Resumen de Buenas Prácticas

1. **Siempre usar clases de Tailwind** en lugar de CSS personalizado
2. **Usar componentes DaisyUI** para consistencia visual
3. **Implementar estado reactivo** mediante servicios Scoped en Program.cs
4. **Persistir en localStorage** usando las APIs JavaScript wrappadas
5. **Aplicar temas inmediatamente** en index.html para evitar flash
6. **Usar tooltips** para acciones colapsadas (sidebar)
7. **Implementar loading states** con skeletons
8. **Manejar errores gracefully** con alerts
9. **Usar grid responsive** para layouts
10. **Localizar todo el contenido** mediante LocalizationState

---

## Recursos Útiles

- **DaisyUI Docs**: https://daisyui.com/components/
- **Tailwind CSS Docs**: https://tailwindcss.com/docs
- **Blazor Docs**: https://learn.microsoft.com/en-us/aspnet/core/blazor/
- **LocalStorage MDN**: https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage

---

**Documento preparado para servir como referencia de rediseño de ComplianceControlCenter**
