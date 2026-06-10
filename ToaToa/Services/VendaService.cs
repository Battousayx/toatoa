using Microsoft.EntityFrameworkCore;
using ToaToa.Data;
using ToaToa.Domain;

namespace ToaToa.Services;

/// <summary>Item solicitado no PDV antes de virar venda.</summary>
public record ItemCarrinho(int VarianteVestidoId, string Descricao, int Quantidade, decimal PrecoUnitario)
{
    public decimal Subtotal => PrecoUnitario * Quantidade;
}

public interface IVendaService
{
    Task<Venda> RegistrarVendaAsync(IEnumerable<ItemCarrinho> itens, FormaPagamento forma, int? caixaId, string? usuario);
    Task<List<Venda>> VendasDoDiaAsync(DateTime data);
}

public class VendaService(IDbContextFactory<CatalogoDbContext> factory) : IVendaService
{
    public async Task<Venda> RegistrarVendaAsync(IEnumerable<ItemCarrinho> itens, FormaPagamento forma, int? caixaId, string? usuario)
    {
        var lista = itens.ToList();
        if (lista.Count == 0)
            throw new InvalidOperationException("A venda não possui itens.");

        await using var db = await factory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync();

        var venda = new Venda
        {
            DataHora = DateTime.UtcNow,
            FormaPagamento = forma,
            CaixaId = caixaId,
            Usuario = usuario
        };

        foreach (var item in lista)
        {
            var variante = await db.Variantes.FirstOrDefaultAsync(v => v.Id == item.VarianteVestidoId)
                ?? throw new InvalidOperationException($"Variante {item.VarianteVestidoId} não encontrada.");

            if (variante.EstoqueQtd < item.Quantidade)
                throw new InvalidOperationException($"Estoque insuficiente para {item.Descricao} (disponível: {variante.EstoqueQtd}).");

            variante.EstoqueQtd -= item.Quantidade;

            venda.Itens.Add(new ItemVenda
            {
                VarianteVestidoId = item.VarianteVestidoId,
                DescricaoItem = item.Descricao,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario
            });
        }

        venda.Total = lista.Sum(i => i.Subtotal);
        db.Vendas.Add(venda);

        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return venda;
    }

    public async Task<List<Venda>> VendasDoDiaAsync(DateTime data)
    {
        var (inicio, fim) = StoreTime.DiaUtc(data);

        await using var db = await factory.CreateDbContextAsync();
        return await db.Vendas
            .AsNoTracking()
            .Include(v => v.Itens)
            .Where(v => v.DataHora >= inicio && v.DataHora < fim)
            .OrderByDescending(v => v.DataHora)
            .ToListAsync();
    }
}
