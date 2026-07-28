using System.Text.Json.Nodes;
using Logistics.Application.Modules.Financial.Invoices.Commands;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class SendInvoiceToolTests
{
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly SendInvoiceTool sut;

    public SendInvoiceToolTests()
    {
        sut = new SendInvoiceTool(mediator);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("send_invoice", sut.Name);
    }

    [Fact]
    public async Task Execute_MissingInvoiceId_ReturnsError()
    {
        var result = await sut.ExecuteAsync(
            new JsonObject { ["recipient_email"] = "a@b.com" }, CancellationToken.None);

        Assert.Contains("invoice_id", result);
        await mediator.DidNotReceiveWithAnyArgs().Send<Result>(default!, default);
    }

    [Fact]
    public async Task Execute_MissingRecipientEmail_ReturnsError()
    {
        var result = await sut.ExecuteAsync(
            new JsonObject { ["invoice_id"] = Guid.NewGuid().ToString() }, CancellationToken.None);

        Assert.Contains("recipient_email", result);
    }

    [Fact]
    public async Task Execute_ValidInput_SendsCommand()
    {
        var invoiceId = Guid.NewGuid();
        mediator.Send(Arg.Any<SendInvoiceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        var result = await sut.ExecuteAsync(new JsonObject
        {
            ["invoice_id"] = invoiceId.ToString(),
            ["recipient_email"] = "billing@acme.com",
            ["personal_message"] = "Thanks for your business"
        }, CancellationToken.None);

        Assert.Contains("\"success\":true", result);
        await mediator.Received(1).Send(
            Arg.Is<SendInvoiceCommand>(c =>
                c.InvoiceId == invoiceId &&
                c.RecipientEmail == "billing@acme.com" &&
                c.PersonalMessage == "Thanks for your business"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_CommandFails_ReturnsError()
    {
        mediator.Send(Arg.Any<SendInvoiceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Invoice is cancelled"));

        var result = await sut.ExecuteAsync(new JsonObject
        {
            ["invoice_id"] = Guid.NewGuid().ToString(),
            ["recipient_email"] = "billing@acme.com"
        }, CancellationToken.None);

        Assert.Contains("Invoice is cancelled", result);
    }
}
