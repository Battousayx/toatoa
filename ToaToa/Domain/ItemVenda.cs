namespace ToaToa.Domain;

public class ItemVenda
{
    public int Id { get; set; }

    public int VendaId { get; set; }

    public Venda? Venda { get; set; }

    public int VarianteVestidoId { get; set; }

    public VarianteVestido? VarianteVestido { get; set; }

    /// <summary>Nome do vestido no momento da venda (snapshot).</summary>
    public string DescricaoItem { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    public decimal PrecoUnitario { get; set; }

    public decimal Subtotal => PrecoUnitario * Quantidade;
}
