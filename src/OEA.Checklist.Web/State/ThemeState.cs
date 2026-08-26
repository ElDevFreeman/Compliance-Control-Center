using Microsoft.AspNetCore.Http;

namespace OEA.Checklist.Web.State;

/// <summary>
/// Estado del tema daisyUI ("emerald" claro / "dark" oscuro).
///
/// El tema se persiste en <b>dos lados</b> para evitar el flash y para que el
/// servidor sepa qué tema pintar en el atributo <c>data-theme</c> del <c>&lt;html&gt;</c>:
/// <list type="bullet">
///   <item>Cookie <c>oea_theme</c> (leída por el servidor en cada request).</item>
///   <item>localStorage <c>oea_theme</c> (para JS pre-boot y persistencia cliente).</item>
/// </list>
/// </summary>
public class ThemeState
{
    public const string LightTheme = "emerald";
    public const string DarkTheme = "dark";
    public const string CookieName = "oea_theme";

    private string _theme = LightTheme;

    public ThemeState(IHttpContextAccessor httpContextAccessor)
    {
        // Al iniciar el circuit (o el request en SSR), toma el tema de la cookie.
        var cookie = httpContextAccessor.HttpContext?.Request?.Cookies[CookieName];
        if (IsValid(cookie))
        {
            _theme = cookie!;
        }
    }

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

    // Alias legacy para compatibilidad.
    public string Current => _theme;

    public bool IsDark => _theme == DarkTheme;

    public event Action? OnChange;

    public void Set(string theme)
    {
        if (!IsValid(theme)) return;
        Theme = theme;
    }

    // Alias legacy.
    public void SetTheme(string theme) => Set(theme);

    public void Toggle() => Theme = IsDark ? LightTheme : DarkTheme;

    private static bool IsValid(string? theme) =>
        theme == LightTheme || theme == DarkTheme;
}
