namespace AsyncScapeIA.UITests.Scenarios;

using Microsoft.Playwright;
using TUnit;

public class ManualSyncSpec
{
    [Test]
    public async Task ManualSync_triggers_connector_job_and_reports_status()
    {
        await using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        var page = await browser.NewPageAsync();

        await page.GotoAsync("http://localhost:5216/");

        await page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("platform.admin@example.com");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("P@ssword1!");
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

        await page.GetByRole(AriaRole.Navigation, new() { Name = "Main" }).GetByRole(AriaRole.Link, new() { Name = "Connectors" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Manual sync" }).ClickAsync();

        await page.GetByText("Sync job started").WaitForAsync(new LocatorWaitForOptions
        {
            Timeout = 5_000
        });

        await page.GetByText("Last sync status: Success").WaitForAsync(new LocatorWaitForOptions
        {
            Timeout = 30_000
        });
    }
}
