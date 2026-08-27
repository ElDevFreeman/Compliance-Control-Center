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

// Popover FIXED: calcula coordenadas absolutas de viewport para que el popup
// use `position: fixed` y escape de contenedores con `overflow: hidden/auto`.
// Devuelve { top, left, openUp, openLeft } que el componente aplica como
// estilos inline al popup. Requerido para dropdowns dentro de la tabla del
// Checklist (que tiene overflow-x-auto para el scroll horizontal).
//
// popoverH / popoverW: dimensiones estimadas del popup (px).
// preferredAlign: "start" | "center" | "end" — alineación horizontal preferida
//                                                (aplica solo en side="bottom").
// side: "bottom" (default) — el popup se abre debajo/arriba del trigger.
//       "right"            — el popup se abre a la derecha/izquierda del trigger,
//                             alineado verticalmente al mismo top.
//
// Además expone `measureAndPlace(popoverEl, triggerEl, preferredAlign, side)`
// que mide el popover REAL ya renderizado en el DOM. Esto elimina errores
// causados por estimaciones de altura/ancho: útil cuando la estimación en
// px no coincide con el tamaño real y el clamp separa el popup del trigger.
window.cccFixedPopover = (function () {
    function compute(rect, popoverH, popoverW, preferredAlign, side) {
        var vh = window.innerHeight || document.documentElement.clientHeight;
        var vw = window.innerWidth  || document.documentElement.clientWidth;
        var GAP = 4;
        var MARGIN = 8;

        side = side === "right" ? "right" : "bottom";

        if (side === "right") {
            // Colocación lateral: preferimos a la derecha del trigger.
            var spaceRight = vw - rect.right;
            var spaceLeft  = rect.left;
            var openLeft   = spaceRight < (popoverW + GAP) && spaceLeft >= spaceRight;

            var left = openLeft
                ? (rect.left - popoverW - GAP)
                : (rect.right + GAP);
            left = Math.max(MARGIN, Math.min(left, vw - popoverW - MARGIN));

            var top = rect.top;
            top = Math.max(MARGIN, Math.min(top, vh - popoverH - MARGIN));

            return { top: top, left: left, openUp: false, openLeft: openLeft };
        }

        // side === "bottom"
        var spaceBelow = vh - rect.bottom;
        var spaceAbove = rect.top;
        // Preferir arriba solo si abajo no cabe Y arriba tiene más espacio.
        var openUp = spaceBelow < (popoverH + GAP) && spaceAbove > spaceBelow;

        // Alineación horizontal.
        var leftIfStart  = rect.left;
        var leftIfEnd    = rect.right - popoverW;
        var leftIfCenter = rect.left + (rect.width - popoverW) / 2;

        var openLeftB = false;
        var leftB;
        if (preferredAlign === "end") {
            openLeftB = leftIfEnd >= MARGIN;
            leftB = openLeftB ? leftIfEnd : leftIfStart;
        } else if (preferredAlign === "center") {
            leftB = leftIfCenter;
        } else {
            openLeftB = (rect.left + popoverW > vw - MARGIN) && (leftIfEnd >= MARGIN);
            leftB = openLeftB ? leftIfEnd : leftIfStart;
        }
        leftB = Math.max(MARGIN, Math.min(leftB, vw - popoverW - MARGIN));

        var topB = openUp
            ? (rect.top - popoverH - GAP)
            : (rect.bottom + GAP);
        topB = Math.max(MARGIN, Math.min(topB, vh - popoverH - MARGIN));

        return { top: topB, left: leftB, openUp: openUp, openLeft: openLeftB };
    }

    return {
        // API antigua: usa estimaciones de tamaño.
        getPlacement: function (triggerEl, popoverH, popoverW, preferredAlign, side) {
            try {
                var rect = triggerEl.getBoundingClientRect();
                return compute(rect, popoverH, popoverW, preferredAlign, side);
            } catch (e) {
                return { top: 0, left: 0, openUp: false, openLeft: false };
            }
        },

        // API preferida: mide el popover REAL ya renderizado (evita clamp
        // agresivo cuando la altura/ancho estimados eran mayores al real).
        // El popover debe estar en el DOM. Recomendación: render inicial con
        // visibility:hidden para evitar el flash, luego aplicar top/left y
        // quitar el hidden.
        measureAndPlace: function (popoverEl, triggerEl, preferredAlign, side) {
            try {
                if (!popoverEl || !triggerEl) {
                    return { top: 0, left: 0, openUp: false, openLeft: false };
                }
                var rect = triggerEl.getBoundingClientRect();
                var pr   = popoverEl.getBoundingClientRect();
                // Ancho/alto mínimos por si el popover aún no tiene tamaño.
                var w = pr.width  > 0 ? pr.width  : popoverEl.offsetWidth;
                var h = pr.height > 0 ? pr.height : popoverEl.offsetHeight;
                return compute(rect, h, w, preferredAlign, side);
            } catch (e) {
                return { top: 0, left: 0, openUp: false, openLeft: false };
            }
        }
    };
})();

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

// Drag-to-scroll: permite hacer scroll horizontal arrastrando con el mouse
// (como Trello o Google Sheets). Se activa en cualquier contenedor que tenga
// [data-drag-scroll="1"]. Ignora clicks en elementos interactivos (inputs,
// buttons, links, celdas editables) para no interferir con la edición normal.
//
// Uso: <div class="overflow-x-auto ccc-drag-scroll" data-drag-scroll="1">...</div>
// El observer detecta contenedores nuevos automáticamente (útil con Blazor).
window.cccDragScroll = {
    _wired: false,
    // Selectores que NO deben iniciar drag (permiten uso normal del control).
    IGNORE_SELECTOR: 'input, textarea, select, button, a, label, ' +
        '[contenteditable="true"], [role="button"], [data-no-drag]',
    // Distancia mínima (px) que debe moverse el mouse antes de considerarlo drag.
    // Debajo de esto es un click normal.
    DRAG_THRESHOLD: 4,

    _onPointerDown: function (e) {
        // Solo botón principal (izquierdo) y no si viene de touch (los touches
        // ya scrollean nativamente en dispositivos móviles).
        if (e.button !== 0 || e.pointerType === 'touch') return;

        var container = e.currentTarget;
        // Si el click fue en un control interactivo, no interceptar.
        if (e.target.closest(window.cccDragScroll.IGNORE_SELECTOR)) return;

        // Si no hay overflow horizontal, no hay nada que scrollear.
        if (container.scrollWidth <= container.clientWidth) return;

        var state = {
            startX: e.clientX,
            startScrollLeft: container.scrollLeft,
            dragging: false,   // se activa cuando se supera el threshold
            pointerId: e.pointerId
        };
        container._dragState = state;

        var onMove = function (ev) {
            var s = container._dragState;
            if (!s) return;
            var dx = ev.clientX - s.startX;

            if (!s.dragging) {
                if (Math.abs(dx) < window.cccDragScroll.DRAG_THRESHOLD) return;
                s.dragging = true;
                container.classList.add('is-dragging');
                container.setPointerCapture(s.pointerId);
                // Evita que el navegador seleccione texto durante el drag.
                document.body.style.userSelect = 'none';
            }

            container.scrollLeft = s.startScrollLeft - dx;
            ev.preventDefault();
        };

        var cleanup = function () {
            var s = container._dragState;
            container._dragState = null;
            container.removeEventListener('pointermove', onMove);
            container.removeEventListener('pointerup', cleanup);
            container.removeEventListener('pointercancel', cleanup);
            if (s && s.dragging) {
                container.classList.remove('is-dragging');
                try { container.releasePointerCapture(s.pointerId); } catch (e) { }
                document.body.style.userSelect = '';
                // Bloquea el próximo `click` sintético para que un drag no
                // dispare accidentalmente el click de una celda al soltar.
                container._blockNextClick = true;
                setTimeout(function () { container._blockNextClick = false; }, 0);
            }
        };

        container.addEventListener('pointermove', onMove);
        container.addEventListener('pointerup', cleanup);
        container.addEventListener('pointercancel', cleanup);
    },

    _onClickCapture: function (e) {
        // Si acabamos de terminar un drag, cancela el click asociado.
        var container = e.currentTarget;
        if (container._blockNextClick) {
            e.stopPropagation();
            e.preventDefault();
        }
    },

    attach: function (container) {
        if (!container || container._dragScrollAttached) return;
        container._dragScrollAttached = true;
        container.addEventListener('pointerdown', this._onPointerDown);
        // useCapture=true para interceptar antes que los handlers de las celdas.
        container.addEventListener('click', this._onClickCapture, true);
    },

    scan: function (root) {
        var scope = root || document;
        var nodes = scope.querySelectorAll('[data-drag-scroll="1"]');
        for (var i = 0; i < nodes.length; i++) {
            window.cccDragScroll.attach(nodes[i]);
        }
    },

    init: function () {
        if (this._wired) return;
        this._wired = true;

        // Escaneo inicial de contenedores ya renderizados.
        this.scan(document);

        // Blazor renderiza async: observa mutaciones y engancha nuevos contenedores.
        var observer = new MutationObserver(function (mutations) {
            for (var i = 0; i < mutations.length; i++) {
                var m = mutations[i];
                for (var j = 0; j < m.addedNodes.length; j++) {
                    var n = m.addedNodes[j];
                    if (n.nodeType !== 1) continue;  // sólo elementos
                    if (n.matches && n.matches('[data-drag-scroll="1"]')) {
                        window.cccDragScroll.attach(n);
                    }
                    if (n.querySelectorAll) {
                        window.cccDragScroll.scan(n);
                    }
                }
            }
        });
        observer.observe(document.body, { childList: true, subtree: true });
    }
};

// Auto-inicialización al cargar el DOM (funciona con Blazor Server).
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () { window.cccDragScroll.init(); });
} else {
    window.cccDragScroll.init();
}

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
