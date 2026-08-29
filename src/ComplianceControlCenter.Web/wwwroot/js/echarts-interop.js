// ECharts interop for Blazor Server.
// Manages chart instances keyed by DOM element id.
// Supports init, update (merge), dispose, and auto-resize.
//
// NO usa el tema built-in "dark" de ECharts porque sus colores
// (fondo #333, texto blanco fijo) no coinciden con DaisyUI.
// En su lugar, lee los CSS custom properties del tema activo y
// aplica colores de texto, ejes, leyenda, tooltip y gauge track
// adaptados al tema light/dark de DaisyUI.

window.echartsInterop = (function () {
    var _instances = {};

    // ── Detección de tema ─────────────────────────────────────────
    function isDarkTheme() {
        try {
            var t = document.documentElement.getAttribute("data-theme") || "";
            return t === "dark" || t === "dim" || t === "night" || t === "dracula";
        } catch (e) { return false; }
    }

    // Resuelve un color de DaisyUI (--color-{name}) a rgb.
    // DaisyUI 5 usa oklch que canvas no entiende. Creamos un elemento
    // temporal, le aplicamos la variable, y leemos el color computado.
    function resolveColor(name, fallback) {
        try {
            var el = document.createElement("span");
            el.style.display = "none";
            el.style.color = "var(--color-" + name + ")";
            document.body.appendChild(el);
            var resolved = getComputedStyle(el).color;
            document.body.removeChild(el);
            if (!resolved || resolved === "") return fallback;
            return resolved;
        } catch (e) {
            return fallback;
        }
    }

    // Genera la paleta de colores derivada del tema activo de DaisyUI.
    function getThemePalette() {
        var dark = isDarkTheme();
        var textColor = resolveColor("base-content", dark ? "#a6adbb" : "#1f2937");
        return {
            dark: dark,
            text: textColor,
            textSub: dark ? "rgba(255,255,255,0.5)" : "rgba(0,0,0,0.45)",
            axisLine: dark ? "rgba(255,255,255,0.15)" : "rgba(0,0,0,0.12)",
            splitLine: dark ? "rgba(255,255,255,0.08)" : "rgba(0,0,0,0.08)",
            gaugeTrack: dark ? "rgba(255,255,255,0.12)" : "#e5e7eb",
            tooltipBg: dark ? "rgba(30,30,30,0.92)" : "rgba(255,255,255,0.96)",
            tooltipBorder: dark ? "rgba(255,255,255,0.1)" : "rgba(0,0,0,0.08)",
            tooltipText: dark ? "#e5e7eb" : "#1f2937"
        };
    }

    // ── Parcheo profundo de opciones ──────────────────────────────
    // Recorre las opciones y aplica colores del tema donde corresponde.
    // Esto resuelve el problema de colores hardcodeados en el JSON
    // generado server-side que no conoce el tema del browser.
    function patchOptions(opts, palette) {
        // Fondo transparente siempre
        opts.backgroundColor = "transparent";

        // Texto global
        if (!opts.textStyle) opts.textStyle = {};
        opts.textStyle.color = palette.text;

        // Legend
        if (opts.legend) {
            if (!opts.legend.textStyle) opts.legend.textStyle = {};
            opts.legend.textStyle.color = palette.text;
        }

        // Tooltip
        if (!opts.tooltip) opts.tooltip = {};
        opts.tooltip.backgroundColor = palette.tooltipBg;
        opts.tooltip.borderColor = palette.tooltipBorder;
        if (!opts.tooltip.textStyle) opts.tooltip.textStyle = {};
        opts.tooltip.textStyle.color = palette.tooltipText;

        // xAxis (puede ser objeto o array)
        patchAxis(opts.xAxis, palette);
        // yAxis
        patchAxis(opts.yAxis, palette);

        // Radar
        if (opts.radar) {
            if (opts.radar.axisName) {
                opts.radar.axisName.color = palette.text;
            }
            if (opts.radar.splitLine) {
                if (!opts.radar.splitLine.lineStyle) opts.radar.splitLine.lineStyle = {};
                opts.radar.splitLine.lineStyle.color = palette.splitLine;
            }
            if (opts.radar.splitArea && opts.radar.splitArea.areaStyle) {
                var a = palette.dark ? 0.06 : 0.04;
                var b = palette.dark ? 0.12 : 0.08;
                opts.radar.splitArea.areaStyle.color = [
                    "rgba(99,102,241," + a + ")",
                    "rgba(99,102,241," + b + ")"
                ];
            }
        }

        // Graphic (usado por mini donut con texto central)
        if (opts.graphic && Array.isArray(opts.graphic)) {
            for (var i = 0; i < opts.graphic.length; i++) {
                var g = opts.graphic[i];
                if (g && g.style && g.style.fill) {
                    g.style.fill = palette.text;
                }
            }
        }

        // Series: parchear gauge track y labels
        if (opts.series && Array.isArray(opts.series)) {
            for (var i = 0; i < opts.series.length; i++) {
                var s = opts.series[i];
                if (!s) continue;

                // Gauge: parchear el track (axisLine) y labels
                if (s.type === "gauge") {
                    if (s.axisLine && s.axisLine.lineStyle && s.axisLine.lineStyle.color) {
                        // El color del gauge track es un array [[1, "#e5e7eb"]]
                        // Reemplazar el color del segmento de fondo
                        var lineColor = s.axisLine.lineStyle.color;
                        if (Array.isArray(lineColor)) {
                            for (var j = 0; j < lineColor.length; j++) {
                                if (Array.isArray(lineColor[j]) && lineColor[j].length === 2) {
                                    lineColor[j][1] = palette.gaugeTrack;
                                }
                            }
                        }
                    }
                    // Detail (el número central del gauge)
                    if (s.detail && s.detail.color) {
                        // Mantener el color semántico del valor (verde/rojo/etc)
                        // No parchear — es intencional
                    }
                }

                // Bar/Line: labels dentro de barras
                if (s.label && s.label.color) {
                    // Solo parchear si es un color genérico
                }
            }
        }

        return opts;
    }

    // Parchea un eje (xAxis o yAxis), que puede ser objeto o array.
    function patchAxis(axis, palette) {
        if (!axis) return;
        var axes = Array.isArray(axis) ? axis : [axis];
        for (var i = 0; i < axes.length; i++) {
            var ax = axes[i];
            if (!ax) continue;
            // Axis labels
            if (ax.axisLabel) {
                ax.axisLabel.color = palette.text;
            }
            // Axis line
            if (ax.axisLine) {
                if (!ax.axisLine.lineStyle) ax.axisLine.lineStyle = {};
                ax.axisLine.lineStyle.color = palette.axisLine;
            }
            // Split lines (grid)
            if (ax.splitLine) {
                if (!ax.splitLine.lineStyle) ax.splitLine.lineStyle = {};
                ax.splitLine.lineStyle.color = palette.splitLine;
            }
            // Axis tick
            if (ax.axisTick) {
                if (!ax.axisTick.lineStyle) ax.axisTick.lineStyle = {};
                ax.axisTick.lineStyle.color = palette.axisLine;
            }
        }
    }

    // ── API principal ─────────────────────────────────────────────

    function initOrUpdate(elementId, optionsJson) {
        try {
            var container = document.getElementById(elementId);
            if (!container) return;

            if (_instances[elementId]) {
                _instances[elementId].dispose();
                delete _instances[elementId];
            }

            var chart = echarts.init(container, null, { renderer: "canvas" });
            var options = JSON.parse(optionsJson);
            var palette = getThemePalette();
            patchOptions(options, palette);
            chart.setOption(options, true);
            _instances[elementId] = chart;
        } catch (e) {
            console.error("[echartsInterop.initOrUpdate]", e);
        }
    }

    function update(elementId, optionsJson) {
        try {
            var chart = _instances[elementId];
            if (!chart) {
                initOrUpdate(elementId, optionsJson);
                return;
            }
            var options = JSON.parse(optionsJson);
            var palette = getThemePalette();
            patchOptions(options, palette);
            chart.setOption(options, true);
        } catch (e) {
            console.error("[echartsInterop.update]", e);
        }
    }

    function dispose(elementId) {
        try {
            var chart = _instances[elementId];
            if (chart) {
                chart.dispose();
                delete _instances[elementId];
            }
        } catch (e) { }
    }

    function resize(elementId) {
        try {
            var chart = _instances[elementId];
            if (chart) chart.resize();
        } catch (e) { }
    }

    function resizeAll() {
        try {
            for (var id in _instances) {
                if (_instances.hasOwnProperty(id) && _instances[id]) {
                    _instances[id].resize();
                }
            }
        } catch (e) { }
    }

    window.addEventListener("resize", function () {
        resizeAll();
    });

    return {
        initOrUpdate: initOrUpdate,
        update: update,
        dispose: dispose,
        resize: resize,
        resizeAll: resizeAll
    };
})();
