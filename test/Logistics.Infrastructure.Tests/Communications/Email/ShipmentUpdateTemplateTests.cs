using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Infrastructure.Communications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Tests.Communications.Email;

public class ShipmentUpdateTemplateTests
{
    private readonly IEmailTemplateService _templates;

    public ShipmentUpdateTemplateTests()
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

    private static ShipmentUpdateEmailModel Model(List<string>? vehicles = null) => new()
    {
        CompanyName = "Acme Auto Transport",
        LoadNumber = 1042,
        LoadName = "2 vehicles",
        OriginCity = "Dallas",
        DestinationCity = "Phoenix",
        Window = "Nov 12, 8:00 AM - 2:00 PM",
        Eta = "Nov 12, 3:15 PM",
        TrackingUrl = "https://portal.test/track/t/abc",
        ShipmentUrl = "https://portal.test/shipments/abc",
        Message = "A closed road near Tucson added two hours.",
        DeliveredAt = "November 14, 2026",
        ReceiverName = "Sam Receiver",
        Vehicles = vehicles ?? []
    };

    [Fact]
    public async Task RenderAsync_DeliveredVehicleLoad_ListsEveryVehicleAndLinksTheEbol()
    {
        var html = await _templates.RenderAsync("ShipmentDelivered",
            Model(["2021 Honda Accord - VIN 1HGBH41JXMN109186", "2018 Ford F-150"]));

        Assert.Contains("#1042", html);
        Assert.Contains("Sam Receiver", html);
        Assert.Contains("2021 Honda Accord - VIN 1HGBH41JXMN109186", html);
        Assert.Contains("2018 Ford F-150", html);
        Assert.Contains("eBOL", html);
        Assert.Contains("https://portal.test/shipments/abc", html);
    }

    [Fact]
    public async Task RenderAsync_DeliveredFreightLoad_HasNoEbolCopy()
    {
        var html = await _templates.RenderAsync("ShipmentDelivered", Model());

        Assert.Contains("Your shipment was delivered", html);
        Assert.DoesNotContain("eBOL", html);
    }

    [Theory]
    [InlineData("ShipmentDispatched", "Pickup window")]
    [InlineData("ShipmentPickedUp", "Delivery window")]
    [InlineData("ShipmentLate", "Appointment window")]
    public async Task RenderAsync_InTransitUpdates_ShowTheWindowAndTrackingLink(string template, string windowLabel)
    {
        var html = await _templates.RenderAsync(template, Model());

        Assert.Contains($"{windowLabel}: Nov 12, 8:00 AM - 2:00 PM", html);
        Assert.Contains("https://portal.test/track/t/abc", html);
    }

    [Fact]
    public async Task RenderAsync_Late_QuotesTheDelayNotice()
    {
        var html = await _templates.RenderAsync("ShipmentLate", Model());

        Assert.Contains("A closed road near Tucson added two hours.", html);
        Assert.Contains("Estimated arrival: Nov 12, 3:15 PM", html);
    }
}
