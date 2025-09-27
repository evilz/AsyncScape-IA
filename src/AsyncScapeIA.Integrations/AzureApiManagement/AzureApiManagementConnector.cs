namespace AsyncScapeIA.Integrations.AzureApiManagement;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AsyncScapeIA.Integrations.Abstractions;

public sealed class AzureApiManagementConnector : IConnectorClient
{
    private readonly HttpClient _httpClient;

    public AzureApiManagementConnector(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public Task<ConnectorSyncResult> SyncAsync(ConnectorRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
