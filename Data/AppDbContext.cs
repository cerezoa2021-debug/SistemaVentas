
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Models;

namespace SistemaVentas.Data;

public class AppDbContext : DbContext
{
    // Constructor que recibe la configuración de la base de datos
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Definición de las tablas
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

    // Configuración y datos iniciales
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.Property(c => c.Nombre).HasMaxLength(80).IsRequired();
            entity.HasIndex(c => c.Nombre).IsUnique();
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Categorias_Nombre_Length",
                    "length(trim(\"Nombre\")) BETWEEN 1 AND 80");
                table.HasCheckConstraint(
                    "CK_Categorias_Nombre_Format",
                    "\"Nombre\" NOT GLOB '*[0-9]*'");
            });
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.Property(c => c.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(254);
            entity.Property(c => c.Celular).HasMaxLength(20);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Clientes_Nombre_Length",
                    "length(trim(\"Nombre\")) BETWEEN 1 AND 120");
                table.HasCheckConstraint(
                    "CK_Clientes_Nombre_Format",
                    "\"Nombre\" NOT GLOB '*[0-9]*'");
                table.HasCheckConstraint(
                    "CK_Clientes_Email_Length",
                    "\"Email\" IS NULL OR length(\"Email\") <= 254");
                table.HasCheckConstraint(
                    "CK_Clientes_Celular_Length",
                    "\"Celular\" IS NULL OR (\"Celular\" NOT GLOB '*[^0-9]*' AND length(\"Celular\") BETWEEN 7 AND 8)");
            });
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Productos_Nombre_Length",
                    "length(trim(\"Nombre\")) BETWEEN 1 AND 100");
                table.HasCheckConstraint(
                    "CK_Productos_Precio_Range",
                    "CAST(\"Precio\" AS NUMERIC) BETWEEN 0 AND 99999999.99");
                table.HasCheckConstraint(
                    "CK_Productos_Stock_Range",
                    "\"Stock\" BETWEEN 0 AND 1000000");
                table.HasCheckConstraint(
                    "CK_Productos_StockMinimo_Range",
                    "\"StockMinimo\" BETWEEN 0 AND 1000000");
            });
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Ventas_Total_Range",
                    "CAST(\"Total\" AS NUMERIC) BETWEEN 0 AND 99999999.99");
                table.HasCheckConstraint(
                    "CK_Ventas_Recibido_Range",
                    "CAST(\"Recibido\" AS NUMERIC) BETWEEN 0 AND 99999999.99");
                table.HasCheckConstraint(
                    "CK_Ventas_Cambio_Range",
                    "CAST(\"Cambio\" AS NUMERIC) BETWEEN 0 AND 99999999.99");
            });
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_DetallesVenta_Cantidad_Range",
                    "\"Cantidad\" BETWEEN 1 AND 1000000");
                table.HasCheckConstraint(
                    "CK_DetallesVenta_Precio_Range",
                    "CAST(\"Precio\" AS NUMERIC) BETWEEN 0 AND 99999999.99");
                table.HasCheckConstraint(
                    "CK_DetallesVenta_Subtotal_Range",
                    "CAST(\"Subtotal\" AS NUMERIC) BETWEEN 0 AND 99999999.99");
            });
        });

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Cliente)
            .WithMany(c => c.Ventas)
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleVenta>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Datos iniciales para categorías
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Limpieza" },
            new Categoria { Id = 2, Nombre = "Abarrotes" },
            new Categoria { Id = 3, Nombre = "Bebidas" }
        );

        // Datos iniciales para productos
        modelBuilder.Entity<Producto>().HasData(
            new Producto { Id = 1, Nombre = "Jabon", Precio = 8, Stock = 6, StockMinimo = 2, CategoriaId = 1 },
            new Producto { Id = 2, Nombre = "Arroz", Precio = 16, Stock = 55, StockMinimo = 10, CategoriaId = 2 },
            new Producto { Id = 3, Nombre = "CocaCola", Precio = 21, Stock = 150, StockMinimo = 15, CategoriaId = 3 }
        );
    }
}
