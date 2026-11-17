using System.Linq.Expressions;
using Isaac_AP1_P2.DAL;
using Isaac_AP1_P2.Models;
using Microsoft.EntityFrameworkCore;

namespace Isaac_AP1_P2.Services;

public class ProductosService(IDbContextFactory<Contexto> dbFactory)
{
    public async Task<List<Productos>> Listar(Expression<Func<Productos, bool>> criterio)
    {
        await using var contexto = await dbFactory.CreateDbContextAsync();
        return await contexto.Productos
            .Where(criterio)
            .OrderBy(p => p.Descripcion)
            .AsNoTracking()
            .ToListAsync();
    }
}