// OEA Checklist - Helpers de tema, sesión y sidebar persistidos.
// Cookie 'oea_theme' permite al servidor conocer el tema durante SSR/re-render.

window.oeaTheme = {
    LIGHT: "emerald",
    DARK: "night",
    _normalize: function (t) {
        return (t === "emerald" || t === "night") ? t : "emerald";
    },
    _readCookie: function () {
        var m = document.cookie.match(/(?:^|;\s*)oea_theme=([^;]+)/);
        return m ? decodeURIComponent(m[1]) : null;
    },
    _writeCookie: function (t) {
        var maxAge = 60 * 60 * 24 * 365;
        document.cookie = "oea_theme=" + encodeURIComponent(t) +
            "; Max-Age=" + maxAge + "; Path=/; SameSite=Lax";
    },
    get: function () {
        try {
            var t = localStorage.getItem("oea_theme") || this._readCookie();
            return this._normalize(t);
        } catch (e) { return "emerald"; }
    },
    set: function (theme) {
        theme = this._normalize(theme);
        try { localStorage.setItem("oea_theme", theme); } catch (e) { }
        try { this._writeCookie(theme); } catch (e) { }
        document.documentElement.setAttribute("data-theme", theme);
    }
};

window.oeaSession = {
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
window.oeaDetails = {
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
window.oeaSidebar = {
    STORAGE_KEY: "oea_sidebar_collapsed",

    applyInitial: function (checkboxId) {
        try {
            var cb = document.getElementById(checkboxId);
            if (!cb) return;
            var collapsed = localStorage.getItem(this.STORAGE_KEY) === "true";
            cb.checked = !collapsed;
        } catch (e) { }
    },

    persist: function (checkboxId) {
        try {
            var cb = document.getElementById(checkboxId);
            if (!cb) return;
            // El evento click en el <label> alterna el checkbox DESPUÉS de este handler,
            // así que invertimos su estado actual para reflejar el estado post-click.
            var willBeCollapsed = cb.checked; // estará "close" tras el toggle
            localStorage.setItem(this.STORAGE_KEY, willBeCollapsed ? "true" : "false");
        } catch (e) { }
    }
};
