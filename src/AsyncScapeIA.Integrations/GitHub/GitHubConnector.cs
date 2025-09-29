namespace AsyncScapeIA.Integrations.GitHub;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AsyncScapeIA.Integrations.Abstractions;
using YamlDotNet.RepresentationModel;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public sealed class GitHubConnector : IConnectorClient
{
    private readonly HttpClient _httpClient;

    public GitHubConnector(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<ConnectorSyncResult> SyncAsync(ConnectorRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var owner = request.Metadata.GetValueOrDefault("owner") ?? throw new ArgumentException("owner is required");
        var repo = request.Metadata.GetValueOrDefault("repository") ?? throw new ArgumentException("repository is required");
        var branch = request.Metadata.GetValueOrDefault("branch") ?? "main";
        var manifestPath = request.Metadata.GetValueOrDefault("manifestPath") ?? "catalog/manifest.yml";

        // Fetch manifest
        var manifestUri = new Uri($"{owner}/{repo}/{branch}/{manifestPath}", UriKind.Relative);
        var manifestContent = await GetStringWithRetryAsync(manifestUri, cancellationToken);

        // Parse YAML manifest
        var yaml = new YamlStream();
        using (var reader = new StringReader(manifestContent))
        {
            yaml.Load(reader);
        }
        var root = (YamlMappingNode)yaml.Documents[0].RootNode;
        var componentsNode = (YamlSequenceNode)root.Children[new YamlScalarNode("components")];

        var components = new List<ConnectorComponent>();
        var schemas = new List<ConnectorSchema>();

        foreach (YamlMappingNode c in componentsNode)
        {
            var slug = c.GetString("slug");
            var name = c.GetString("name");
            var domain = c.GetString("domain");
            var lifecycleState = c.GetString("lifecycleState");

            components.Add(new ConnectorComponent(
                Slug: slug,
                Name: name,
                Domain: domain,
                LifecycleState: Enum.TryParse(lifecycleState, ignoreCase: true, out Domain.LifecycleState state) ? state : Domain.LifecycleState.Active,
                Tags: new Dictionary<string, string?>()));

            if (c.Children.TryGetValue(new YamlScalarNode("schemas"), out var schemasNode) && schemasNode is YamlSequenceNode seq)
            {
                foreach (YamlMappingNode s in seq)
                {
                    var sName = s.GetString("name");
                    var sPath = s.GetString("path");
                    var sFormat = s.GetString("format");
                    var sVersion = s.GetString("version");
                    var sChecksum = s.GetString("checksum");

                    var sourceUri = new Uri($"{owner}/{repo}/{branch}/{sPath}", UriKind.Relative);
                    schemas.Add(new ConnectorSchema(
                        Name: sName,
                        Format: string.Equals(sFormat, "openapi", StringComparison.OrdinalIgnoreCase) ? Domain.SchemaFormat.OpenApi : Domain.SchemaFormat.AsyncApi,
                        Version: sVersion,
                        SourceUri: new Uri(_httpClient.BaseAddress!, sourceUri),
                        Checksum: sChecksum,
                        OwningComponentSlug: slug));
                }
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
}

file static class YamlExtensions
{
    public static string GetString(this YamlMappingNode node, string key)
    {
        return node.Children.TryGetValue(new YamlScalarNode(key), out var value)
            ? ((YamlScalarNode)value).Value ?? string.Empty
            : string.Empty;
    }
}
