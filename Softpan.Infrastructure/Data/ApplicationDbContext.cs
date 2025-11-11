

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Softpan.Domain.Entities;

namespace Softpan.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<DetalleVenta> DetalleVentas { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<PagoVenta> PagoVentas { get; set; }

    public DbSet<PrecioCliente> PrecioClientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(60);
            entity.Property(c => c.Telefono).HasMaxLength(20);
            entity.Property(c => c.Direccion).HasMaxLength(50);
            entity.Property(c => c.TipoCliente).HasConversion<int>();
            entity.Property(c => c.Activo).HasDefaultValue(true);
            entity.HasMany(c => c.Ventas).WithOne(v => v.Cliente).HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(c => c.PreciosClientes).WithOne(v => v.Cliente).HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Cascade);

        });
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nombre).IsRequired().HasMaxLength(60);
            entity.Property(p => p.Descripcion).HasMaxLength(100);
            entity.Property(p => p.Activo).HasDefaultValue(true);
            entity.Property(p => p.PrecioBase).HasColumnType("decimal(18,2)");
            entity.HasMany(p => p.DetallesVenta).WithOne(dv => dv.Producto).HasForeignKey(dv => dv.ProductoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(p => p.PreciosPersonalizados).WithOne(v => v.Producto).HasForeignKey(v => v.ProductoId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.MontoTotal).HasColumnType("decimal(18,2)");
            entity.Property(v => v.MontoPagado).HasColumnType("decimal(18,2)");
            entity.Property(v => v.Estado).HasConversion<int>();
            entity.HasMany(v => v.DetallesVenta).WithOne(dv => dv.Venta).HasForeignKey(dv => dv.VentaId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(v => v.PagosVenta).WithOne(dv => dv.Venta).HasForeignKey(dv => dv.VentaId).OnDelete(DeleteBehavior.Cascade);

        });
        modelBuilder.Entity<PrecioCliente>(entity =>
        {
            entity.HasKey(pc => pc.Id);

            entity.Property(pc => pc.Precio)
                .HasColumnType("decimal(18,2)");
            entity.HasIndex(pc => new { pc.ClienteId, pc.ProductoId })
                .IsUnique();
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Monto).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Observaciones).HasMaxLength(500);
            entity.Property(p => p.TipoPago).HasConversion<int>();
            entity.HasOne(p => p.Cliente).WithMany().HasForeignKey(p => p.ClienteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(p => p.PagosAplicado).WithOne(pv => pv.Pago).HasForeignKey(pv => pv.PagoId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(dv => dv.Id);

            entity.Property(dv => dv.PrecioUnitario)
                .HasColumnType("decimal(18,2)");

            entity.Ignore(dv => dv.Subtotal);  // No se guarda en BD
        });
        modelBuilder.Entity<PagoVenta>(entity =>
        {
            entity.HasKey(pv => pv.Id);

            entity.Property(pv => pv.MontoAplicado)
                .HasColumnType("decimal(18,2)");

            entity.HasIndex(pv => new { pv.PagoId, pv.VentaId });
        });
    }

}
