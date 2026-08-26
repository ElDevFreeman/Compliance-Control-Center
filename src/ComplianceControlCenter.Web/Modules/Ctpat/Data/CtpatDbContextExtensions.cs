using Microsoft.EntityFrameworkCore;
using ComplianceControlCenter.Web.Modules.Ctpat.Domain.Entities;

namespace ComplianceControlCenter.Web.Modules.Ctpat.Data;

/// <summary>
/// Extension methods para registrar las entidades del módulo CTPAT en el <c>AppDbContext</c>.
///
/// Mantiene el Core desacoplado del módulo: el DbContext principal invoca
/// <see cref="AddCtpatModule(ModelBuilder)"/> desde <c>OnModelCreating</c>, pero
/// no importa nada del namespace <c>Modules.Ctpat</c> directamente en su firma.
/// </summary>
public static class CtpatDbContextExtensions
{
    public static ModelBuilder AddCtpatModule(this ModelBuilder builder)
    {
        // ────────────────────────────────────────────────────────────────
        // CCC_CTPAT_Questions (catálogo de 139 preguntas)
        // ────────────────────────────────────────────────────────────────
        builder.Entity<CtpatQuestion>(b =>
        {
            b.ToTable("CCC_CTPAT_Questions");
            b.HasKey(x => x.Id);
            b.Property(x => x.ExternalId).HasMaxLength(32).IsRequired();
            b.Property(x => x.Criterio).HasMaxLength(128).IsRequired();
            b.Property(x => x.Pregunta).IsRequired();
            b.Property(x => x.Respuesta2025);

            b.HasIndex(x => x.ExternalId).IsUnique();
            b.HasIndex(x => x.Criterio);
            b.HasIndex(x => x.SortOrder);
        });

        // ────────────────────────────────────────────────────────────────
        // CCC_CTPAT_Guidance (guía de revisión por criterio)
        // ────────────────────────────────────────────────────────────────
        builder.Entity<CtpatGuidance>(b =>
        {
            b.ToTable("CCC_CTPAT_Guidance");
            b.HasKey(x => x.Id);
            b.Property(x => x.GroupName).HasMaxLength(128).IsRequired();
            b.Property(x => x.Criterio).HasMaxLength(256).IsRequired();

            b.HasIndex(x => x.GroupName);
            b.HasIndex(x => x.Criterio);
        });

        // ────────────────────────────────────────────────────────────────
        // CCC_CTPAT_Reviews (una fila por pregunta por año)
        // ────────────────────────────────────────────────────────────────
        builder.Entity<CtpatReview>(b =>
        {
            b.ToTable("CCC_CTPAT_Reviews");
            b.HasKey(x => x.Id);
            b.Property(x => x.Revisor).HasMaxLength(128);
            b.Property(x => x.UpdatedBy).HasMaxLength(128);

            b.HasIndex(x => new { x.QuestionId, x.Year }).IsUnique();
            b.HasIndex(x => x.Status);

            b.HasOne(x => x.Question)
                .WithMany(q => q.Reviews)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ────────────────────────────────────────────────────────────────
        // CCC_CTPAT_ReviewFiles (adjuntos, metadata)
        // ────────────────────────────────────────────────────────────────
        builder.Entity<CtpatReviewFile>(b =>
        {
            b.ToTable("CCC_CTPAT_ReviewFiles");
            b.HasKey(x => x.Id);
            b.Property(x => x.ExternalId).HasMaxLength(32).IsRequired();
            b.Property(x => x.FileName).HasMaxLength(256).IsRequired();
            b.Property(x => x.ContentType).HasMaxLength(128);
            b.Property(x => x.RelativePath).HasMaxLength(512).IsRequired();
            b.Property(x => x.UploadedBy).HasMaxLength(128);

            b.HasIndex(x => x.ExternalId).IsUnique();
            b.HasIndex(x => x.ReviewId);

            b.HasOne(x => x.Review)
                .WithMany(r => r.Files)
                .HasForeignKey(x => x.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        return builder;
    }
}
