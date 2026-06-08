using System.ComponentModel.DataAnnotations;

namespace ToaToa.Domain;

/// <summary>Sangria (retirada) ou suprimento (reforço) de dinheiro no caixa.</summary>
public class MovimentoCaixa
{
    public int Id { get; set; }

    public int CaixaId { get; set; }

    public Caixa? Caixa { get; set; }

    public TipoMovimentoCaixa Tipo { get; set; }

    [Range(0.01, 1_000_000, ErrorMessage = "Valor deve ser maior que zero.")]
    public decimal Valor { get; set; }

    [MaxLength(300)]
    public string? Motivo { get; set; }

    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}
