# Componentes DaisyUI - Ejemplos Prácticos

Referencia rápida de componentes DaisyUI con ejemplos de código listos para copiar/pegar en OEA.Checklist.

---

## 1. Sidebar + Navbar (MainLayout)

### Estructura Completa

```razor
@inherits LayoutComponentBase
@inject IJSRuntime JS

<div class="drawer lg:drawer-open">
    <input id="sidebar-drawer" type="checkbox" class="drawer-toggle" />

    <!-- Contenido principal -->
    <div class="drawer-content flex flex-col h-screen bg-base-200">
        
        <!-- Navbar móvil -->
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

        <!-- Contenido principal -->
        <main class="flex-1 min-h-0 p-4 lg:p-6 overflow-hidden">
            @Body
        </main>
    </div>

    <!-- Sidebar -->
    <div class="drawer-side z-40">
        <label for="sidebar-drawer" aria-label="close sidebar" class="drawer-overlay"></label>
        
        <aside class="menu bg-base-100 border-base-300 text-base-content min-h-full border-r p-3 transition-[width] duration-200 ease-out w-64">
            <!-- Logo -->
            <div class="flex items-center mb-8 gap-3 px-2">
                <div class="w-9 h-9 rounded-lg bg-primary flex items-center justify-center">
                    <span class="text-primary-content font-bold text-sm">SB</span>
                </div>
                <div>
                    <h2 class="font-semibold text-base-content text-sm">App Name</h2>
                    <p class="text-xs text-base-content/50">Subtitle</p>
                </div>
            </div>

            <!-- Navegación -->
            <ul class="space-y-1">
                <li>
                    <NavLink href="/" Match="NavLinkMatch.All" class="flex items-center rounded-lg text-sm font-medium transition-colors text-base-content/60 hover:bg-base-200 hover:text-base-content gap-3 px-3 py-2.5" ActiveClass="!bg-primary/10 !text-primary">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-3m0 0l7-4 7 4M5 9v10a1 1 0 001 1h12a1 1 0 001-1V9" />
                        </svg>
                        <span>Dashboard</span>
                    </NavLink>
                </li>
                <li>
                    <NavLink href="/page1" class="flex items-center rounded-lg text-sm font-medium transition-colors text-base-content/60 hover:bg-base-200 hover:text-base-content gap-3 px-3 py-2.5" ActiveClass="!bg-primary/10 !text-primary">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <span>Page One</span>
                    </NavLink>
                </li>
            </ul>

            <!-- Footer (espacio) -->
            <div class="mt-auto pt-6 border-t border-base-300">
                <div class="space-y-2">
                    <button class="flex items-center rounded-lg text-sm font-medium transition-colors text-base-content/60 hover:bg-base-200 hover:text-base-content w-full gap-3 px-3 py-2.5">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1m-16 0H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
                        </svg>
                        <span>Theme</span>
                    </button>
                    <button class="flex items-center rounded-lg text-sm font-medium transition-colors text-base-content/60 hover:bg-base-200 hover:text-base-content w-full gap-3 px-3 py-2.5">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5h12M9 3v2m1 13.5A10.022 10.022 0 003 12m18 0a10 10 0 01-15-9" />
                        </svg>
                        <span>Language</span>
                    </button>
                </div>
            </div>
        </aside>
    </div>
</div>
```

---

## 2. Cards - Variantes

### Card Básico

```razor
<div class="card bg-base-100 shadow-md">
    <div class="card-body">
        <h2 class="card-title">Title</h2>
        <p>Lorem ipsum dolor sit amet.</p>
        <div class="card-actions justify-end">
            <button class="btn btn-primary">Action</button>
        </div>
    </div>
</div>
```

### Card con Borde

```razor
<div class="card card-border bg-base-100">
    <div class="card-body p-4">
        <h3 class="text-sm font-semibold text-base-content">Card Title</h3>
        <p class="text-sm text-base-content/60 mt-2">Card content goes here</p>
    </div>
</div>
```

### Card Stats (Coloreado)

```razor
<div class="bg-success text-success-content rounded-box p-5 shadow-sm">
    <div class="flex items-center justify-between">
        <span class="text-sm font-medium opacity-90">On Time</span>
        <svg xmlns="http://www.w3.org/2000/svg" class="h-7 w-7 opacity-80" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
    </div>
    <div class="mt-2 text-4xl font-bold">42</div>
    <div class="mt-1 text-xs opacity-80">Entries within SLA</div>
</div>

<div class="bg-error text-error-content rounded-box p-5 shadow-sm">
    <div class="flex items-center justify-between">
        <span class="text-sm font-medium opacity-90">Overdue</span>
        <svg xmlns="http://www.w3.org/2000/svg" class="h-7 w-7 opacity-80" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
    </div>
    <div class="mt-2 text-4xl font-bold">8</div>
    <div class="mt-1 text-xs opacity-80">Past deadline</div>
</div>

<div class="bg-warning text-warning-content rounded-box p-5 shadow-sm">
    <!-- Similar structure -->
</div>
```

### Card Grid Responsivo

```razor
<div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
    @foreach (var item in items)
    {
        <div class="card card-border bg-base-100">
            <div class="card-body">
                <h3 class="card-title text-sm">@item.Title</h3>
                <p class="text-sm">@item.Description</p>
            </div>
        </div>
    }
</div>
```

---

## 3. Modals

### Modal Simple

```razor
@if (isModalOpen)
{
    <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="card bg-base-100 w-full max-w-md shadow-xl">
            <div class="card-body">
                <h2 class="card-title">Modal Title</h2>
                <p>This is a simple modal dialog.</p>
                
                <div class="card-actions justify-end mt-4">
                    <button class="btn btn-ghost" @onclick="CloseModal">Cancel</button>
                    <button class="btn btn-primary" @onclick="ConfirmAction">Confirm</button>
                </div>
            </div>
        </div>
    </div>
}

@code {
    private bool isModalOpen = false;

    private void CloseModal() => isModalOpen = false;
    
    private async Task ConfirmAction()
    {
        // Do something
        isModalOpen = false;
    }
}
```

### Modal con Formulario

```razor
@if (isFormModalOpen)
{
    <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="card bg-base-100 w-full max-w-md shadow-xl">
            <div class="card-body">
                <h2 class="card-title mb-4">New Entry</h2>
                
                <div class="space-y-3">
                    <div>
                        <label class="label">
                            <span class="label-text text-sm font-medium">Name</span>
                        </label>
                        <input type="text" class="input input-bordered w-full" @bind="formData.Name" />
                    </div>
                    
                    <div>
                        <label class="label">
                            <span class="label-text text-sm font-medium">Email</span>
                        </label>
                        <input type="email" class="input input-bordered w-full" @bind="formData.Email" />
                    </div>

                    <div>
                        <label class="label">
                            <span class="label-text text-sm font-medium">Message</span>
                        </label>
                        <textarea class="textarea textarea-bordered w-full" @bind="formData.Message"></textarea>
                    </div>
                </div>
                
                <div class="card-actions justify-end mt-6">
                    <button class="btn btn-ghost" @onclick="CloseFormModal">Cancel</button>
                    <button class="btn btn-primary" @onclick="SaveForm" disabled="@isSubmitting">
                        @if (isSubmitting)
                        {
                            <span class="loading loading-spinner loading-sm"></span>
                        }
                        else
                        {
                            <span>Save</span>
                        }
                    </button>
                </div>
            </div>
        </div>
    </div>
}
```

---

## 4. Alerts

### Alert Variantes

```razor
<!-- Info -->
<div role="alert" class="alert alert-info">
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="w-6 h-6 mx-2 stroke-current">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
    </svg>
    <span>Information message</span>
</div>

<!-- Success -->
<div role="alert" class="alert alert-success">
    <svg xmlns="http://www.w3.org/2000/svg" class="w-6 h-6 mx-2 stroke-current flex-shrink-0" fill="none" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
    </svg>
    <span>Success! Your operation completed.</span>
</div>

<!-- Error -->
<div role="alert" class="alert alert-error">
    <svg xmlns="http://www.w3.org/2000/svg" class="w-6 h-6 mx-2 stroke-current flex-shrink-0" fill="none" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l-2-2m0 0l-2-2m2 2l2-2m-2 2l-2 2"></path>
    </svg>
    <span>Error! Task failed.</span>
</div>

<!-- Warning -->
<div role="alert" class="alert alert-warning">
    <svg xmlns="http://www.w3.org/2000/svg" class="w-6 h-6 mx-2 stroke-current flex-shrink-0" fill="none" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4v2m0 6v2m0-20v2"></path>
    </svg>
    <span>Warning! Check your input.</span>
</div>
```

### Alert Soft (Variant)

```razor
<!-- Soft error (fondo translúcido) -->
<div role="alert" class="alert alert-error alert-soft">
    <svg xmlns="http://www.w3.org/2000/svg" class="w-6 h-6 stroke-current" fill="none" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4v2"></path>
    </svg>
    <span>Error with soft background</span>
</div>
```

---

## 5. Forms & Inputs

### Input con Label

```razor
<div>
    <label class="label">
        <span class="label-text text-sm font-medium">Employee Number</span>
        <span class="label-text-alt text-xs">Required</span>
    </label>
    <input type="text" 
           class="input input-bordered w-full" 
           placeholder="Enter employee number"
           @bind="employeeNumber" />
</div>
```

### Input Variants

```razor
<!-- Default bordered -->
<input type="text" class="input input-bordered w-full" placeholder="Bordered input" />

<!-- Disabled -->
<input type="text" class="input input-bordered w-full" disabled value="Disabled input" />

<!-- Ghost (sin borde) -->
<input type="text" class="input input-ghost w-full" placeholder="Ghost input" />

<!-- Success state -->
<input type="text" class="input input-bordered input-success w-full" value="Success" />

<!-- Error state -->
<input type="text" class="input input-bordered input-error w-full" value="Error" />
```

### Textarea

```razor
<label class="label">
    <span class="label-text text-sm font-medium">Comments</span>
</label>
<textarea class="textarea textarea-bordered w-full" 
          rows="4" 
          placeholder="Enter your comments..."
          @bind="comments"></textarea>
```

### Select (Dropdown)

```razor
<label class="label">
    <span class="label-text text-sm font-medium">Category</span>
</label>
<select class="select select-bordered w-full" @bind="selectedCategory">
    <option disabled selected>Pick one</option>
    <option>Category A</option>
    <option>Category B</option>
    <option>Category C</option>
</select>
```

### Checkbox

```razor
<label class="label cursor-pointer">
    <span class="label-text">Remember me</span>
    <input type="checkbox" class="checkbox checkbox-primary" @bind="rememberMe" />
</label>
```

### Radio

```razor
<div class="space-y-2">
    <label class="label cursor-pointer">
        <span class="label-text">Option A</span>
        <input type="radio" class="radio radio-primary" @onchange="@((ChangeEventArgs e) => selectedOption = "A")" />
    </label>
    <label class="label cursor-pointer">
        <span class="label-text">Option B</span>
        <input type="radio" class="radio radio-primary" @onchange="@((ChangeEventArgs e) => selectedOption = "B")" />
    </label>
</div>
```

---

## 6. Buttons

### Button Variants

```razor
<!-- Primary -->
<button class="btn btn-primary">Primary</button>

<!-- Secondary -->
<button class="btn btn-secondary">Secondary</button>

<!-- Ghost (transparent) -->
<button class="btn btn-ghost">Ghost</button>

<!-- Outline -->
<button class="btn btn-outline">Outline</button>

<!-- Link -->
<button class="btn btn-link">Link Button</button>

<!-- Disabled -->
<button class="btn btn-primary" disabled>Disabled</button>

<!-- With loading spinner -->
<button class="btn btn-primary" disabled>
    <span class="loading loading-spinner loading-sm"></span>
    Loading
</button>
```

### Button Sizes

```razor
<button class="btn btn-xs">Extra Small</button>
<button class="btn btn-sm">Small</button>
<button class="btn">Default</button>
<button class="btn btn-lg">Large</button>

<!-- Full width -->
<button class="btn w-full">Full Width</button>
```

### Button with Icon

```razor
<button class="btn gap-2">
    <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
    </svg>
    Click me
</button>
```

### Button Square (Icon Only)

```razor
<button class="btn btn-square btn-ghost">
    <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
    </svg>
</button>
```

---

## 7. Tables

### Basic Table

```razor
<div class="overflow-x-auto">
    <table class="table">
        <thead>
            <tr>
                <th>Name</th>
                <th>Job</th>
                <th>Status</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>John Doe</td>
                <td>Engineer</td>
                <td>Active</td>
            </tr>
        </tbody>
    </table>
</div>
```

### Table Compact with Zebra

```razor
<div class="overflow-x-auto rounded-lg border border-base-300">
    <table class="table table-xs table-zebra table-pin-rows">
        <thead>
            <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Status</th>
                <th>Action</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var item in items)
            {
                <tr class="hover">
                    <td>@item.Name</td>
                    <td>@item.Email</td>
                    <td>
                        <span class="badge @(item.IsActive ? "badge-success" : "badge-error")">
                            @(item.IsActive ? "Active" : "Inactive")
                        </span>
                    </td>
                    <td>
                        <button class="btn btn-ghost btn-xs">Edit</button>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

---

## 8. Badges

```razor
<!-- Basic badges -->
<span class="badge">Default</span>
<span class="badge badge-primary">Primary</span>
<span class="badge badge-secondary">Secondary</span>

<!-- With colors -->
<span class="badge badge-success">Success</span>
<span class="badge badge-error">Error</span>
<span class="badge badge-warning">Warning</span>
<span class="badge badge-info">Info</span>

<!-- Soft variant -->
<span class="badge badge-soft badge-success">Soft Success</span>
<span class="badge badge-soft badge-error">Soft Error</span>

<!-- Sizes -->
<span class="badge badge-xs">XS</span>
<span class="badge badge-sm">SM</span>
<span class="badge">Default</span>
<span class="badge badge-lg">LG</span>
```

---

## 9. Tabs

### Tabs Border

```razor
<div role="tablist" class="tabs tabs-border">
    <a role="tab" class="tab @(activeTab == "summary" ? "tab-active" : "")" 
       @onclick="@(() => activeTab = "summary")">
        Summary
    </a>
    <a role="tab" class="tab @(activeTab == "details" ? "tab-active" : "")" 
       @onclick="@(() => activeTab = "details")">
        Details
    </a>
    <a role="tab" class="tab @(activeTab == "history" ? "tab-active" : "")" 
       @onclick="@(() => activeTab = "history")">
        History
    </a>
</div>

@if (activeTab == "summary")
{
    <div>Summary content</div>
}
else if (activeTab == "details")
{
    <div>Details content</div>
}
else if (activeTab == "history")
{
    <div>History content</div>
}
```

### Tabs Boxed

```razor
<div role="tablist" class="tabs tabs-box">
    <button role="tab" class="tab">Tab 1</button>
    <button role="tab" class="tab tab-active">Tab 2</button>
    <button role="tab" class="tab">Tab 3</button>
</div>
```

---

## 10. Loading States & Skeletons

### Skeleton Placeholders

```razor
@if (isLoading)
{
    <div class="space-y-4">
        <!-- Card skeleton -->
        <div class="card card-border bg-base-100">
            <div class="card-body gap-3 p-4">
                <div class="skeleton h-4 w-24"></div>
                <div class="skeleton h-8 w-16"></div>
                <div class="skeleton h-3 w-28"></div>
            </div>
        </div>

        <!-- Multiple skeletons -->
        <div class="space-y-3">
            <div class="skeleton h-6 w-full"></div>
            @for (int i = 0; i < 8; i++)
            {
                <div class="skeleton h-4 w-full"></div>
            }
        </div>

        <!-- Image skeleton -->
        <div class="skeleton h-48 w-48 rounded-full"></div>
    </div>
}
else
{
    <!-- Real content -->
}
```

### Loading Spinners

```razor
<button class="btn btn-primary" disabled>
    <span class="loading loading-spinner loading-xs"></span>
    XS
</button>

<button class="btn btn-primary" disabled>
    <span class="loading loading-spinner loading-sm"></span>
    SM
</button>

<button class="btn btn-primary" disabled>
    <span class="loading loading-spinner loading-md"></span>
    MD (default)
</button>

<button class="btn btn-primary" disabled>
    <span class="loading loading-spinner loading-lg"></span>
    LG
</button>
```

---

## 11. Tooltips

### Tooltip Positioning

```razor
<!-- Top -->
<div class="tooltip" data-tip="Tooltip top">
    <button class="btn">Hover me</button>
</div>

<!-- Bottom -->
<div class="tooltip tooltip-bottom" data-tip="Tooltip bottom">
    <button class="btn">Hover me</button>
</div>

<!-- Left -->
<div class="tooltip tooltip-left" data-tip="Tooltip left">
    <button class="btn">Hover me</button>
</div>

<!-- Right -->
<div class="tooltip tooltip-right" data-tip="Tooltip right">
    <button class="btn">Hover me</button>
</div>

<!-- Open by default (useful for titles) -->
<div class="tooltip tooltip-right" data-tip="Help text" role="status">
    <span class="question-mark">?</span>
</div>
```

---

## 12. Dividers

```razor
<!-- Horizontal divider -->
<div class="divider"></div>

<!-- Divider with text -->
<div class="divider">OR</div>

<!-- Vertical divider (in flexbox) -->
<div class="flex items-center gap-4">
    <div>Left</div>
    <div class="divider divider-horizontal"></div>
    <div>Right</div>
</div>
```

---

## 13. Join (Button Groups)

### Input + Button

```razor
<div class="join w-full">
    <input type="text" 
           class="input join-item flex-1 input-bordered" 
           placeholder="Search..." />
    <button class="btn btn-primary join-item">Search</button>
</div>
```

### Pagination

```razor
<div class="join">
    <button class="join-item btn btn-sm" disabled="@(currentPage <= 1)">«</button>
    
    @for (int p = 1; p <= totalPages; p++)
    {
        var pageNum = p;
        <button class="join-item btn btn-sm @(pageNum == currentPage ? "btn-active" : "")" 
                @onclick="@(() => GoToPage(pageNum))">
            @p
        </button>
    }
    
    <button class="join-item btn btn-sm" disabled="@(currentPage >= totalPages)">»</button>
</div>
```

### Button Group

```razor
<div class="join">
    <button class="join-item btn">Copy</button>
    <button class="join-item btn">Edit</button>
    <button class="join-item btn">Delete</button>
</div>
```

---

## 14. Responsive Patterns

### Grid Auto

```razor
<!-- Automático: 1 col en móvil, 2 en sm, 3 en lg -->
<div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
    @foreach (var item in items)
    {
        <div class="card bg-base-100"><!-- content --></div>
    }
</div>
```

### Flex Responsive

```razor
<!-- Stack en móvil, horizontal en lg -->
<div class="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
    <div>Left content</div>
    <div>Right content</div>
</div>
```

### Hide/Show Responsive

```razor
<!-- Hidden en móvil, visible en sm y superior -->
<span class="hidden sm:inline">Full Text</span>

<!-- Visible en móvil, oculto en lg -->
<span class="lg:hidden">Short</span>

<!-- Diferentes tamaños según responsive -->
<h1 class="text-xl sm:text-2xl lg:text-4xl">Responsive Title</h1>
```

---

## 15. Custom Utility Example

En `app.css`:

```css
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

Uso:

```razor
<div class="fade-in">
    Content with smooth fade-in animation
</div>
```

---

## Colores Comunes para Copiar/Pegar

```
Primary accent: bg-primary, text-primary, border-primary
Success: bg-success, text-success, badge-success
Error: bg-error, text-error, badge-error
Warning: bg-warning, text-warning, badge-warning
Base colors: bg-base-100, bg-base-200, text-base-content
Opacity modifiers: /10, /20, /30, /40, /50, /60, /70, /80, /90
```

---

**Estos ejemplos están listos para copiar directamente en OEA.Checklist**
