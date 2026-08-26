namespace OEA.Checklist.Web.State;

/// <summary>
/// Coordina la apertura del <c>LoginModal</c> desde cualquier componente de la app.
///
/// El modal en sí vive en <c>MainLayout</c> y se suscribe a <see cref="OnOpenRequested"/>.
/// Cualquier página o componente puede llamar <see cref="Open"/> para pedir que se muestre.
///
/// Registrar como <b>Scoped</b> — un estado por circuit de Blazor Server.
/// </summary>
public sealed class LoginModalState
{
    /// <summary>Se dispara cuando alguien pide abrir el modal.</summary>
    public event Action<string?>? OnOpenRequested;

    /// <summary>URL a la que redirigir tras un login exitoso. Null = página actual.</summary>
    public string? ReturnUrl { get; private set; }

    /// <summary>Abre el modal. <paramref name="returnUrl"/> es opcional (default: página actual).</summary>
    public void Open(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        OnOpenRequested?.Invoke(returnUrl);
    }
}
