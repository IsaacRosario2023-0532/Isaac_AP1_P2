using System.Linq.Expressions;
using Isaac_AP1_P2.DAL;
using Isaac_AP1_P2.Models;
using Microsoft.EntityFrameworkCore;

namespace Isaac_AP1_P2.Services;
public class VentasService(IDbContextFactory<Contexto> dbFactory)
{
    public async Task<bool> Existe(int ventaId)
    {
        await using var contexto = await dbFactory.CreateDbContextAsync();
        return await contexto.Ventas.AnyAsync(v => v.VentaId == ventaId);
    }

    public async Task<(bool Ok, string Mensaje)> Guardar(Ventas venta)
    {
        try
        {
            if (venta.VentasDetalle.Count == 0)
                return (false, "Debe agregar al menos un producto al detalle");

            if (venta.VentaId == 0 || !await Existe(venta.VentaId))
                return await Insertar(venta);

            return await Modificar(venta);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }
        catch (DbUpdateException)
        {
            return (false, "Error al guardar en la base de datos");
        }
    }

    private async Task<(bool, string)> Insertar(Ventas venta)
    {
        await using var contexto = await dbFactory.CreateDbContextAsync();
        await using var tx = await contexto.Database.BeginTransactionAsync();

        venta.Total = venta.VentasDetalle.Sum(d => d.Cantidad * d.Precio);

        await AfectarStock(contexto, venta.VentasDetalle, -1);

        contexto.Ventas.Add(venta);
        await contexto.SaveChangesAsync();
        await tx.CommitAsync();

        return (true, "Venta guardada correctamente");
    }

    private async Task<(bool, string)> Modificar(Ventas venta)
    {
        await using var contexto = await dbFactory.CreateDbContextAsync();
        await using var tx = await contexto.Database.BeginTransactionAsync();

        var original = await contexto.Ventas
            .Include(v => v.VentasDetalle)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.VentaId == venta.VentaId);

        if (original == null)
            return (false, "La venta no existe");

        await AfectarStock(contexto, original.VentasDetalle, +1);

        await contexto.VentasDetalle
            .Where(d => d.VentaId == venta.VentaId)
            .ExecuteDeleteAsync();

        var nueva = new Ventas
        {
            VentaId = venta.VentaId,
            Fecha = venta.Fecha,
            Observacion = venta.Observacion,
            Total = venta.VentasDetalle.Sum(d => d.Cantidad * d.Precio),
            VentasDetalle = venta.VentasDetalle.Select(d => new VentasDetalle
            {
                VentaId = venta.VentaId,
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                Precio = d.Precio
            }).ToList()
        };

        await AfectarStock(contexto, nueva.VentasDetalle, -1);

        contexto.Ventas.Update(nueva);
        await contexto.SaveChangesAsync();
        await tx.CommitAsync();

        return (true, "Venta modificada correctamente");
    }

    public async Task<(bool Ok, string Mensaje)> Eliminar(int ventaId)
    {
        try
        {
            await using var contexto = await dbFactory.CreateDbContextAsync();
            await using var tx = await contexto.Database.BeginTransactionAsync();

            var venta = await contexto.Ventas
                .Include(v => v.VentasDetalle)
                .FirstOrDefaultAsync(v => v.VentaId == ventaId);

            if (venta == null)
                return (false, "La venta no existe");

            await AfectarStock(contexto, venta.VentasDetalle, +1);

            contexto.Ventas.Remove(venta);
            await contexto.SaveChangesAsync();
            await tx.CommitAsync();

            return (true, "Venta eliminada correctamente");
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }
        catch (DbUpdateException)
        {
            return (false, "Error al eliminar en la base de datos");
        }
    }

    public async Task<Ventas?> Buscar(int ventaId)
    {
        await using var contexto = await dbFactory.CreateDbContextAsync();
        return await contexto.Ventas
            .Include(v => v.VentasDetalle)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.VentaId == ventaId);
    }

    public async Task<List<Ventas>> Listar(Expression<Func<Ventas, bool>> criterio)
    {
        await using var contexto = await dbFactory.CreateDbContextAsync();
        return await contexto.Ventas
            .Where(criterio)
            .OrderBy(v => v.VentaId)
            .AsNoTracking()
            .ToListAsync();
    }

    private static async Task AfectarStock(Contexto contexto, IEnumerable<VentasDetalle> detalles, int factor)
    {
        foreach (var d in detalles)
        {
            var producto = await contexto.Productos.FindAsync(d.ProductoId)
                ?? throw new InvalidOperationException("El producto no existe");

            producto.Stock += factor * d.Cantidad;

            if (producto.Stock < 0)
                throw new InvalidOperationException(
                    $"Stock insuficiente de '{producto.Descripcion}'. Disponible: {producto.Stock - (factor * d.Cantidad)}");
        }
    }
}