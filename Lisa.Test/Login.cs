using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Lisa.Test;

public class Login : PageTest
{
    [Fact]
    public async Task GetStartedLink()
    {
        await Page.GotoAsync("http://localhost:5142");
        await Page.GetByRole(AriaRole.Button, new() { Name = " Log In" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("admin@dcegroup.co.za");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Lis@Adm!n7Dc3Gr0up");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Article)).ToContainTextAsync("Learner Information System Dashboard");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Schools" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Add School" })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add School" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Short Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Short Name" }).FillAsync("Test 1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Long Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Long Name" }).FillAsync("Test 1 From Playright");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Color" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Color" }).FillAsync("#f50000");
        await Page.GetByText("Add School Short Name Long").ClickAsync();
        await Page.GetByLabel("School Type").SelectOptionAsync(["0194a7a5-37aa-782e-8eeb-dc9b51f7fd72"]);
        await Page.GetByLabel("Curriculum").SelectOptionAsync(["0194a7a5-37f6-78e8-8227-b537564f494a"]);
        await Page.GetByRole(AriaRole.Button, new() { Name = "School SMTP Details" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("test@gmail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Hexagoon@1995");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();


        await Page.GetByRole(AriaRole.Link, new() { Name = "Teachers" }).ClickAsync();
        await SelectFirstSchool();
        await Expect(Page.GetByRole(AriaRole.Heading)).ToContainTextAsync("Teachers for Test 1");

        // Add Teacher
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add Teacher" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Surname" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Surname" }).FillAsync("Test Teacher");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Surname" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Name" }).FillAsync("Test Teacher Playwright");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("playwright@teacher.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("!@12QWqwdf");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add Teacher" }).ClickAsync();
        await Expect(Page.Locator("tbody")).ToContainTextAsync("Test Teacher Test Teacher Playwright");


        await Page.GetByRole(AriaRole.Link, new() { Name = "Schools" }).ClickAsync();
        await Expect(Page.Locator("tbody")).ToContainTextAsync("Test 1");
        await Page.GetByRole(AriaRole.Button, new() { Name = "" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Article)).ToContainTextAsync("Are you sure you want to delete the school Test 1?");


        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();

    }

    private async Task SelectFirstSchool()
    {
        await Page.SelectOptionAsync("#SelectedSchoolId", new SelectOptionValue { Index = 1 });
    }
}
