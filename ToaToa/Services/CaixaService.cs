using Microsoft.EntityFrameworkCore;
using ToaToa.Data;
using ToaToa.Domain;

namespace ToaToa.Services;

/// <summary>Resumo financeiro de um caixa, usado na conferência de fechamento.</summary>
public record ResumoCaixa(
    decimal ValorAbertura,
    decimal TotalDinheiro,
    decimal TotalCartao,
    decimal TotalPix,
    decimal Suprimentos,
    decimal Sangrias,
    decimal DinheiroEsperado)
{
    public decimal TotalVendas => TotalDinheiro + TotalCartao + TotalPix;
}

public interface ICaixaService
{
    Task<Caixa?> ObterCaixaAbertoAsync();
    Task<Caixa> AbrirCaixaAsync(decimal valorAbertura, string? usuario);
    Task RegistrarMovimentoAsync(int caixaId, TipoMovimentoCaixa tipo, decimal valor, string? motivo);
    Task<ResumoCaixa> ResumoAsync(int caixaId);
    Task<Caixa> FecharCaixaAsync(int caixaId, decimal valorContado, string? usuario);
}

public class CaixaService(IDbContextFactory<CatalogoDbContext> factory) : ICaixaService
{
    public async Task<Caixa?> ObterCaixaAbertoAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Caixas
            .Include(c => c.Movimentos)
            .FirstOrDefaultAsync(c => c.Status == StatusCaixa.Aberto);
    }

    public async Task<Caixa> AbrirCaixaAsync(decimal valorAbertura, string? usuario)
    {
        await using var db = await factory.CreateDbContextAsync();
        if (await db.Caixas.AnyAsync(c => c.Status == StatusCaixa.Aberto))
            throw new InvalidOperationException("Já existe um caixa aberto.");

        var caixa = new Caixa
        {
            DataAbertura = DateTime.UtcNow,
            ValorAbertura = valorAbertura,
            UsuarioAbertura = usuario,
            Status = StatusCaixa.Aberto
        };
        db.Caixas.Add(caixa);
        await db.SaveChangesAsync();
        return caixa;
    }

    public async Task RegistrarMovimentoAsync(int caixaId, TipoMovimentoCaixa tipo, decimal valor, string? motivo)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.MovimentosCaixa.Add(new MovimentoCaixa
        {
            CaixaId = caixaId,
            Tipo = tipo,
            Valor = valor,
            Motivo = motivo,
            DataHora = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task<ResumoCaixa> ResumoAsync(int caixaId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var caixa = await db.Caixas.FindAsync(caixaId)
            ?? throw new InvalidOperationException("Caixa não encontrado.");

        var vendas = await db.Vendas.Where(v => v.CaixaId == caixaId).ToListAsync();
        var movimentos = await db.MovimentosCaixa.Where(m => m.CaixaId == caixaId).ToListAsync();

        decimal dinheiro = vendas.Where(v => v.FormaPagamento == FormaPagamento.Dinheiro).Sum(v => v.Total);
        decimal cartao = vendas.Where(v => v.FormaPagamento == FormaPagamento.Cartao).Sum(v => v.Total);
        decimal pix = vendas.Where(v => v.FormaPagamento == FormaPagamento.Pix).Sum(v => v.Total);
        decimal suprimentos = movimentos.Where(m => m.Tipo == TipoMovimentoCaixa.Suprimento).Sum(m => m.Valor);
        decimal sangrias = movimentos.Where(m => m.Tipo == TipoMovimentoCaixa.Sangria).Sum(m => m.Valor);

        decimal dinheiroEsperado = caixa.ValorAbertura + dinheiro + suprimentos - sangrias;

        return new ResumoCaixa(caixa.ValorAbertura, dinheiro, cartao, pix, suprimentos, sangrias, dinheiroEsperado);
    }

    public async Task<Caixa> FecharCaixaAsync(int caixaId, decimal valorContado, string? usuario)
    {
        var resumo = await ResumoAsync(caixaId);

        await using var db = await factory.CreateDbContextAsync();
        var caixa = await db.Caixas.FindAsync(caixaId)
            ?? throw new InvalidOperationException("Caixa não encontrado.");
        if (caixa.Status == StatusCaixa.Fechado)
            throw new InvalidOperationException("Caixa já está fechado.");

        caixa.ValorContado = valorContado;
        caixa.Diferenca = valorContado - resumo.DinheiroEsperado;
        caixa.DataFechamento = DateTime.UtcNow;
        caixa.UsuarioFechamento = usuario;
        caixa.Status = StatusCaixa.Fechado;

        await db.SaveChangesAsync();
        return caixa;
    }
}
