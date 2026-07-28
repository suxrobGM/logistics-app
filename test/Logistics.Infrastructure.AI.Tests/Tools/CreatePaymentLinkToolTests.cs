using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Financial.PaymentLinks.Commands;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class CreatePaymentLinkToolTests
{
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly CreatePaymentLinkTool sut;

    public CreatePaymentLinkToolTests()
    {
        sut = new CreatePaymentLinkTool(mediator);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("create_payment_link", sut.Name);
    }

    [Fact]
    public async Task Execute_MissingInvoiceId_ReturnsError()
    {
        var result = await sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        Assert.Contains("invoice_id", result);
        await mediator.DidNotReceiveWithAnyArgs().Send<Result<PaymentLinkDto>>(default!, default);
    }

    [Fact]
    public async Task Execute_ValidInput_ReturnsLinkUrl()
    {
        var invoiceId = Guid.NewGuid();
        mediator.Send(Arg.Any<CreatePaymentLinkCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<PaymentLinkDto>.Ok(new PaymentLinkDto
            {
                Id = Guid.NewGuid(),
                Token = "tok_123",
                InvoiceId = invoiceId,
                ExpiresAt = new DateTime(2026, 8, 27, 0, 0, 0, DateTimeKind.Utc),
                Url = "https://pay.example.com/pay/tenant/tok_123"
            }));

        var result = await sut.ExecuteAsync(new JsonObject
        {
            ["invoice_id"] = invoiceId.ToString(),
            ["expiration_days"] = 14
        }, CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.Equal("https://pay.example.com/pay/tenant/tok_123", root.GetProperty("url").GetString());

        await mediator.Received(1).Send(
            Arg.Is<CreatePaymentLinkCommand>(c => c.InvoiceId == invoiceId && c.ExpirationDays == 14),
            Arg.Any<CancellationToken>());
    }
}
