using System.ComponentModel.DataAnnotations;

namespace ToaToa.Domain;

/// <summary>
/// Combinação de tamanho/cor de um vestido, com estoque próprio.
/// </summary>
public class VarianteVestido
{
    public int Id { get; set; }

    public int VestidoId { get; set; }

    public Vestido? Vestido { get; set; }

    [Required(ErrorMessage = "Informe o tamanho.")]
    [MaxLength(20)]
    public string Tamanho { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a cor.")]
    [MaxLength(40)]
    public string Cor { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Estoque não pode ser negativo.")]
    public int EstoqueQtd { get; set; }

    [MaxLength(60)]
    public string? Sku { get; set; }
}
