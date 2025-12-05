using Microsoft.EntityFrameworkCore;
using FacturacionVERIFACTU.API.Models;

namespace FacturacionVERIFACTU.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<LineaFactura> LineasFactura { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar índices
            modelBuilder.Entity<Empresa>()
                .HasIndex(e => e.Nif)
                .IsUnique();

            modelBuilder.Entity<Factura>()
                .HasIndex(f => new { f.EmpresaId, f.Serie, f.NumeroFactura })
                .IsUnique();

            modelBuilder.Entity<Factura>()
                .HasIndex(f => f.FechaExpedicion);

            modelBuilder.Entity<Factura>()
                .HasIndex(f => f.EstadoVerifactu);

            modelBuilder.Entity<LineaFactura>()
                .HasIndex(l => l.FacturaId);

            // Configurar relaciones con DELETE CASCADE
            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Empresa)
                .WithMany(e => e.Facturas)
                .HasForeignKey(f => f.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LineaFactura>()
                .HasOne(l => l.Factura)
                .WithMany(f => f.Lineas)
                .HasForeignKey(l => l.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}