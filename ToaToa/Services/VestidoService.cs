using Microsoft.EntityFrameworkCore;
using ToaToa.Data;
using ToaToa.Domain;

namespace ToaToa.Services;

public interface IVestidoService
{
    Task<List<Vestido>> ListarAsync(bool somenteAtivos = false);
    Task<Vestido?> ObterAsync(int id);
    Task<Vestido?> ObterPorSlugAsync(string slug);
    Task<Vestido> SalvarAsync(Vestido vestido, List<VarianteVestido> variantes);
    Task ExcluirAsync(int id);

    Task AdicionarFotoAsync(int vestidoId, string objectKey, string url);
    Task RemoverFotoAsync(int fotoId);
    Task DefinirFotoPrincipalAsync(int vestidoId, int fotoId);
}

public class VestidoService(IDbContextFactory<CatalogoDbContext> factory) : IVestidoService
{
    public async Task<List<Vestido>> ListarAsync(bool somenteAtivos = false)
    {
        await using var db = await factory.CreateDbContextAsync();
        var query = db.Vestidos
            .AsNoTracking()
            .Include(v => v.Categoria)
            .Include(v => v.Fotos)
            .Include(v => v.Variantes)
            .AsQueryable();

        if (somenteAtivos)
            query = query.Where(v => v.Ativo);

        return await query.OrderBy(v => v.Nome).ToListAsync();
    }

    public async Task<Vestido?> ObterAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Vestidos
            .Include(v => v.Variantes)
            .Include(v => v.Fotos)
            .Include(v => v.Categoria)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Vestido?> ObterPorSlugAsync(string slug)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Vestidos
            .AsNoTracking()
            .Include(v => v.Variantes)
            .Include(v => v.Fotos)
            .Include(v => v.Categoria)
            .FirstOrDefaultAsync(v => v.Slug == slug);
    }

    public async Task<Vestido> SalvarAsync(Vestido vestido, List<VarianteVestido> variantes)
    {
        await using var db = await factory.CreateDbContextAsync();
        vestido.Slug = SlugHelper.Gerar(vestido.Nome);

        if (vestido.Id == 0)
        {
            vestido.Variantes = variantes;
            db.Vestidos.Add(vestido);
        }
        else
        {
            db.Vestidos.Update(vestido);

            // Sincroniza variantes (remove as ausentes, adiciona/atualiza as demais)
            var existentes = await db.Variantes.Where(v => v.VestidoId == vestido.Id).ToListAsync();
            var idsMantidos = variantes.Where(v => v.Id != 0).Select(v => v.Id).ToHashSet();
            db.Variantes.RemoveRange(existentes.Where(e => !idsMantidos.Contains(e.Id)));

            foreach (var variante in variantes)
            {
                variante.VestidoId = vestido.Id;
                if (variante.Id == 0)
                    db.Variantes.Add(variante);
                else
                    db.Variantes.Update(variante);
            }
        }

        await db.SaveChangesAsync();
        return vestido;
    }

    public async Task ExcluirAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var v = await db.Vestidos.FindAsync(id);
        if (v is not null)
        {
            db.Vestidos.Remove(v);
            await db.SaveChangesAsync();
        }
    }

    public async Task AdicionarFotoAsync(int vestidoId, string objectKey, string url)
    {
        await using var db = await factory.CreateDbContextAsync();
        var qtd = await db.Fotos.CountAsync(f => f.VestidoId == vestidoId);
        var primeira = qtd == 0;

        db.Fotos.Add(new FotoVestido
        {
            VestidoId = vestidoId,
            ObjectKey = objectKey,
            Url = url,
            Ordem = qtd,
            Principal = primeira
        });

        if (primeira)
        {
            var vestido = await db.Vestidos.FindAsync(vestidoId);
            if (vestido is not null)
                vestido.ImagemUrl = url;
        }

        await db.SaveChangesAsync();
    }

    public async Task RemoverFotoAsync(int fotoId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var foto = await db.Fotos.FindAsync(fotoId);
        if (foto is not null)
        {
            db.Fotos.Remove(foto);
            await db.SaveChangesAsync();
        }
    }

    public async Task DefinirFotoPrincipalAsync(int vestidoId, int fotoId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var fotos = await db.Fotos.Where(f => f.VestidoId == vestidoId).ToListAsync();
        foreach (var f in fotos)
            f.Principal = f.Id == fotoId;

        var principal = fotos.FirstOrDefault(f => f.Id == fotoId);
        if (principal is not null)
        {
            var vestido = await db.Vestidos.FindAsync(vestidoId);
            if (vestido is not null)
                vestido.ImagemUrl = principal.Url;
        }

        await db.SaveChangesAsync();
    }
}
