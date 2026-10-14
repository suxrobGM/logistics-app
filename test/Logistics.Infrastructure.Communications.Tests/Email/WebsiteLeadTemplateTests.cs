using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistics.Infrastructure.Communications.Tests.Email;

public class WebsiteLeadTemplateTests
{
    private readonly IEmailTemplateService _templates;

    public WebsiteLeadTemplateTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Resend:ApiKey"] = "test" })
            .Build();

        _templates = new ServiceCollection()
            .AddLogging()
            .AddCommunicationsInfrastructure(configuration)
            .BuildServiceProvider()
            .GetRequiredService<IEmailTemplateService>();
    }

    [Fact]
    public async Task RenderAsync_RendersFieldsAndEscapesVisitorInput()
    {
        var html = await _templates.RenderAsync("WebsiteLead", new WebsiteLeadEmailModel
        {
            Heading = "New demo request",
            Fields = [new("Company", "Acme <b>Trucking</b>"), new("Fleet size", "11-25")],
            Message = "<script>alert(1)</script>",
        });

        Assert.Contains("New demo request", html);
        Assert.Contains("Fleet size", html);
        Assert.Contains("11-25", html);
        Assert.Contains("Acme &lt;b&gt;Trucking&lt;/b&gt;", html);
        Assert.DoesNotContain("<script>", html);
    }
}
