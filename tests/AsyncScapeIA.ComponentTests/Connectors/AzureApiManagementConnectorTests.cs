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
using AsyncScapeIA.Integrations.AzureApiManagement;
using TUnit;
using TUnit.Assertions;

public class AzureApiManagementConnectorTests
{
    [Test]
    public async Task SyncAsync_projects_api_inventory_into_connector_result()
    {
        var apisPayload = FixtureLoader.LoadText("azure/apis.json");
        var specPayload = FixtureLoader.LoadText("azure/payments-openapi.json");

        var handler = new FixtureHttpMessageHandler(request =>
        {
            if (request.RequestUri is null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            var host = request.RequestUri.Host;
            var path = request.RequestUri.AbsolutePath;

            if (host.Equals("management.azure.com", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/apis", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(apisPayload)
                };
            }

            if (host.Equals("storage.contoso.net", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/payments-openapi.json", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(specPayload)
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://management.azure.com/")
        };

        var metadata = new Dictionary<string, string?>
        {
            ["subscriptionId"] = "00000000-1111-2222-3333-000000000000",
            ["resourceGroup"] = "rg-asyncscape-portal",
            ["serviceName"] = "asyncscape-apim",
            ["apiVersion"] = "2023-09-01-preview",
            ["defaultDomain"] = "Payments"
        };

        var connector = new AzureApiManagementConnector(client);
        var request = new ConnectorRequest(
            Guid.Parse("00000000-0000-0000-0000-000000000003"),
            ConnectorProvider.AzureApiManagement,
            "asyncscape-apim",
            null,
            metadata);

        var result = await connector.SyncAsync(request, CancellationToken.None);

        await Assert.That(result.Components).HasCount(1);
        var component = result.Components.Single();
        await Assert.That(component.Name).IsEqualTo("Payments API");
        await Assert.That(component.Domain).IsEqualTo("Payments");
        await Assert.That(component.LifecycleState).IsEqualTo(LifecycleState.Active);

        await Assert.That(result.Schemas).HasCount(1);
        var schema = result.Schemas.Single();
        await Assert.That(schema.Format).IsEqualTo(SchemaFormat.OpenApi);
        await Assert.That(schema.SourceUri).IsEqualTo(new Uri("https://storage.contoso.net/contracts/payments-openapi.json"));
    }
}
