namespace AsyncScapeIA.Integrations.Confluent;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AsyncScapeIA.Integrations.Abstractions;
using System.Text.Json;
using System.Collections.Generic;

public sealed class ConfluentConnector : IConnectorClient
{
    private readonly HttpClient _httpClient;

    public ConfluentConnector(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<ConnectorSyncResult> SyncAsync(ConnectorRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var subject = request.Metadata.GetValueOrDefault("subject") ?? request.ExternalIdentifier;

        // GET latest version
        var latestUri = new Uri($"subjects/{subject}/versions/latest", UriKind.Relative);
        var latestRaw = await GetStringWithRetryAsync(latestUri, cancellationToken);
        var latestJson = NormalizeJson(latestRaw);
        using var latestDoc = JsonDocument.Parse(latestJson);
        var version = latestDoc.RootElement.GetProperty("version").GetInt32();

        var versionUri = new Uri($"subjects/{subject}/versions/{version}", UriKind.Relative);
        // optional: we could fetch to validate availability
        await GetStringWithRetryAsync(versionUri, cancellationToken);

        var schema = new ConnectorSchema(
            Name: subject,
            Format: Domain.SchemaFormat.OpenApi,
            Version: version.ToString(),
            SourceUri: new Uri(_httpClient.BaseAddress!, versionUri),
            Checksum: "sha256::unavailable",
            OwningComponentSlug: null);

        return new ConnectorSyncResult(
            Components: new List<ConnectorComponent>(),
            Schemas: new List<ConnectorSchema> { schema },
            ContinuationToken: null,
            Diagnostics: new List<ConnectorDiagnostic>());
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
            return input; // already valid
        }
        catch
        {
            // Attempt to unescape common escape sequences from double-escaped fixtures
            return System.Text.RegularExpressions.Regex.Unescape(input);
        }
    }
}
