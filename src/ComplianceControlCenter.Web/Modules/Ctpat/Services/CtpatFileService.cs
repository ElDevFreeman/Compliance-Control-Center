using Microsoft.EntityFrameworkCore;
using ComplianceControlCenter.Web.Core.Data;
using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;

namespace ComplianceControlCenter.Web.Modules.Ctpat.Services;

public class CtpatFileService : ICtpatFileService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<CtpatFileService> _logger;

    // Máximo tamaño de archivo permitido: 25 MB (igual que el server.py original).
    public const long MaxFileSize = 25L * 1024 * 1024;

    // Ruta relativa base bajo wwwroot para adjuntos CTPAT.
    private const string UploadsSubfolder = "uploads/ctpat";

    public CtpatFileService(AppDbContext db, IWebHostEnvironment env, ILogger<CtpatFileService> logger)
    {
        _db = db;
        _env = env;
        _logger = logger;
    }

    public async Task<CtpatReviewFile> UploadAsync(int reviewId, string fileName, string? contentType, Stream content, string uploadedBy, CancellationToken ct = default)
    {
        var review = await _db.CtpatReviews.FirstOrDefaultAsync(r => r.Id == reviewId, ct)
            ?? throw new InvalidOperationException($"Review {reviewId} not found");

        // Sanitizar nombre y crear directorio destino
        var safeName = SanitizeFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName)) safeName = "archivo";

        var externalId = "f_" + Guid.NewGuid().ToString("N").Substring(0, 16);
        var reviewDir = Path.Combine(UploadsSubfolder, reviewId.ToString());
        var relativeFile = Path.Combine(reviewDir, $"{externalId}__{safeName}").Replace('\\', '/');

        var absoluteDir = Path.Combine(_env.WebRootPath, reviewDir);
        Directory.CreateDirectory(absoluteDir);
        var absoluteFile = Path.Combine(_env.WebRootPath, relativeFile);

        long size;
        await using (var fs = File.Create(absoluteFile))
        {
            await content.CopyToAsync(fs, ct);
            size = fs.Length;
        }

        if (size > MaxFileSize)
        {
            File.Delete(absoluteFile);
            throw new InvalidOperationException($"El archivo excede {MaxFileSize / 1024 / 1024} MB");
        }

        var entity = new CtpatReviewFile
        {
            ReviewId     = reviewId,
            ExternalId   = externalId,
            FileName     = fileName,
            Size         = size,
            ContentType  = contentType,
            RelativePath = relativeFile,
            UploadedAt   = DateTime.UtcNow,
            UploadedBy   = uploadedBy
        };
        _db.CtpatReviewFiles.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteAsync(int fileId, CancellationToken ct = default)
    {
        var f = await _db.CtpatReviewFiles.FirstOrDefaultAsync(x => x.Id == fileId, ct);
        if (f is null) return;

        var abs = Path.Combine(_env.WebRootPath, f.RelativePath);
        try
        {
            if (File.Exists(abs)) File.Delete(abs);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete physical file {Path}", abs);
        }

        _db.CtpatReviewFiles.Remove(f);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<(string AbsolutePath, string FileName, string? ContentType)?> GetPhysicalPathAsync(string externalId, CancellationToken ct = default)
    {
        var f = await _db.CtpatReviewFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExternalId == externalId, ct);
        if (f is null) return null;
        var abs = Path.Combine(_env.WebRootPath, f.RelativePath);
        if (!File.Exists(abs)) return null;
        return (abs, f.FileName, f.ContentType);
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.Where(c => !invalid.Contains(c) && c != '<' && c != '>' && c != ':' && c != '"' && c != '/' && c != '\\' && c != '|' && c != '?' && c != '*').ToArray();
        return new string(chars).Trim();
    }
}
