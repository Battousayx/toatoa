namespace ToaToa.Domain;

/// <summary>
/// Foto de um vestido, armazenada no MinIO. Url é o link público/derivado do ObjectKey.
/// </summary>
public class FotoVestido
{
    public int Id { get; set; }

    public int VestidoId { get; set; }

    public Vestido? Vestido { get; set; }

    /// <summary>Chave do objeto no bucket do MinIO.</summary>
    public string ObjectKey { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public int Ordem { get; set; }

    public bool Principal { get; set; }
}
