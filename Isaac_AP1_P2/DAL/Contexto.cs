using Isaac_AP1_P2.Models;
using Microsoft.EntityFrameworkCore;

namespace Isaac_AP1_P2.DAL;

public class Contexto : DbContext
{
    public Contexto(DbContextOptions<Contexto> options) : base(options) { }

    public DbSet<Productos> Productos { get; set; }
    public DbSet<Ventas> Ventas { get; set; }
    public DbSet<VentasDetalle> VentasDetalle { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<VentasDetalle>()
            .HasOne<Productos>()
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Productos>().HasData(
            new Productos { ProductoId = 1, Descripcion = "Starbucks Frappuccino Mocha 281ml", Precio = 150.00m, Stock = 50 },
            new Productos { ProductoId = 2, Descripcion = "Starbucks Frappuccino Caramel 281ml", Precio = 150.00m, Stock = 40 },
            new Productos { ProductoId = 3, Descripcion = "Starbucks Doubleshot Espresso 200ml", Precio = 175.00m, Stock = 30 },
            new Productos { ProductoId = 4, Descripcion = "Starbucks Cold Brew Vainilla 325ml", Precio = 195.00m, Stock = 25 },
            new Productos { ProductoId = 5, Descripcion = "Starbucks Latte Cremoso 281ml", Precio = 160.00m, Stock = 35 }
        );
    }
}