namespace ToaToa.Services;

public class CartItem
{
    public int VarianteVestidoId { get; set; }
    public int VestidoId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string? ImagemUrl { get; set; }
    public int Quantidade { get; set; }

    public decimal Subtotal => Preco * Quantidade;
}

/// <summary>
/// Carrinho de compras da loja. Scoped — persiste durante o circuito interativo do usuário.
/// </summary>
public class CartService
{
    private readonly List<CartItem> _itens = new();

    public IReadOnlyList<CartItem> Itens => _itens;
    public int TotalItens => _itens.Sum(i => i.Quantidade);
    public decimal Total => _itens.Sum(i => i.Subtotal);

    public event Action? OnChange;

    public void Adicionar(CartItem item)
    {
        var existente = _itens.FirstOrDefault(i => i.VarianteVestidoId == item.VarianteVestidoId);
        if (existente is not null)
            existente.Quantidade += item.Quantidade;
        else
            _itens.Add(item);
        OnChange?.Invoke();
    }

    public void Remover(int varianteVestidoId)
    {
        _itens.RemoveAll(i => i.VarianteVestidoId == varianteVestidoId);
        OnChange?.Invoke();
    }

    public void AlterarQuantidade(int varianteVestidoId, int quantidade)
    {
        var item = _itens.FirstOrDefault(i => i.VarianteVestidoId == varianteVestidoId);
        if (item is not null)
        {
            item.Quantidade = Math.Max(1, quantidade);
            OnChange?.Invoke();
        }
    }

    public void Limpar()
    {
        _itens.Clear();
        OnChange?.Invoke();
    }
}
