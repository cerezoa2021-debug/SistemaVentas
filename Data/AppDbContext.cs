
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
