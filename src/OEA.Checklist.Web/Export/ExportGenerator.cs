using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using OEA.Checklist.Web.Domain.Entities;
using OEA.Checklist.Web.Domain.Enums;
using OEA.Checklist.Web.Services;

namespace OEA.Checklist.Web.Export;

/// <summary>
/// Genera archivos CSV y XLSX del checklist mensual y de la matriz.
/// </summary>
public static class ExportGenerator
{
    // ── Checklist mensual ────────────────────────────────────────────

    public static byte[] ChecklistToCsv(IEnumerable<Activity> activities, int year, int month)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Item,Fundamento,Actividad,Descripción,Documentos,Responsable,Relacionados,Frecuencia,Estado,Vencimiento,Acción correctiva");
        foreach (var a in activities)
        {
            var st = a.MonthlyStatuses.FirstOrDefault();
            sb.AppendLine(string.Join(",", new[]
            {
                Csv(a.Item), Csv(a.Legal), Csv(a.Name), Csv(a.Description),
                Csv(a.Documents), Csv(a.Owner), Csv(a.Related), Csv(a.Frequency),
                Csv(LabelFor(st?.Status ?? ComplianceStatus.Pendiente)),
                Csv(st?.DueDate?.ToString("yyyy-MM-dd") ?? ""),
                Csv(st?.CorrectiveActions ?? "")
            }));
        }
        // UTF-8 BOM para que Excel abra bien los acentos
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public static byte[] ChecklistToXlsx(IEnumerable<Activity> activities, int year, int month)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add($"Checklist {year:0000}-{month:00}");

        var headers = new[]
        {
            "Item","Fundamento","Actividad","Descripción","Documentos",
            "Responsable","Relacionados","Frecuencia","Estado","Vencimiento","Acción correctiva"
        };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
        }
        ws.Range(1, 1, 1, headers.Length).Style
            .Fill.SetBackgroundColor(XLColor.FromHtml("#1e3a8a"))
            .Font.SetFontColor(XLColor.White)
            .Font.SetBold();

        int row = 2;
        foreach (var a in activities)
        {
            var st = a.MonthlyStatuses.FirstOrDefault();
            ws.Cell(row, 1).Value = a.Item;
            ws.Cell(row, 2).Value = a.Legal;
            ws.Cell(row, 3).Value = a.Name;
            ws.Cell(row, 4).Value = a.Description;
            ws.Cell(row, 5).Value = a.Documents;
            ws.Cell(row, 6).Value = a.Owner;
            ws.Cell(row, 7).Value = a.Related;
            ws.Cell(row, 8).Value = a.Frequency;
            ws.Cell(row, 9).Value = LabelFor(st?.Status ?? ComplianceStatus.Pendiente);
            ws.Cell(row, 10).Value = st?.DueDate?.ToDateTime(TimeOnly.MinValue);
            ws.Cell(row, 10).Style.DateFormat.Format = "yyyy-mm-dd";
            ws.Cell(row, 11).Value = st?.CorrectiveActions ?? "";

            // Color de fondo según estado
            var color = ColorForStatus(st?.Status ?? ComplianceStatus.Pendiente);
            if (color is not null)
                ws.Cell(row, 9).Style.Fill.BackgroundColor = color;

            row++;
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Matriz mensual ───────────────────────────────────────────────

    public static byte[] MatrixToXlsx(
        IEnumerable<MatrixRow> rows,
        IEnumerable<(int Year, int Month)> months)
    {
        var monthsList = months.ToList();
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Matriz");

        ws.Cell(1, 1).Value = "Item";
        ws.Cell(1, 2).Value = "Actividad";
        ws.Cell(1, 3).Value = "Responsable";
        for (int i = 0; i < monthsList.Count; i++)
        {
            ws.Cell(1, 4 + i).Value = $"{monthsList[i].Year:0000}-{monthsList[i].Month:00}";
        }
        ws.Cell(1, 4 + monthsList.Count).Value = "% Cumplimiento";

        var headerCount = 3 + monthsList.Count + 1;
        ws.Range(1, 1, 1, headerCount).Style
            .Fill.SetBackgroundColor(XLColor.FromHtml("#1e3a8a"))
            .Font.SetFontColor(XLColor.White)
            .Font.SetBold();

        int r = 2;
        foreach (var row in rows)
        {
            ws.Cell(r, 1).Value = row.Item;
            ws.Cell(r, 2).Value = row.Name;
            ws.Cell(r, 3).Value = row.Owner;
            for (int i = 0; i < monthsList.Count; i++)
            {
                var s = row.Statuses.TryGetValue(monthsList[i], out var st) ? st : null;
                var cell = ws.Cell(r, 4 + i);
                cell.Value = s is null ? "" : LabelFor(s.Value);
                var color = s is null ? null : ColorForStatus(s.Value);
                if (color is not null)
                    cell.Style.Fill.BackgroundColor = color;
            }
            ws.Cell(r, 4 + monthsList.Count).Value = Math.Round(row.CompliancePct, 1) / 100.0;
            ws.Cell(r, 4 + monthsList.Count).Style.NumberFormat.Format = "0.0%";
            r++;
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);
        ws.SheetView.FreezeColumns(3);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        var needsQuoting = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        var escaped = value.Replace("\"", "\"\"");
        return needsQuoting ? $"\"{escaped}\"" : escaped;
    }

    private static string LabelFor(ComplianceStatus s) => s switch
    {
        ComplianceStatus.EnProgreso => "En progreso",
        ComplianceStatus.NA => "N/A",
        _ => s.ToString()
    };

    private static XLColor? ColorForStatus(ComplianceStatus s) => s switch
    {
        ComplianceStatus.Completado => XLColor.FromHtml("#d1fae5"),
        ComplianceStatus.EnProgreso => XLColor.FromHtml("#dbeafe"),
        ComplianceStatus.Pendiente => XLColor.FromHtml("#fef3c7"),
        ComplianceStatus.Vencido => XLColor.FromHtml("#fee2e2"),
        ComplianceStatus.NA => XLColor.FromHtml("#f3f4f6"),
        _ => null
    };
}
