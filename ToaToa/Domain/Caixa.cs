using System.ComponentModel.DataAnnotations;

namespace ToaToa.Domain;

/// <summary>
/// Sessão de caixa: abertura com fundo de troco, movimentos e fechamento com conferência.
/// </summary>
public class Caixa
{
    public int Id { get; set; }

    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;

    public DateTime? DataFechamento { get; set; }

    /// <summary>Fundo de troco / valor inicial.</summary>
    public decimal ValorAbertura { get; set; }

    /// <summary>Valor em dinheiro contado na conferência do fechamento.</summary>
    public decimal? ValorContado { get; set; }

    /// <summary>Diferença = ValorContado - valor esperado em dinheiro.</summary>
    public decimal? Diferenca { get; set; }

    public StatusCaixa Status { get; set; } = StatusCaixa.Aberto;

    [MaxLength(256)]
    public string? UsuarioAbertura { get; set; }

    [MaxLength(256)]
    public string? UsuarioFechamento { get; set; }

    public ICollection<Venda> Vendas { get; set; } = new List<Venda>();

    public ICollection<MovimentoCaixa> Movimentos { get; set; } = new List<MovimentoCaixa>();
}
