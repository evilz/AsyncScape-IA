namespace AsyncScapeIA.UITests.Scenarios;

using Microsoft.Playwright;
using TUnit;

public class CatalogSearchSpec
{
    [Test]
    public async Task CatalogSearch_displays_results_for_component_query()
    {
        await using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        var page = await browser.NewPageAsync();

        await page.GotoAsync("http://localhost:5216/");

        await page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("curator@example.com");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("P@ssword1!");
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

        await page.GetByRole(AriaRole.Textbox, new() { Name = "Search architecture" }).FillAsync("payments");
        await page.Keyboard.PressAsync("Enter");

        await page.GetByRole(AriaRole.Row, new() { Name = "Payments API" }).WaitForAsync(new LocatorWaitForOptions
        {
            Timeout = 5_000
        });
    }
}
