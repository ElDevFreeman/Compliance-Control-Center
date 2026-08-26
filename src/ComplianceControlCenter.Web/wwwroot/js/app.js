// Compliance Control Center - Helpers de tema, sesión y sidebar persistidos.
// Cookie 'ccc_theme' permite al servidor conocer el tema durante SSR/re-render.

window.cccTheme = {
    LIGHT: "emerald",
    DARK: "dark",
    _normalize: function (t) {
        return (t === "emerald" || t === "dark") ? t : "emerald";
    },
    _readCookie: function () {
        var m = document.cookie.match(/(?:^|;\s*)ccc_theme=([^;]+)/);
        return m ? decodeURIComponent(m[1]) : null;
    },
    _writeCookie: function (t) {
        var maxAge = 60 * 60 * 24 * 365;
        document.cookie = "ccc_theme=" + encodeURIComponent(t) +
            "; Max-Age=" + maxAge + "; Path=/; SameSite=Lax";
    },
    get: function () {
        try {
            var t = localStorage.getItem("ccc_theme") || this._readCookie();
            return this._normalize(t);
        } catch (e) { return "emerald"; }
    },
    set: function (theme) {
        theme = this._normalize(theme);
        try { localStorage.setItem("ccc_theme", theme); } catch (e) { }
        try { this._writeCookie(theme); } catch (e) { }
        document.documentElement.setAttribute("data-theme", theme);
    }
};

window.cccSession = {
    get: function (key) {
        try { return localStorage.getItem(key); } catch (e) { return null; }
    },
    set: function (key, value) {
        try { localStorage.setItem(key, value); } catch (e) { }
    },
    remove: function (key) {
        try { localStorage.removeItem(key); } catch (e) { }
    }
};

// Cierra un <details> (usado por StatusMenu al seleccionar una opción,
// porque Blazor no controla el atributo `open` de forma reactiva).
window.cccDetails = {
    close: function (el) {
        try {
            if (el && el.tagName === "DETAILS") {
                el.removeAttribute("open");
            }
        } catch (e) { }
    }
};

// Sidebar: sincroniza el checkbox del drawer con localStorage.
// El checkbox se llama "sidebar-drawer" en MainLayout.
// - collapsed=true  => checkbox NO marcado => is-drawer-close:*
// - collapsed=false => checkbox marcado    => is-drawer-open:*
window.cccSidebar = {
    STORAGE_KEY: "ccc_sidebar_collapsed",
    _wired: false,

    applyInitial: function (checkboxId) {
        try {
            var cb = document.getElementById(checkboxId);
            if (!cb) return;
            var collapsed = localStorage.getItem(this.STORAGE_KEY) === "true";
            cb.checked = !collapsed;

            // Registra un listener 'change' UNA sola vez para persistir cualquier
            // cambio del checkbox, sin importar qué <label> o script lo dispare.
            if (!this._wired) {
                var self = this;
                cb.addEventListener("change", function () {
                    try {
                        // checked=true  => drawer abierto  => collapsed=false
                        // checked=false => drawer cerrado  => collapsed=true
                        localStorage.setItem(self.STORAGE_KEY, cb.checked ? "false" : "true");
                    } catch (e) { }
                });
                this._wired = true;
            }
        } catch (e) { }
    }
};
