using Microsoft.EntityFrameworkCore;

namespace TP_Stationnement.Model;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicule> Vehicules => Set<Vehicule>();
    public DbSet<SessionStationnement> Sessions => Set<SessionStationnement>();
    public DbSet<AuditModification> Audits => Set<AuditModification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Même si quelqu'un effaçait un véhicule dans phpMyAdmin,
        // MySQL refuserait tant qu'il a des sessions. L'API, de toute façon,
        // n'offre aucune suppression.
        modelBuilder.Entity<SessionStationnement>()
            .HasOne(s => s.Vehicule)
            .WithMany()
            .HasForeignKey(s => s.VehiculeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AuditModification>()
            .HasOne<SessionStationnement>()
            .WithMany()
            .HasForeignKey(a => a.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SessionStationnement>()
            .Property(s => s.EstTerminee)
            .HasDefaultValue(false);
    }
}
