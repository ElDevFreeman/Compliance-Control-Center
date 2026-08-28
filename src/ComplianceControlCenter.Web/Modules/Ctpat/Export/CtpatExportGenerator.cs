using System.Globalization;
using System.Text;
using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;
using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Enums;

namespace ComplianceControlCenter.Web.Modules.Ctpat.Export;

/// <summary>
/// Genera exportaciones (CSV) de la matriz CTPAT.
///
/// El formato refleja las mismas columnas que muestra <c>CtpatMatrix.razor</c>
/// más los campos editables de la review (evidencia, cambios detectados,
/// respuesta nueva, revisor, fecha, comentarios).
///
/// Convención de encoding: UTF-8 con BOM para que Excel abra acentos y ñ
/// correctamente al hacer doble clic en el CSV.
/// </summary>
public static class CtpatExportGenerator
{
    // Encabezados en el mismo orden que las columnas del CSV.
    private static readonly string[] Headers =
    {
        "ID",
        "Criterio",
        "Pregunta",
        "Respuesta 2025",
        "Estado",
        "Evidencia revisada",
        "Cambios detectados",
        "Respuesta nueva",
        "Revisor",
        "Fecha revisión",
        "Comentarios",
        "Adjuntos"
    };

    /// <summary>
    /// Genera un CSV con todas las preguntas (opcionalmente filtradas por criterio)
    /// para el año indicado, incluyendo el estado de review si existe.
    /// </summary>
    /// <param name="questions">Catálogo de preguntas (activas) a exportar.</param>
    /// <param name="reviewsByQuestionId">Mapa de reviews del año. Si una pregunta no tiene review, se exporta como "Pendiente".</param>
    /// <param name="year">Año del ciclo.</param>
    /// <param name="criterioFilter">Filtro opcional por criterio (case-sensitive, mismo valor que el combo).</param>
    public static byte[] MatrixToCsv(
        IEnumerable<CtpatQuestion> questions,
        IReadOnlyDictionary<int, CtpatReview> reviewsByQuestionId,
        int year,
        string? criterioFilter = null)
    {
        var filtered = questions
            .Where(q => q.IsActive)
            .Where(q => string.IsNullOrEmpty(criterioFilter) || q.Criterio == criterioFilter)
            .OrderBy(q => q.Criterio, StringComparer.OrdinalIgnoreCase)
            .ThenBy(q => q.SortOrder)
            .ThenBy(q => q.Id);

        var sb = new StringBuilder();

        // Metadatos informativos (comentario) — útil para trazabilidad al abrir el archivo.
        sb.Append("# Matriz CTPAT · Ciclo ").Append(year);
        if (!string.IsNullOrWhiteSpace(criterioFilter))
        {
            sb.Append(" · Criterio: ").Append(criterioFilter);
        }
        sb.Append(" · Generado ").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
        sb.AppendLine();

        // Header row.
        sb.AppendLine(string.Join(",", Headers.Select(Csv)));

        foreach (var q in filtered)
        {
            reviewsByQuestionId.TryGetValue(q.Id, out var r);
            var status = r?.Status ?? CtpatReviewStatus.Pendiente;
            var fecha = r?.FechaRevision?.ToString("yyyy-MM-dd") ?? "";
            var adjuntos = r?.Files is null ? 0 : r.Files.Count;

            sb.AppendLine(string.Join(",", new[]
            {
                Csv(q.ExternalId),
                Csv(q.Criterio),
                Csv(q.Pregunta),
                Csv(q.Respuesta2025),
                Csv(LabelFor(status)),
                Csv(r?.EvidenciaRevisada),
                Csv(r?.CambiosDetectados),
                Csv(r?.RespuestaNueva),
                Csv(r?.Revisor),
                Csv(fecha),
                Csv(r?.Comentarios),
                Csv(adjuntos.ToString(CultureInfo.InvariantCulture))
            }));
        }

        // UTF-8 BOM para que Excel abra los acentos correctamente.
        return Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
            .ToArray();
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        var needsQuoting = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        var escaped = value.Replace("\"", "\"\"");
        return needsQuoting ? $"\"{escaped}\"" : escaped;
    }

    private static string LabelFor(CtpatReviewStatus s) => s switch
    {
        CtpatReviewStatus.Pendiente  => "Pendiente",
        CtpatReviewStatus.SinCambios => "Sin cambios",
        CtpatReviewStatus.ConCambios => "Con cambios",
        CtpatReviewStatus.Revisado   => "Revisado",
        _                            => s.ToString()
    };
}
