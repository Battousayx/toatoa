using System.ComponentModel.DataAnnotations;

namespace ToaToa.Domain;

public class Vestido
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(170)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Descricao { get; set; }

    [Range(0.01, 1_000_000, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Preco { get; set; }

    [MaxLength(500)]
    public string? ImagemUrl { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Selecione uma categoria.")]
    public int CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }

    public ICollection<VarianteVestido> Variantes { get; set; } = new List<VarianteVestido>();

    public ICollection<FotoVestido> Fotos { get; set; } = new List<FotoVestido>();
}
