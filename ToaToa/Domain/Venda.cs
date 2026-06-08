using System.ComponentModel.DataAnnotations;

namespace ToaToa.Domain;

public class Venda
{
    public int Id { get; set; }

    public DateTime DataHora { get; set; } = DateTime.UtcNow;

    public FormaPagamento FormaPagamento { get; set; }

    public decimal Total { get; set; }

    [MaxLength(256)]
    public string? Usuario { get; set; }

    public int? CaixaId { get; set; }

    public Caixa? Caixa { get; set; }

    public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
}
