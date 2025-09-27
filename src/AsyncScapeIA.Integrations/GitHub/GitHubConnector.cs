namespace AsyncScapeIA.Integrations.GitHub;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AsyncScapeIA.Integrations.Abstractions;

public sealed class GitHubConnector : IConnectorClient
{
    private readonly HttpClient _httpClient;

    public GitHubConnector(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public Task<ConnectorSyncResult> SyncAsync(ConnectorRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
