using Microsoft.AspNetCore.Http;

namespace ComplianceControlCenter.Web.Core.State;

/// <summary>
/// Modo compacto: reduce el tamaño base (font-size del &lt;html&gt;) para
/// compensar la escala de Windows a 125%/150% y mostrar más contenido en
/// pantalla. Todo Tailwind usa unidades relativas (rem), por lo que espacios,
/// paddings y tipografías escalan de forma proporcional.
///
/// Se persiste en cookie <c>ccc_compact</c> y en <c>localStorage</c>:
/// <list type="bullet">
///   <item>Cookie → leída por el servidor para pintar la clase en el &lt;html&gt; y evitar flash.</item>
///   <item>localStorage → usada por el script pre-boot de <c>App.razor</c>.</item>
/// </list>
/// </summary>
public class CompactModeState
{
    public const string CookieName = "ccc_compact";
    public const string HtmlClass = "compact-mode";

    private bool _isCompact;

    public CompactModeState(IHttpContextAccessor httpContextAccessor)
    {
        var cookie = httpContextAccessor.HttpContext?.Request?.Cookies[CookieName];
        _isCompact = string.Equals(cookie, "1", StringComparison.Ordinal);
    }

    public bool IsCompact
    {
        get => _isCompact;
        private set
        {
            if (_isCompact == value) return;
            _isCompact = value;
            OnChange?.Invoke();
        }
    }

    public event Action? OnChange;

    public void Set(bool compact) => IsCompact = compact;

    public void Toggle() => IsCompact = !_isCompact;
}
