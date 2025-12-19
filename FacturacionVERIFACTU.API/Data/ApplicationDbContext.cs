using API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FacturacionVERIFACTU.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets se añadirán en el Bloque 2
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<SerieNumeracion> SeriesNumeraciones => Set<SerieNumeracion>();
        public DbSet<CierreEjercicio> CierreEjercicios => Set<CierreEjercicio>();
        public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
        public DbSet<LineaPresupuesto> LineasPresupuesto => Set<LineaPresupuesto>();
        public DbSet<Albaran> Albaranes => Set<Albaran>();
        public DbSet<LineaAlbaran> LineasAlbaranes => Set<LineaAlbaran>();
        public DbSet<Factura> Facturas => Set<Factura>();
        public DbSet<LineaFactura> LineasFacturas => Set<LineaFactura>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<SerieNumeracion> SeriesNumeracion => Set<SerieNumeracion>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tenant>()
               .HasIndex(t => t.NIF)
               .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => new { u.TenantId, u.Email })
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => new { c.TenantId, c.NIF })
                .IsUnique();

            modelBuilder.Entity<Producto>()
                .HasIndex(p => new { p.TenantId, p.Codigo })
                .IsUnique();

            modelBuilder.Entity<SerieNumeracion>()
                .HasIndex(s => new { s.TenantId, s.Codigo, s.Ejercicio })
                .IsUnique();

            modelBuilder.Entity<Presupuesto>()
                .HasIndex(p => new { p.TenantId, p.Numero })
                .IsUnique();

            modelBuilder.Entity<Albaran>()
                .HasIndex(a => new { a.TenantId, a.Numero })
                .IsUnique();

            modelBuilder.Entity<Factura>()
                .HasIndex(f => new { f.TenantId, f.Numero })
                .IsUnique();

            // ==========================================
            // RELACIONES Y DELETE BEHAVIOR
            // ==========================================

            // Tenant -> Usuarios
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Tenant)
                .WithMany(t => t.Usuarios)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tenant -> Clientes
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Tenant)
                .WithMany(t => t.Clientes)
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tenant -> Productos
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Tenant)
                .WithMany(t => t.Productos)
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tenant -> Series
            modelBuilder.Entity<SerieNumeracion>()
                .HasOne(s => s.Tenant)
                .WithMany(t => t.Series)
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tenant -> Cierres
            modelBuilder.Entity<CierreEjercicio>()
                .HasOne(c => c.Tenant)
                .WithMany()
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cliente -> Presupuestos
            modelBuilder.Entity<Presupuesto>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Presupuestos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cliente -> Albaranes
            modelBuilder.Entity<Albaran>()
                .HasOne(a => a.Cliente)
                .WithMany(c => c.Albaranes)
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cliente -> Facturas
            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Cliente)
                .WithMany(c => c.Facturas)
                .HasForeignKey(f => f.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Presupuesto -> Albaranes (opcional)
            modelBuilder.Entity<Albaran>()
                .HasOne(a => a.Presupuesto)
                .WithMany(p => p.Albaranes)
                .HasForeignKey(a => a.PresupuestoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Factura -> Albaranes (opcional)
            modelBuilder.Entity<Albaran>()
                .HasOne(a => a.Factura)
                .WithMany(f => f.Albaranes)
                .HasForeignKey(a => a.FacturaId)
                .OnDelete(DeleteBehavior.SetNull);

            // Presupuesto -> LineasPresupuesto
            modelBuilder.Entity<LineaPresupuesto>()
                .HasOne(l => l.Presupuesto)
                .WithMany(p => p.Lineas)
                .HasForeignKey(l => l.PresupuestoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Albaran -> LineasAlbaran
            modelBuilder.Entity<LineaAlbaran>()
                .HasOne(l => l.Albaran)
                .WithMany(a => a.Lineas)
                .HasForeignKey(l => l.AlbaranId)
                .OnDelete(DeleteBehavior.Cascade);

            // Factura -> LineasFactura
            modelBuilder.Entity<LineaFactura>()
                .HasOne(l => l.Factura)
                .WithMany(f => f.Lineas)
                .HasForeignKey(l => l.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Producto -> LineasPresupuesto
            modelBuilder.Entity<LineaPresupuesto>()
                .HasOne(l => l.Producto)
                .WithMany(p => p.LineasPresupuesto)
                .HasForeignKey(l => l.ProductoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Producto -> LineasAlbaran
            modelBuilder.Entity<LineaAlbaran>()
                .HasOne(l => l.Producto)
                .WithMany(p => p.LineasAlbaran)
                .HasForeignKey(l => l.ProductoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Producto -> LineasFactura
            modelBuilder.Entity<LineaFactura>()
                .HasOne(l => l.Producto)
                .WithMany(p => p.LineasFactura)
                .HasForeignKey(l => l.ProductoId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.HasKey(rt => rt.Id);
                entity.HasIndex(rt => rt.Token);
                entity.Property(rt => rt.Token).IsRequired();

                entity.HasOne(rt => rt.Usuario)
                    .WithMany()
                    .HasForeignKey(rt => rt.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // QUERY FILTERS POR TENANT
            // (Se activarán con el sistema de autenticación)
            // ==========================================
            /*
            modelBuilder.Entity<Usuario>().HasQueryFilter(u => u.TenantId == CurrentTenantId);
            modelBuilder.Entity<Cliente>().HasQueryFilter(c => c.TenantId == CurrentTenantId);
            modelBuilder.Entity<Producto>().HasQueryFilter(p => p.TenantId == CurrentTenantId);
            modelBuilder.Entity<Presupuesto>().HasQueryFilter(p => p.TenantId == CurrentTenantId);
            modelBuilder.Entity<Albaran>().HasQueryFilter(a => a.TenantId == CurrentTenantId);
            modelBuilder.Entity<Factura>().HasQueryFilter(f => f.TenantId == CurrentTenantId);
            */
        }
    }
}
