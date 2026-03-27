using System.Text.Json;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using StockApp.Options;

namespace StockApp.Services;

// MinIO resmi .NET istemcisi ile yükleme/silme (PutObjectArgs, bucket oluşturma)
public sealed class MinioFileService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioStorageOptions _options;
    private readonly HashSet<string> _bucketReady = new();
    private readonly SemaphoreSlim _bucketSemaphore = new(1, 1);

    public MinioFileService(IOptions<MinioStorageOptions> options)
    {
        _options = options.Value;

        // HTTPS için WithSSL(); HTTP için çağrılmaz. Güvenli çağrı olması için
        var builder = new MinioClient()
            .WithEndpoint(_options.Endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey);

        _minioClient = (_options.UseSSL ? builder.WithSSL() : builder).Build();
    }

    public async Task<string> UploadObjectAsync(Stream stream, string objectName, long objectSize, string contentType)
    {
        var bucketName = _options.Bucket;
        await EnsureBucketExistsAsync(bucketName);

        await _minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(objectSize)
                .WithContentType(contentType));

        return $"{_options.BaseUrl.TrimEnd('/')}/{bucketName}/{objectName}";
    }

    public async Task DeleteObjectByUrlAsync(string fileUrl)
    {
        var parsed = ParsePhotoUrl(fileUrl);
        if (parsed is not { } t)
        {
            return;
        }

        await _minioClient.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(t.bucket)
                .WithObject(t.fileName));
    }

    // URL yolundan bucket ve nesne adı: /bucket/object
    public (string bucket, string fileName)? ParsePhotoUrl(string? photoUrl)
    {
        if (string.IsNullOrEmpty(photoUrl))
        {
            return null;
        }

        try
        {
            var uri = new Uri(photoUrl);
            var pathParts = uri.AbsolutePath.TrimStart('/').Split('/', 2);

            if (pathParts.Length == 2)
            {
                return (pathParts[0], pathParts[1]);
            }
        }
        catch
        {
            throw new InvalidOperationException($"Geçersiz dosya URL'si: {photoUrl}");
        }

        return null;
    }

    /// <summary>
    /// Uygulama açılışında veya mevcut bucket için: tarayıcıdan doğrudan URL ile okuma (anonim GetObject).
    /// Aksi halde MinIO varsayılanı private bucket → AccessDenied.
    /// </summary>
    public async Task EnsurePublicReadPolicyAsync(CancellationToken cancellationToken = default)
    {
        var bucketName = _options.Bucket;
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            return;
        }

        await _bucketSemaphore.WaitAsync(cancellationToken);
        try
        {
            var exists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucketName), cancellationToken);
            if (!exists)
            {
                return;
            }

            await ApplyAnonymousReadPolicyAsync(bucketName, cancellationToken);
            _bucketReady.Add(bucketName);
        }
        finally
        {
            _bucketSemaphore.Release();
        }
    }

    private async Task EnsureBucketExistsAsync(string bucketName)
    {
        if (_bucketReady.Contains(bucketName))
        {
            return;
        }

        await _bucketSemaphore.WaitAsync();
        try
        {
            if (_bucketReady.Contains(bucketName))
            {
                return;
            }

            var bucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucketName));

            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(bucketName));
            }

            await ApplyAnonymousReadPolicyAsync(bucketName, CancellationToken.None);
            _bucketReady.Add(bucketName);
        }
        finally
        {
            _bucketSemaphore.Release();
        }
    }

    private static string BuildAnonymousReadOnlyPolicy(string bucketName)
    {
        // MinIO S3 uyumlu: anonim s3:GetObject (tarayıcı img src ile doğrudan URL).
        var doc = new
        {
            Version = "2012-10-17",
            Statement = new object[]
            {
                new
                {
                    Effect = "Allow",
                    Principal = new { AWS = new[] { "*" } },
                    Action = new[] { "s3:GetObject" },
                    Resource = new[] { $"arn:aws:s3:::{bucketName}/*" }
                }
            }
        };
        return JsonSerializer.Serialize(doc);
    }

    private async Task ApplyAnonymousReadPolicyAsync(string bucketName, CancellationToken cancellationToken)
    {
        var policyJson = BuildAnonymousReadOnlyPolicy(bucketName);
        await _minioClient.SetPolicyAsync(
            new SetPolicyArgs()
                .WithBucket(bucketName)
                .WithPolicy(policyJson),
            cancellationToken);
    }
}
