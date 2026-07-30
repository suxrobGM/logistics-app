using Logistics.Infrastructure.AI.Tools.Operations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.IdentityAccess.Customers.Queries;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Operations;

public class SearchCustomersToolTests
{
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly SearchCustomersTool sut;

    public SearchCustomersToolTests()
    {
        sut = new SearchCustomersTool(mediator);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("search_customers", sut.Name);
    }

    [Fact]
    public async Task Execute_ReturnsCompactCustomerList()
    {
        mediator.Send(Arg.Any<GetCustomersQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<CustomerDto>.Ok(
                [new CustomerDto { Id = Guid.NewGuid(), Name = "Acme", Email = "ap@acme.com" }], 1, 20));

        var result = await sut.ExecuteAsync(
            new JsonObject { ["search"] = "Acme" }, CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        var customer = Assert.Single(root.GetProperty("customers").EnumerateArray());
        Assert.Equal("Acme", customer.GetProperty("name").GetString());

        await mediator.Received(1).Send(
            Arg.Is<GetCustomersQuery>(q => q.Search == "Acme" && q.PageSize == 20),
            Arg.Any<CancellationToken>());
    }
}
