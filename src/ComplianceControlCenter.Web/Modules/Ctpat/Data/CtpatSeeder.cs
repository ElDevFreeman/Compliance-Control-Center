using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ComplianceControlCenter.Web.Core.Data;
using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;

namespace ComplianceControlCenter.Web.Modules.Ctpat.Data;

/// <summary>
/// Seeder que carga el catálogo CTPAT (139 preguntas + bloques de guía) desde
/// <c>Modules/Ctpat/Data/ctpat_data.json</c> hacia las tablas
/// <c>CCC_CTPAT_Questions</c> y <c>CCC_CTPAT_Guidance</c>.
///
/// Se ejecuta idempotente: sólo agrega registros que no existan ya
/// (por <c>ExternalId</c> en questions y por <c>(GroupName, Criterio)</c> en guidance).
/// </summary>
public static class CtpatSeeder
{
    public static async Task SeedAsync(AppDbContext db, IWebHostEnvironment env, ILogger logger, CancellationToken ct = default)
    {
        // El JSON del catálogo vive en el proyecto en Modules/Ctpat/Data/ctpat_data.json
        // ContentRootPath apunta al directorio del proyecto en dev y al app en prod.
        var jsonPath = Path.Combine(env.ContentRootPath, "Modules", "Ctpat", "Data", "ctpat_data.json");
        if (!File.Exists(jsonPath))
        {
            logger.LogWarning("CTPAT seed skipped: {Path} not found", jsonPath);
            return;
        }

        await using var stream = File.OpenRead(jsonPath);
        var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var root = doc.RootElement;

        // ── Questions ────────────────────────────────────────────────
        if (root.TryGetProperty("questions", out var questionsEl) && questionsEl.ValueKind == JsonValueKind.Array)
        {
            var existingIds = await db.CtpatQuestions
                .Select(q => q.ExternalId)
                .ToListAsync(ct);
            var existingSet = new HashSet<string>(existingIds, StringComparer.Ordinal);

            var toAdd = new List<CtpatQuestion>();
            var order = 0;
            foreach (var item in questionsEl.EnumerateArray())
            {
                order += 10;
                var externalId = item.GetProperty("id").GetString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(externalId) || existingSet.Contains(externalId)) continue;

                toAdd.Add(new CtpatQuestion
                {
                    ExternalId    = externalId,
                    Criterio      = item.TryGetProperty("criterio", out var c) ? (c.GetString() ?? "") : "",
                    Pregunta      = item.TryGetProperty("pregunta", out var p) ? (p.GetString() ?? "") : "",
                    Respuesta2025 = item.TryGetProperty("respuesta2025", out var r) ? (r.GetString() ?? "") : "",
                    SortOrder     = order
                });
            }

            if (toAdd.Count > 0)
            {
                db.CtpatQuestions.AddRange(toAdd);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("CTPAT seed: added {Count} questions", toAdd.Count);
            }
        }

        // ── Guidance ─────────────────────────────────────────────────
        if (root.TryGetProperty("guidance", out var guidanceEl) && guidanceEl.ValueKind == JsonValueKind.Object)
        {
            var existingGuidance = await db.CtpatGuidance
                .Select(g => new { g.GroupName, g.Criterio })
                .ToListAsync(ct);
            var existingKeys = new HashSet<string>(
                existingGuidance.Select(g => $"{g.GroupName}|{g.Criterio}"),
                StringComparer.Ordinal);

            var toAdd = new List<CtpatGuidance>();
            foreach (var group in guidanceEl.EnumerateObject())
            {
                var groupName = group.Name;
                if (group.Value.ValueKind != JsonValueKind.Array) continue;
                foreach (var item in group.Value.EnumerateArray())
                {
                    var crit = item.TryGetProperty("criterio", out var c) ? (c.GetString() ?? "") : "";
                    if (string.IsNullOrWhiteSpace(crit)) continue;
                    var key = $"{groupName}|{crit}";
                    if (existingKeys.Contains(key)) continue;

                    toAdd.Add(new CtpatGuidance
                    {
                        GroupName = groupName,
                        Criterio  = crit,
                        RespTip   = item.TryGetProperty("respTip",   out var rt) ? rt.GetString() : null,
                        Revisar   = item.TryGetProperty("revisar",   out var re) ? re.GetString() : null,
                        Evidencia = item.TryGetProperty("evidencia", out var ev) ? ev.GetString() : null,
                    });
                    existingKeys.Add(key);
                }
            }

            if (toAdd.Count > 0)
            {
                db.CtpatGuidance.AddRange(toAdd);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("CTPAT seed: added {Count} guidance items", toAdd.Count);
            }
        }
    }
}
