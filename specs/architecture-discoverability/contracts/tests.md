# Phase 1 Test Scaffolds

## Unit Test (Failing) Example
```csharp
public class ArchitectureComponentTests
{
    [Fact]
    public void MustHavePrimaryOwnershipAssignment()
    {
        var component = ArchitectureComponent.Create(
            name: "Payments API",
            domain: "Payments",
            lifecycleState: LifecycleState.Active
        );

        component.AssignOwnership(new OwnershipAssignment(OwnershipRole.Secondary, Guid.NewGuid()));

        Assert.Throws<DomainRuleException>(() => component.ValidateInvariants());
    }
}
```
*Status*: Pending implementation of `ValidateInvariants` and domain rule enforcement.

## Connector Contract Test (Failing)
```csharp
public class GitHubConnectorTests
{
    [Fact]
    public async Task DiscoverArtifactsAsync_ReturnsArtifacts_WhenRepoContainsOpenApi()
    {
        var connector = new GitHubConnectorProvider(Fixture.Configuration);
        var artifacts = await connector.DiscoverArtifactsAsync(ConnectorCursor.Empty, CancellationToken.None).ToListAsync();

        artifacts.Should().NotBeEmpty(); // Pending implementation -> currently throws NotImplementedException
    }
}
```

## Playwright Test (Placeholder)
```csharp
[Fact]
public async Task CatalogSearch_ShowsComponents()
{
    using var app = await AspireTestHost.StartAsync();
    using var page = await PlaywrightFixture.LaunchAsync(app);

    await page.GotoAsync("https://localhost:5001");
    await page.FillAsync("input[aria-label='Search components']", "Payments");
    await page.ClickAsync("button:has-text('Search')");

    var result = await page.Locator("[data-testid='component-card']").First.InnerTextAsync();
    result.Should().Contain("Payments"); // Will fail until UI implemented
}
```
