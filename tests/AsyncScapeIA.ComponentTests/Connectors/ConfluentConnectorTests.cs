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
using AsyncScapeIA.Integrations.Confluent;
using TUnit;
using TUnit.Assertions;

public class ConfluentConnectorTests
{
    [Test]
    public async Task SyncAsync_materializes_openapi_artifact_from_latest_subject_version()
    {
        var latestResponse = FixtureLoader.LoadText("confluent/subject-latest.json");
        var schemaResponse = FixtureLoader.LoadText("confluent/subject-schema.json");

        var handler = new FixtureHttpMessageHandler(request =>
        {
            if (request.RequestUri is null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            var path = request.RequestUri.AbsolutePath;

            if (path.EndsWith("/subjects/catalog-payments/versions/latest", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(latestResponse)
                };
            }

            if (path.EndsWith("/subjects/catalog-payments/versions/12", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(schemaResponse)
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://schema.asyncscape.test/")
        };

        var metadata = new Dictionary<string, string?>
        {
            ["subject"] = "catalog-payments"
        };

        var connector = new ConfluentConnector(client);
        var request = new ConnectorRequest(
            Guid.Parse("00000000-0000-0000-0000-000000000002"),
            ConnectorProvider.ConfluentSchemaRegistry,
            "catalog-payments",
            null,
            metadata);

        var result = await connector.SyncAsync(request, CancellationToken.None);

        await Assert.That(result.Schemas).HasCount(1);
        var schema = result.Schemas.Single();
        await Assert.That(schema.Name).IsEqualTo("catalog-payments");
        await Assert.That(schema.Format).IsEqualTo(SchemaFormat.OpenApi);
        await Assert.That(schema.Version).IsEqualTo("12");
        await Assert.That(schema.SourceUri).IsEqualTo(new Uri("https://schema.asyncscape.test/subjects/catalog-payments/versions/12"));
    }
}
