namespace AsyncScapeIA.Integrations.AzureApiManagement;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AsyncScapeIA.Integrations.Abstractions;
using System.Text.Json;
using System.Collections.Generic;

public sealed class AzureApiManagementConnector : IConnectorClient
{
    private readonly HttpClient _httpClient;

    public AzureApiManagementConnector(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<ConnectorSyncResult> SyncAsync(ConnectorRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var subscriptionId = request.Metadata.GetValueOrDefault("subscriptionId") ?? throw new ArgumentException("subscriptionId is required");
        var resourceGroup = request.Metadata.GetValueOrDefault("resourceGroup") ?? throw new ArgumentException("resourceGroup is required");
        var serviceName = request.Metadata.GetValueOrDefault("serviceName") ?? request.ExternalIdentifier;
        var apiVersion = request.Metadata.GetValueOrDefault("apiVersion") ?? "2023-09-01-preview";
        var defaultDomain = request.Metadata.GetValueOrDefault("defaultDomain") ?? "General";

        var listUri = new Uri($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.ApiManagement/service/{serviceName}/apis?api-version={apiVersion}", UriKind.Relative);
        var raw = await GetStringWithRetryAsync(listUri, cancellationToken);
        var json = NormalizeJson(raw);

        using var doc = JsonDocument.Parse(json);
        var value = doc.RootElement.GetProperty("value");

        var components = new List<ConnectorComponent>();
        var schemas = new List<ConnectorSchema>();

        foreach (var item in value.EnumerateArray())
        {
            var name = item.GetProperty("name").GetString() ?? "api";
            var props = item.GetProperty("properties");
            var displayName = props.GetProperty("displayName").GetString() ?? name;
            var link = props.GetProperty("value").GetString();

            components.Add(new ConnectorComponent(
                Slug: name,
                Name: displayName,
                Domain: defaultDomain,
                LifecycleState: Domain.LifecycleState.Active,
                Tags: new Dictionary<string, string?>()));

            if (!string.IsNullOrWhiteSpace(link))
            {
                schemas.Add(new ConnectorSchema(
                    Name: displayName,
                    Format: Domain.SchemaFormat.OpenApi,
                    Version: "1",
                    SourceUri: new Uri(link),
                    Checksum: "sha256::unavailable",
                    OwningComponentSlug: name));
            }
        }

        return new ConnectorSyncResult(components, schemas, null, new List<ConnectorDiagnostic>());
    }

    private async Task<string> GetStringWithRetryAsync(Uri uri, CancellationToken cancellationToken)
    {
        int attempts = 0;
        while (true)
        {
            attempts++;
            try
            {
                using var response = await _httpClient.GetAsync(uri, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (HttpRequestException) when (attempts < 3)
            {
                await Task.Delay(50, cancellationToken);
            }
        }
    }

    private static string NormalizeJson(string input)
    {
        try
        {
            using var _ = JsonDocument.Parse(input);
            return input; // valid JSON
        }
        catch
        {
            return System.Text.RegularExpressions.Regex.Unescape(input);
        }
    }
}
