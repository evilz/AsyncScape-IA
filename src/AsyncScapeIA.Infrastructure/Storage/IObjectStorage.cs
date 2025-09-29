namespace AsyncScapeIA.Infrastructure.Storage;

public interface IObjectStorage
{
    Task UploadAsync(string bucket, string key, Stream content, string contentType, CancellationToken cancellationToken);
    Task<Stream> DownloadAsync(string bucket, string key, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string bucket, string key, CancellationToken cancellationToken);
}

