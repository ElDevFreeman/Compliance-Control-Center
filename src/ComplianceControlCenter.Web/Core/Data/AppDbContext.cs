using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ComplianceControlCenter.Web.Core.Domain.Entities;
using ComplianceControlCenter.Web.Modules.Oea.Domain.Entities;

namespace ComplianceControlCenter.Web.Core.Data;

/// <summary>
/// DbContext principal de la aplicación.
///
/// Convención de nombres de tablas:
///   * `CCC_*`      — tablas del núcleo compartido (Identity, auditoría, adjuntos)
///   * `CCC_OEA_*`  — tablas específicas del módulo OEA
///   * `CCC_CTPAT_*`— tablas específicas del módulo CTPAT
///
/// Todas las tablas viven en la BD compartida `FDS_DEV_RYR9` y no chocan
/// con otras aplicaciones gracias al prefijo `CCC_`.
///
/// Tablas de negocio OEA:
///   CCC_OEA_Activities, CCC_OEA_MonthlyStatus, CCC_OEA_Comments
///
/// Tablas del núcleo:
///   CCC_Users, CCC_Roles, CCC_UserRoles, CCC_UserClaims,
///   CCC_UserLogins, CCC_UserTokens, CCC_RoleClaims, CCC_AuditLog
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<MonthlyStatus> MonthlyStatuses => Set<MonthlyStatus>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ────────────────────────────────────────────────────────────────
        // Núcleo compartido: Identity (prefijo CCC_)
        // ────────────────────────────────────────────────────────────────
        builder.Entity<ApplicationUser>(b => b.ToTable("CCC_Users"));
        builder.Entity<IdentityRole>(b => b.ToTable("CCC_Roles"));
        builder.Entity<IdentityUserRole<string>>(b => b.ToTable("CCC_UserRoles"));
        builder.Entity<IdentityUserClaim<string>>(b => b.ToTable("CCC_UserClaims"));
        builder.Entity<IdentityUserLogin<string>>(b => b.ToTable("CCC_UserLogins"));
        builder.Entity<IdentityUserToken<string>>(b => b.ToTable("CCC_UserTokens"));
        builder.Entity<IdentityRoleClaim<string>>(b => b.ToTable("CCC_RoleClaims"));

        // ────────────────────────────────────────────────────────────────
        // Módulo OEA: CCC_OEA_Activities
        // ────────────────────────────────────────────────────────────────
        builder.Entity<Activity>(b =>
        {
            b.ToTable("CCC_OEA_Activities");
            b.HasKey(x => x.Id);
            b.Property(x => x.Item).HasMaxLength(64).IsRequired();
            b.Property(x => x.Legal).HasMaxLength(256);
            b.Property(x => x.Name).HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(1024);
            b.Property(x => x.Documents).HasMaxLength(512);
            b.Property(x => x.Owner).HasMaxLength(128);
            b.Property(x => x.Related).HasMaxLength(256);
            b.Property(x => x.Frequency).HasMaxLength(64);
            b.Property(x => x.CreatedBy).HasMaxLength(128);
            b.Property(x => x.UpdatedBy).HasMaxLength(128);

            b.HasIndex(x => x.Item);
            b.HasIndex(x => x.IsActive);
        });

        // ────────────────────────────────────────────────────────────────
        // Módulo OEA: CCC_OEA_MonthlyStatus
        // ────────────────────────────────────────────────────────────────
        builder.Entity<MonthlyStatus>(b =>
        {
            b.ToTable("CCC_OEA_MonthlyStatus");
            b.HasKey(x => x.Id);
            b.Property(x => x.CorrectiveActions).HasMaxLength(1024);
            b.Property(x => x.UpdatedBy).HasMaxLength(128);

            b.HasIndex(x => new { x.ActivityId, x.Year, x.Month }).IsUnique();
            b.HasIndex(x => new { x.Year, x.Month });

            b.HasOne(x => x.Activity)
                .WithMany(a => a.MonthlyStatuses)
                .HasForeignKey(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ────────────────────────────────────────────────────────────────
        // Módulo OEA: CCC_OEA_Comments
        // ────────────────────────────────────────────────────────────────
        builder.Entity<Comment>(b =>
        {
            b.ToTable("CCC_OEA_Comments");
            b.HasKey(x => x.Id);
            b.Property(x => x.Author).HasMaxLength(128).IsRequired();
            b.Property(x => x.AuthorUserId).HasMaxLength(450);
            b.Property(x => x.Text).HasMaxLength(2048).IsRequired();

            b.HasIndex(x => x.ActivityId);
            b.HasIndex(x => x.CreatedAt);

            b.HasOne(x => x.Activity)
                .WithMany(a => a.Comments)
                .HasForeignKey(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ────────────────────────────────────────────────────────────────
        // Núcleo compartido: CCC_AuditLog (usado por todos los módulos)
        // ────────────────────────────────────────────────────────────────
        builder.Entity<AuditLog>(b =>
        {
            b.ToTable("CCC_AuditLog");
            b.HasKey(x => x.Id);
            b.Property(x => x.User).HasMaxLength(128);
            b.Property(x => x.Action).HasMaxLength(32);
            b.Property(x => x.EntityName).HasMaxLength(128);
            b.Property(x => x.EntityId).HasMaxLength(64);
            // Changes es nvarchar(max) por default

            b.HasIndex(x => x.Timestamp);
            b.HasIndex(x => x.EntityName);
        });
    }
}
