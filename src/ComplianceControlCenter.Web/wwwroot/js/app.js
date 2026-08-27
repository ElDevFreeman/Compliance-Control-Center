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
    AUTO_KEY: "ccc_sidebar_auto",   // "1" si nunca ha sido tocado manualmente
    // Debajo de este ancho de viewport, el sidebar se colapsa automáticamente
    // en la primera visita (evita que consuma demasiado espacio en pantallas
    // pequeñas o con Windows a 125-150%). El usuario puede sobreescribirlo.
    AUTO_COLLAPSE_BELOW_PX: 1400,
    _wired: false,

    applyInitial: function (checkboxId) {
        try {
            var cb = document.getElementById(checkboxId);
            if (!cb) return;

            // 1) Decidir estado inicial.
            var stored = localStorage.getItem(this.STORAGE_KEY);
            var isAuto = localStorage.getItem(this.AUTO_KEY) !== "0";  // por defecto en modo auto
            var collapsed;

            if (stored === null || isAuto) {
                // Modo automático: colapsar en pantallas estrechas.
                collapsed = window.innerWidth < this.AUTO_COLLAPSE_BELOW_PX;
                localStorage.setItem(this.STORAGE_KEY, collapsed ? "true" : "false");
            } else {
                // El usuario tomó una decisión manual — respetarla.
                collapsed = stored === "true";
            }

            cb.checked = !collapsed;

            // 2) Registra 'change' UNA sola vez para persistir cambios manuales.
            //    Al cambiar manualmente, salimos del modo auto para no reasignar
            //    en próximas cargas.
            if (!this._wired) {
                var self = this;
                cb.addEventListener("change", function () {
                    try {
                        localStorage.setItem(self.STORAGE_KEY, cb.checked ? "false" : "true");
                        localStorage.setItem(self.AUTO_KEY, "0");  // decisión manual
                    } catch (e) { }
                });
                this._wired = true;
            }

            // 3) Si sigue en auto, re-evaluar al redimensionar la ventana
            //    (por si el usuario mueve la ventana entre monitores).
            if (isAuto) {
                var self = this;
                var lastCollapsed = collapsed;
                window.addEventListener("resize", function () {
                    // Solo re-evaluamos si aún estamos en modo auto.
                    if (localStorage.getItem(self.AUTO_KEY) === "0") return;
                    var shouldCollapse = window.innerWidth < self.AUTO_COLLAPSE_BELOW_PX;
                    if (shouldCollapse === lastCollapsed) return;
                    lastCollapsed = shouldCollapse;
                    var el = document.getElementById(checkboxId);
                    if (!el) return;
                    el.checked = !shouldCollapse;
                    localStorage.setItem(self.STORAGE_KEY, shouldCollapse ? "true" : "false");
                    // Dispara evento 'change' sintético para que otros listeners
                    // reaccionen si los hay. Marcamos con un flag para evitar
                    // que nuestro propio handler ponga AUTO_KEY=0.
                    var ev = new Event("change", { bubbles: true });
                    ev._cccAuto = true;
                    el.dispatchEvent(ev);
                    // Restaurar AUTO_KEY porque el handler anterior lo puso a 0.
                    localStorage.setItem(self.AUTO_KEY, "1");
                });
            }
        } catch (e) { }
    }
};
