namespace AsyncScapeIA.ComponentTests.Connectors;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AsyncScapeIA.ComponentTests.Infrastructure;
using AsyncScapeIA.Domain;
using AsyncScapeIA.Integrations.Abstractions;
using AsyncScapeIA.Integrations.GitHub;
using TUnit;
using TUnit.Assertions;

public class GitHubConnectorTests
{
    [Test]
    public async Task SyncAsync_returns_components_and_schemas_from_manifest()
    {
        var manifest = FixtureLoader.LoadText("github/manifest.yml");
        var openApi = FixtureLoader.LoadText("github/specs/payments/openapi.yml");
        var asyncApi = FixtureLoader.LoadText("github/specs/payments/asyncapi.yml");

        var handler = new FixtureHttpMessageHandler(request =>
        {
            if (request.RequestUri is null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            var path = request.RequestUri.AbsolutePath;

            if (path.EndsWith("/catalog/manifest.yml", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(manifest)
                };
            }

            if (path.EndsWith("/specs/payments/openapi.yml", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(openApi)
                };
            }

            if (path.EndsWith("/specs/payments/asyncapi.yml", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(asyncApi)
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://raw.githubusercontent.com/")
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("AsyncScapeIA-TestHarness/1.0");

        var connector = new GitHubConnector(client);
        var metadata = new Dictionary<string, string?>
        {
            ["owner"] = "asyncscape",
            ["repository"] = "architecture-docs",
            ["branch"] = "main",
            ["manifestPath"] = "catalog/manifest.yml"
        };

        var request = new ConnectorRequest(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            ConnectorProvider.GitHubRepository,
            "asyncscape/architecture-docs",
            null,
            metadata);

        var result = await connector.SyncAsync(request, CancellationToken.None);

        await Assert.That(result.Components).HasCount(1);
        var component = result.Components.Single();
        await Assert.That(component.Slug).IsEqualTo("payments-api");
        await Assert.That(component.Domain).IsEqualTo("Payments");
        await Assert.That(component.LifecycleState).IsEqualTo(LifecycleState.Active);

        await Assert.That(result.Schemas).HasCount(2);
        var openApiSchema = result.Schemas.First(schema => schema.Format == SchemaFormat.OpenApi);
        await Assert.That(openApiSchema.Name).IsEqualTo("Payments API");
        await Assert.That(openApiSchema.Checksum).IsEqualTo("sha256:abc123");

        var asyncApiSchema = result.Schemas.First(schema => schema.Format == SchemaFormat.AsyncApi);
        await Assert.That(asyncApiSchema.Version).IsEqualTo("2024.09.0");
    }
}
