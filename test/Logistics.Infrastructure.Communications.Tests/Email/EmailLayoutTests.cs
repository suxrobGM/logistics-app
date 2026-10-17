using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistics.Infrastructure.Communications.Tests.Email;

public class EmailLayoutTests
{
    private readonly IEmailTemplateService _templates;

    public EmailLayoutTests()
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
    public async Task RenderAsync_WrapsTemplateInLayoutWithItsTitleAndPreheader()
    {
        var html = await _templates.RenderAsync("PasswordReset", new PasswordResetEmailModel
        {
            ResetUrl = "https://id.test/Account/ResetPassword?code=abc&email=a%40b.test"
        });

        Assert.StartsWith("<!DOCTYPE html>", html.TrimStart());
        Assert.Contains("<title>Reset your password</title>", html);
        Assert.Contains("Use the link inside to choose a new password.", html);
        Assert.Contains("href=\"https://id.test/Account/ResetPassword?code=abc&amp;email=a%40b.test\"", html);
        Assert.Contains("LogisticsX", html);
    }

    [Fact]
    public async Task RenderAsync_EncodesUserTextAndShowsTheSendingCompany()
    {
        var html = await _templates.RenderAsync("PayrollRejected", new PayrollRejectedEmailModel
        {
            EmployeeName = "José <b>Driver</b>",
            PayrollNumber = 42,
            RejectionReason = "<img src=x onerror=alert(1)>",
            PeriodStart = "Oct 1",
            PeriodEnd = "Oct 14"
        });

        Assert.Contains("José &lt;b&gt;Driver&lt;/b&gt;", html);
        Assert.DoesNotContain("<img src=x", html);
    }
}
