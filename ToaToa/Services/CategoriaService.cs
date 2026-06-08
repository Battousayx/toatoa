using Microsoft.EntityFrameworkCore;
using ToaToa.Data;
using ToaToa.Domain;

namespace ToaToa.Services;

public interface ICategoriaService
{
    Task<List<Categoria>> ListarAsync();
    Task<Categoria?> ObterAsync(int id);
    Task<Categoria> SalvarAsync(Categoria categoria);
    Task ExcluirAsync(int id);
}

public class CategoriaService(IDbContextFactory<CatalogoDbContext> factory) : ICategoriaService
{
    public async Task<List<Categoria>> ListarAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Categorias.AsNoTracking().OrderBy(c => c.Nome).ToListAsync();
    }

    public async Task<Categoria?> ObterAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Categorias.FindAsync(id);
    }

    public async Task<Categoria> SalvarAsync(Categoria categoria)
    {
        await using var db = await factory.CreateDbContextAsync();
        categoria.Slug = SlugHelper.Gerar(categoria.Nome);

        if (categoria.Id == 0)
            db.Categorias.Add(categoria);
        else
            db.Categorias.Update(categoria);

        await db.SaveChangesAsync();
        return categoria;
    }

    public async Task ExcluirAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var c = await db.Categorias.FindAsync(id);
        if (c is not null)
        {
            db.Categorias.Remove(c);
            await db.SaveChangesAsync();
        }
    }
}
