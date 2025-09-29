namespace AsyncScapeIA.Infrastructure.Storage;

using System.Net;

public sealed class MinioObjectStorage : IObjectStorage
{
    private readonly string _endpoint;
    private readonly string _accessKey;
    private readonly string _secretKey;

    public MinioObjectStorage(string endpoint, string accessKey, string secretKey)
    {
        _endpoint = endpoint;
        _accessKey = accessKey;
        _secretKey = secretKey;
    }

    public Task UploadAsync(string bucket, string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        // TODO: integrate MinIO SDK
        throw new NotImplementedException();
    }

    public Task<Stream> DownloadAsync(string bucket, string key, CancellationToken cancellationToken)
    {
        // TODO: integrate MinIO SDK
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(string bucket, string key, CancellationToken cancellationToken)
    {
        // TODO: integrate MinIO SDK
        throw new NotImplementedException();
    }
}

