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

        return await query.OrderBy(v => v.Nome).AsSplitQuery().ToListAsync();
    }

    public async Task<Vestido?> ObterAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Vestidos
            .Include(v => v.Variantes)
            .Include(v => v.Fotos)
            .Include(v => v.Categoria)
            .AsSplitQuery()
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
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.Slug == slug);
    }

    public async Task<Vestido> SalvarAsync(Vestido vestido, List<VarianteVestido> variantes)
    {
        await using var db = await factory.CreateDbContextAsync();
        vestido.Slug = SlugHelper.Gerar(vestido.Nome);

        if (vestido.Id == 0)
        {
            // Novo: anexa só os escalares + variantes (sem navegação de Categoria destacada).
            vestido.Categoria = null;
            vestido.Variantes = variantes.Select(v => new VarianteVestido
            {
                Tamanho = v.Tamanho, Cor = v.Cor, EstoqueQtd = v.EstoqueQtd, Sku = v.Sku
            }).ToList();
            db.Vestidos.Add(vestido);
            await db.SaveChangesAsync();
            return vestido;
        }

        // Edição: carrega a entidade RASTREADA e copia os campos (evita conflitos de tracking).
        var atual = await db.Vestidos
            .Include(v => v.Variantes)
            .FirstOrDefaultAsync(v => v.Id == vestido.Id)
            ?? throw new InvalidOperationException("Vestido não encontrado.");

        atual.Nome = vestido.Nome;
        atual.Slug = vestido.Slug;
        atual.Descricao = vestido.Descricao;
        atual.Preco = vestido.Preco;
        atual.ImagemUrl = vestido.ImagemUrl;
        atual.Ativo = vestido.Ativo;
        atual.CategoriaId = vestido.CategoriaId;

        // Sincroniza variantes: remove ausentes, atualiza existentes, insere novas.
        var idsMantidos = variantes.Where(v => v.Id != 0).Select(v => v.Id).ToHashSet();
        db.Variantes.RemoveRange(atual.Variantes.Where(e => !idsMantidos.Contains(e.Id)));

        foreach (var nova in variantes)
        {
            var existente = nova.Id != 0 ? atual.Variantes.FirstOrDefault(e => e.Id == nova.Id) : null;
            if (existente is null)
            {
                atual.Variantes.Add(new VarianteVestido
                {
                    Tamanho = nova.Tamanho, Cor = nova.Cor, EstoqueQtd = nova.EstoqueQtd, Sku = nova.Sku
                });
            }
            else
            {
                existente.Tamanho = nova.Tamanho;
                existente.Cor = nova.Cor;
                existente.EstoqueQtd = nova.EstoqueQtd;
                existente.Sku = nova.Sku;
            }
        }

        await db.SaveChangesAsync();
        return atual;
    }

    public async Task ExcluirAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var v = await db.Vestidos.Include(x => x.Variantes).FirstOrDefaultAsync(x => x.Id == id);
        if (v is null)
            return;

        var varianteIds = v.Variantes.Select(va => va.Id).ToList();
        var temVenda = await db.ItensVenda.AnyAsync(i => varianteIds.Contains(i.VarianteVestidoId));
        if (temVenda)
            throw new InvalidOperationException("Este vestido possui vendas registradas e não pode ser excluído. Desative-o (Ativo = não).");

        db.Vestidos.Remove(v);
        await db.SaveChangesAsync();
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
