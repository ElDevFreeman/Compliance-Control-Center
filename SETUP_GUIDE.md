# Guía de Setup - Implementar DaisyUI en OEA.Checklist

Pasos para configurar Tailwind CSS 4 + DaisyUI 5 en el proyecto OEA.Checklist basado en Southbound.Client.

---

## Paso 1: Instalar Dependencias NPM

### 1.1 Crear o actualizar package.json

En la raíz del proyecto `OEA.Checklist`:

```json
{
  "name": "oea-checklist",
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

### 1.2 Instalar dependencias

```bash
# Con pnpm (recomendado)
pnpm install

# O con npm
npm install
```

---

## Paso 2: Crear Estructura de Carpetas

Si no existen, crear:

```
OEA.Checklist/
├── Styles/
│   └── app.css                 # Nuevo archivo
├── wwwroot/
│   └── css/
│       └── site.css            # Se genera aquí (git ignore)
└── package.json                # Ya existe o se crea
```

---

## Paso 3: Crear app.css

Ubicación: `OEA.Checklist/Styles/app.css`

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

/* Custom utilities */
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

---

## Paso 4: Crear tailwind.extension.json

Ubicación: `OEA.Checklist/tailwind.extension.json`

Para que Visual Studio detecte Tailwind:

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

---

## Paso 5: Actualizar wwwroot/index.html

Asegurarse de que el CSS compilado se carga:

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>OEA Checklist</title>
    <base href="/" />
    <link rel="stylesheet" href="css/site.css" />
    <style>
        /* Aplicar tema antes de que Blazor cargue */
        (function () {
            var theme = 'winter';
            try { theme = localStorage.getItem('sb_theme') || 'winter'; } catch (e) { }
            document.documentElement.setAttribute('data-theme', theme);
        })();
    </style>
</head>
<body>
    <div id="app">
        <!-- Loading splash here if desired -->
    </div>
    <script src="_framework/blazor.webassembly.js"></script>
</body>
</html>
```

---

## Paso 6: Crear Scripts JavaScript

Ubicación: `OEA.Checklist/wwwroot/js/theme.js` (nuevo archivo)

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

window.sbLang = {
    get: function () {
        try { return localStorage.getItem("sb_lang") || "es"; }
        catch (e) { return "es"; }
    },
    set: function (lang) {
        try { localStorage.setItem("sb_lang", lang); } catch (e) { }
    }
};

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

Incluir en `index.html`:

```html
<script src="js/theme.js"></script>
<script src="_framework/blazor.webassembly.js"></script>
```

---

## Paso 7: Crear Servicios de Estado

### 7.1 ThemeState.cs

Ubicación: `OEA.Checklist/State/ThemeState.cs`

```csharp
namespace OeaChecklist.State;

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

### 7.2 LocalizationState.cs

Ubicación: `OEA.Checklist/State/LocalizationState.cs`

```csharp
namespace OeaChecklist.State;

public class LocalizationState
{
    public const string English = "en";
    public const string Spanish = "es";

    private string _lang = Spanish;

    public string Language
    {
        get => _lang;
        private set
        {
            if (_lang == value) return;
            _lang = value;
            OnChange?.Invoke();
        }
    }

    public bool IsSpanish => _lang == Spanish;

    public event Action? OnChange;

    public void Set(string lang) => Language = lang is Spanish or English ? lang : Spanish;

    public void Toggle() => Language = IsSpanish ? English : Spanish;

    public string this[string key]
    {
        get
        {
            if (Translations.TryGetValue(key, out var entry))
            {
                return _lang == Spanish ? entry.Es : entry.En;
            }
            return key;
        }
    }

    private record Entry(string En, string Es);

    private static readonly Dictionary<string, Entry> Translations = new()
    {
        // Agregar traducciones aquí
        ["Dashboard"] = new("Dashboard", "Tablero"),
        // ... más traducciones
    };
}
```

---

## Paso 8: Configurar Program.cs

Agregar servicios de estado:

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OeaChecklist;
using OeaChecklist.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5000";
    return new HttpClient { BaseAddress = new Uri(apiBase) };
});

// ── State Services ──
builder.Services.AddScoped<ThemeState>();
builder.Services.AddScoped<LocalizationState>();

// ── HTTP Services ──
// Agregar aquí

await builder.Build().RunAsync();
```

---

## Paso 9: Crear MainLayout.razor

Ubicación: `OEA.Checklist/Layout/MainLayout.razor`

```razor
@inherits LayoutComponentBase
@inject IJSRuntime JS

<div class="drawer lg:drawer-open">
    <input id="sidebar-drawer" type="checkbox" class="drawer-toggle" />

    <div class="drawer-content flex flex-col h-screen bg-base-200">
        <div class="navbar bg-white border-b border-base-300 lg:hidden shadow-sm">
            <div class="flex-none">
                <label for="sidebar-drawer" class="btn btn-square btn-ghost">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-5 h-5 stroke-current">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"></path>
                    </svg>
                </label>
            </div>
            <div class="flex-1">
                <span class="text-lg font-semibold">App Name</span>
            </div>
        </div>

        <main class="flex-1 min-h-0 p-4 lg:p-6 overflow-hidden">
            @Body
        </main>
    </div>

    <div class="drawer-side z-40">
        <label for="sidebar-drawer" aria-label="close sidebar" class="drawer-overlay"></label>
        <NavMenu />
    </div>
</div>

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Inicialización si es necesario
        await Task.CompletedTask;
    }
}
```

---

## Paso 10: Crear NavMenu.razor

Ubicación: `OEA.Checklist/Layout/NavMenu.razor`

```razor
@inject ThemeState Theme
@inject LocalizationState L
@inject IJSRuntime JS

<aside class="menu bg-base-100 border-base-300 text-base-content min-h-full border-r p-3 w-64">
    <!-- Logo -->
    <div class="flex items-center mb-8 gap-3 px-2">
        <div class="w-9 h-9 rounded-lg bg-primary flex items-center justify-center">
            <span class="text-primary-content font-bold text-sm">EC</span>
        </div>
        <div>
            <h2 class="font-semibold text-base-content text-sm">OEA</h2>
            <p class="text-xs text-base-content/50">Checklist</p>
        </div>
    </div>

    <!-- Navigation -->
    <ul class="space-y-1">
        <li>
            <NavLink href="/" Match="NavLinkMatch.All" 
                     class="flex items-center rounded-lg text-sm font-medium transition-colors text-base-content/60 hover:bg-base-200 hover:text-base-content gap-3 px-3 py-2.5" 
                     ActiveClass="!bg-primary/10 !text-primary">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-3m0 0l7-4 7 4M5 9v10a1 1 0 001 1h12a1 1 0 001-1V9" />
                </svg>
                <span>Dashboard</span>
            </NavLink>
        </li>
    </ul>

    <!-- Footer -->
    <div class="mt-auto pt-6 border-t border-base-300">
        <button class="flex items-center rounded-lg text-sm font-medium transition-colors text-base-content/60 hover:bg-base-200 hover:text-base-content w-full gap-3 px-3 py-2.5" @onclick="ToggleTheme">
            @if (isDark)
            {
                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
                <span>Light</span>
            }
            else
            {
                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                </svg>
                <span>Dark</span>
            }
        </button>
    </div>
</aside>

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

---

## Paso 11: Actualizar _Imports.razor

Agregar imports globales:

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.JSInterop
@using OeaChecklist
@using OeaChecklist.Layout
@using OeaChecklist.Pages
@using OeaChecklist.Components
@using OeaChecklist.State
```

---

## Paso 12: Compilar Estilos

### 12.1 En Desarrollo

```bash
pnpm run css:watch
```

Esto genera `wwwroot/css/site.css` automáticamente mientras editas archivos Razor.

### 12.2 En Producción

```bash
pnpm run css:build
```

Genera CSS minificado.

---

## Paso 13: Ejecutar el Proyecto

```bash
# En una terminal: compilar estilos
pnpm run css:watch

# En otra terminal: ejecutar Blazor
dotnet run
```

O configurar en Visual Studio:
- Tools → Options → Projects and Solutions → Web Development → Tailwind CSS
- Establecer rutas correctas

---

## Verificación

Para verificar que todo funciona:

1. Abrir la página principal
2. Debería verse con estilos DaisyUI
3. Clicar en el botón de tema (si existe)
4. Debería cambiar a tema oscuro
5. Recargar la página
6. El tema debe persistir

---

## Troubleshooting

### CSS no se compila

```bash
# Reinstalar dependencias
pnpm install

# Ejecutar compilación manualmente
npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/site.css
```

### IntelliSense en VS

- Instalar: **Tailwind CSS IntelliSense** extension
- Instalar: **Tailwind CSS for Visual Studio** (para C#)

### site.css no se genera

- Verificar que la ruta en `app.css` sea correcta
- Verificar que @source señale las carpetas correctas
- Verificar que los archivos .razor existan en esas carpetas

### Tema no persiste

- Verificar que `theme.js` esté incluido en `index.html`
- Verificar que localStorage no esté deshabilitado
- Verificar en DevTools (F12 → Application → LocalStorage)

---

## Estructura Final de Carpetas

```
OEA.Checklist/
├── App.razor
├── Program.cs
├── _Imports.razor
├── package.json
├── tailwind.extension.json
│
├── Pages/
│   ├── Index.razor
│   └── ...
│
├── Layout/
│   ├── MainLayout.razor
│   └── NavMenu.razor
│
├── Components/
│   ├── EmptyState.razor
│   └── ...
│
├── State/
│   ├── ThemeState.cs
│   ├── LocalizationState.cs
│   └── ...
│
├── Styles/
│   └── app.css
│
├── wwwroot/
│   ├── index.html
│   ├── css/
│   │   └── site.css (generado)
│   └── js/
│       └── theme.js
│
└── Properties/
    └── launchSettings.json
```

---

## Próximos Pasos

1. Implementar componentes usando ejemplos de DAISYUI_COMPONENTS_EXAMPLES.md
2. Crear servicios HTTP según sea necesario
3. Agregar páginas y componentes
4. Personalizar temas en app.css si es necesario
5. Desplegar a producción

---

**Setup completado. Ver SOUTHBOUND_REFERENCE_GUIDE.md para más detalles sobre patrones y componentes.**
