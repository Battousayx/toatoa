namespace ToaToa.Services;

public class MinioOptions
{
    public const string SectionName = "Minio";

    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin";
    public string Bucket { get; set; } = "toatoa-vestidos";
    /// <summary>URL base pública para montar o link das imagens (sem barra final).</summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:9000";
    public bool UseSSL { get; set; }
}
