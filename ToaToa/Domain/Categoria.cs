using System.ComponentModel.DataAnnotations;

namespace ToaToa.Domain;

public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Slug { get; set; } = string.Empty;

    public ICollection<Vestido> Vestidos { get; set; } = new List<Vestido>();
}
