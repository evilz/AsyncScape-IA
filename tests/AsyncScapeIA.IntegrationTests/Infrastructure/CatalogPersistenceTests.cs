using Npgsql;
using TUnit;
using TUnit.Assertions;
using Testcontainers.PostgreSql;

namespace AsyncScapeIA.IntegrationTests.Infrastructure;

public class CatalogPersistenceTests
{
    [Test]
    public async Task Store_component_persists_record()
    {
        await using var container = new PostgreSqlBuilder().Build();
        await container.StartAsync();

        await using var connection = new NpgsqlConnection(container.GetConnectionString());

        try
        {
            await connection.OpenAsync();
        }
        catch (Exception ex)
        {
            Assert.Fail($"OpenAsync threw: {ex}");
        }

        Assert.Fail("Repository persistence not implemented yet");
    }
}
