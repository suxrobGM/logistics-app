using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistics.Infrastructure.Communications.Tests.Email;

public class VehicleDeliveredTemplateTests
{
    private readonly IEmailTemplateService _templates;

    public VehicleDeliveredTemplateTests()
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
    public async Task RenderAsync_ListsEveryVehicleAndLinksTheShipment()
    {
        var html = await _templates.RenderAsync("VehicleDelivered", new VehicleDeliveredEmailModel
        {
            CompanyName = "Acme Auto Transport",
            LoadNumber = 1042,
            LoadName = "2 vehicles",
            DestinationCity = "Phoenix",
            DeliveredAt = "October 14, 2026",
            ReceiverName = "Sam Receiver",
            Vehicles = ["2021 Honda Accord - VIN 1HGBH41JXMN109186", "2018 Ford F-150"],
            ShipmentUrl = "https://portal.test/shipments/abc"
        });

        Assert.Contains("#1042", html);
        Assert.Contains("Sam Receiver", html);
        Assert.Contains("2021 Honda Accord - VIN 1HGBH41JXMN109186", html);
        Assert.Contains("2018 Ford F-150", html);
        Assert.Contains("https://portal.test/shipments/abc", html);
    }
}
