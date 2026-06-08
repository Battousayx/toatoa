using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace ToaToa.Services;

public interface IStorageService
{
    /// <summary>Garante que o bucket existe e é de leitura pública. Best-effort.</summary>
    Task EnsureBucketAsync();

    /// <summary>Envia um arquivo e retorna (objectKey, url pública).</summary>
    Task<(string ObjectKey, string Url)> UploadAsync(Stream conteudo, string nomeArquivo, string contentType);

    Task RemoverAsync(string objectKey);
}

public class StorageService(IMinioClient client, IOptions<MinioOptions> options, ILogger<StorageService> logger) : IStorageService
{
    private readonly MinioOptions _opt = options.Value;

    public async Task EnsureBucketAsync()
    {
        const int maxTentativas = 10;
        for (var tentativa = 1; tentativa <= maxTentativas; tentativa++)
        {
            try
            {
                var existe = await client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_opt.Bucket));
                if (!existe)
                {
                    await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_opt.Bucket));
                }

                // Política de leitura pública para servir as imagens diretamente via URL.
                var policy = $$"""
                {
                  "Version": "2012-10-17",
                  "Statement": [
                    {
                      "Effect": "Allow",
                      "Principal": "*",
                      "Action": ["s3:GetObject"],
                      "Resource": ["arn:aws:s3:::{{_opt.Bucket}}/*"]
                    }
                  ]
                }
                """;
                await client.SetPolicyAsync(new SetPolicyArgs().WithBucket(_opt.Bucket).WithPolicy(policy));
                logger.LogInformation("Bucket MinIO '{Bucket}' pronto.", _opt.Bucket);
                return;
            }
            catch (Exception ex)
            {
                if (tentativa == maxTentativas)
                {
                    logger.LogWarning(ex, "Não foi possível inicializar o bucket MinIO '{Bucket}' após {N} tentativas. O upload de fotos ficará indisponível até o MinIO estar acessível.", _opt.Bucket, maxTentativas);
                    return;
                }
                logger.LogInformation("MinIO ainda indisponível (tentativa {T}/{N}); aguardando...", tentativa, maxTentativas);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }

    public async Task<(string ObjectKey, string Url)> UploadAsync(Stream conteudo, string nomeArquivo, string contentType)
    {
        var ext = Path.GetExtension(nomeArquivo);
        var objectKey = $"vestidos/{Guid.NewGuid():N}{ext}";

        // PutObject precisa do tamanho; garante stream com Length.
        Stream stream = conteudo;
        if (!conteudo.CanSeek)
        {
            var ms = new MemoryStream();
            await conteudo.CopyToAsync(ms);
            ms.Position = 0;
            stream = ms;
        }

        await client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_opt.Bucket)
            .WithObject(objectKey)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType));

        var url = $"{_opt.PublicBaseUrl.TrimEnd('/')}/{_opt.Bucket}/{objectKey}";
        return (objectKey, url);
    }

    public async Task RemoverAsync(string objectKey)
    {
        try
        {
            await client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(_opt.Bucket).WithObject(objectKey));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao remover objeto '{Key}' do MinIO.", objectKey);
        }
    }
}
