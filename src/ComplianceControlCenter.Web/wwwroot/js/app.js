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

// Modo compacto — persiste el estado en localStorage + cookie.
// La cookie 'ccc_compact' permite al servidor pintar la clase en <html>
// durante SSR y evitar el flash al cargar la página.
window.cccCompact = {
    CLASS: "compact-mode",
    STORAGE_KEY: "ccc_compact",
    COOKIE_NAME: "ccc_compact",
    _readCookie: function () {
        var m = document.cookie.match(/(?:^|;\s*)ccc_compact=([^;]+)/);
        return m ? decodeURIComponent(m[1]) : null;
    },
    _writeCookie: function (on) {
        var maxAge = 60 * 60 * 24 * 365;
        document.cookie = "ccc_compact=" + (on ? "1" : "0") +
            "; Max-Age=" + maxAge + "; Path=/; SameSite=Lax";
    },
    get: function () {
        try {
            var v = localStorage.getItem(this.STORAGE_KEY);
            if (v === null) v = this._readCookie();
            return v === "1";
        } catch (e) { return false; }
    },
    set: function (on) {
        on = !!on;
        try { localStorage.setItem(this.STORAGE_KEY, on ? "1" : "0"); } catch (e) { }
        try { this._writeCookie(on); } catch (e) { }
        var el = document.documentElement;
        if (on) el.classList.add(this.CLASS);
        else    el.classList.remove(this.CLASS);
    },
    toggle: function () {
        var next = !this.get();
        this.set(next);
        return next;
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

// Popover: mide el trigger y devuelve si debe abrirse hacia arriba y/o hacia la izquierda.
// popoverH: altura estimada del popover (px). popoverW: ancho estimado del popover (px).
window.cccPopover = {
    getPlacement: function (triggerEl, popoverH, popoverW) {
        try {
            var rect = triggerEl.getBoundingClientRect();
            var vh = window.innerHeight || document.documentElement.clientHeight;
            var vw = window.innerWidth  || document.documentElement.clientWidth;
            var spaceBelow = vh - rect.bottom;
            var spaceAbove = rect.top;
            var spaceRight = vw - rect.left;
            return {
                openUp:   spaceBelow < (popoverH + 8) && spaceAbove >= spaceBelow,
                openLeft: spaceRight < (popoverW + 8)
            };
        } catch (e) {
            return { openUp: false, openLeft: false };
        }
    }
};

// Panel lateral: bloquea/desbloquea el scroll del <main> mientras el panel está abierto.
// Agrega overflow-hidden al <main> para que no se pueda hacer scroll debajo del overlay.
window.cccPanel = {
    lock: function () {
        try {
            var main = document.querySelector("main");
            if (main) main.classList.add("overflow-hidden");
        } catch (e) { }
    },
    unlock: function () {
        try {
            var main = document.querySelector("main");
            if (main) main.classList.remove("overflow-hidden");
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
