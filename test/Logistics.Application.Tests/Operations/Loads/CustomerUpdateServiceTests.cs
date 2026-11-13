using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.Communications;
using Logistics.Application.Abstractions.Communications.EmailModels;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Abstractions.Options;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Application.Modules.Operations.Loads.Events;
using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Modules.Operations.Tracking.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class CustomerUpdateServiceTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IFeatureService _features = Substitute.For<IFeatureService>();
    private readonly ITrackingLinkService _trackingLinks = Substitute.For<ITrackingLinkService>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IEmailTemplateService _templates = Substitute.For<IEmailTemplateService>();
    private readonly List<CustomerUser> _portalUsers = [];
    private readonly Load _load = LoadInvoiceChargesTests.NewLoad();
    private readonly CustomerUpdateService _sut;

    public CustomerUpdateServiceTests()
    {
        _load.Customer!.Email = "ops@acme.test";

        _tenantUow.Stub<CustomerUser>(_portalUsers);
        _tenantUow.Stub<DeliveryDocument>();
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create("Acme Trucking"));

        _features.IsFeatureEnabledAsync(Arg.Any<Guid>(), TenantFeature.CustomerUpdates).Returns(true);
        _trackingLinks.GetOrCreateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new TrackingLink { Token = "t", LoadId = _load.Id, ExpiresAt = DateTime.UtcNow.AddDays(30) });
        _trackingLinks.GetUrl(Arg.Any<TrackingLink>()).Returns("https://portal.test/track/x/t");
        _templates.RenderAsync(Arg.Any<string>(), Arg.Any<ShipmentUpdateEmailModel>()).Returns("<html/>");
        _emailSender.SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _sut = new CustomerUpdateService(_tenantUow, _features, _trackingLinks, _emailSender, _templates,
            Options.Create(new CustomerPortalOptions { BaseUrl = "https://portal.test" }),
            NullLogger<CustomerUpdateService>.Instance);
    }

    #region Status updates

    [Fact]
    public async Task SendStatusUpdate_Dispatched_EmailsCustomerAndPortalUsersOnceEach()
    {
        _portalUsers.Add(PortalUser("ops@acme.test"));
        _portalUsers.Add(PortalUser("jane@acme.test"));
        _portalUsers.Add(PortalUser("gone@acme.test", isActive: false));

        await _sut.SendStatusUpdateAsync(_load, LoadStatus.Dispatched);

        await _emailSender.Received(2).SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), "<html/>");
        await _templates.Received(1).RenderAsync("ShipmentDispatched", Arg.Any<ShipmentUpdateEmailModel>());
        var update = Assert.Single(_load.CustomerUpdates);
        Assert.Equal(CustomerUpdateOutcome.Sent, update.Outcome);
        Assert.Equal(2, update.RecipientCount);
    }

    [Fact]
    public async Task SendStatusUpdate_AlreadySent_SendsNothing()
    {
        await _sut.SendStatusUpdateAsync(_load, LoadStatus.Dispatched);
        await _sut.SendStatusUpdateAsync(_load, LoadStatus.Dispatched);

        await _emailSender.Received(1).SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task SendStatusUpdate_TurnedOffForCustomer_RecordsSkip()
    {
        _load.Customer!.UpdatePreferences.PickedUp = false;

        await _sut.SendStatusUpdateAsync(_load, LoadStatus.PickedUp);

        await _emailSender.DidNotReceiveWithAnyArgs().SendEmailAsync(default!, default!, default!);
        var update = Assert.Single(_load.CustomerUpdates);
        Assert.Equal(CustomerUpdateOutcome.Skipped, update.Outcome);
        Assert.Equal("Turned off for this customer", update.Reason);
    }

    [Fact]
    public async Task SendStatusUpdate_NoEmailAnywhere_RecordsSkip()
    {
        _load.Customer!.Email = null;

        await _sut.SendStatusUpdateAsync(_load, LoadStatus.Delivered);

        Assert.Equal(CustomerUpdateOutcome.Skipped, Assert.Single(_load.CustomerUpdates).Outcome);
    }

    [Fact]
    public async Task SendStatusUpdate_FeatureOff_SendsAndRecordsNothing()
    {
        _features.IsFeatureEnabledAsync(Arg.Any<Guid>(), TenantFeature.CustomerUpdates).Returns(false);

        await _sut.SendStatusUpdateAsync(_load, LoadStatus.Dispatched);

        await _emailSender.DidNotReceiveWithAnyArgs().SendEmailAsync(default!, default!, default!);
        Assert.Empty(_load.CustomerUpdates);
    }

    [Fact]
    public async Task SendStatusUpdate_SenderFails_RecordsFailure()
    {
        _emailSender.SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        await _sut.SendStatusUpdateAsync(_load, LoadStatus.Dispatched);

        Assert.Equal(CustomerUpdateOutcome.Failed, Assert.Single(_load.CustomerUpdates).Outcome);
    }

    [Fact]
    public async Task SendStatusUpdate_Cancelled_SendsNothing()
    {
        await _sut.SendStatusUpdateAsync(_load, LoadStatus.Cancelled);

        Assert.Empty(_load.CustomerUpdates);
    }

    #endregion

    #region Delay notice

    [Fact]
    public async Task SendDelayNotice_TurnedOffForLate_StillSends()
    {
        _load.Customer!.UpdatePreferences.Late = false;

        var result = await _sut.SendDelayNoticeAsync(_load, "Road closure near Tucson.");

        Assert.True(result.IsSuccess);
        await _templates.Received(1).RenderAsync("ShipmentLate",
            Arg.Is<ShipmentUpdateEmailModel>(m => m.Message == "Road closure near Tucson."));
    }

    [Fact]
    public async Task SendDelayNotice_NoRecipients_FailsWithTheReason()
    {
        _load.Customer!.Email = null;

        var result = await _sut.SendDelayNoticeAsync(_load, "Late.");

        Assert.False(result.IsSuccess);
        Assert.Equal("The customer has no email address", result.Error);
    }

    #endregion

    #region Status event handler

    [Fact]
    public async Task Handle_PickupSupersededByDelivery_SendsOnlyTheDelivery()
    {
        var enqueuer = Substitute.For<ICommandEnqueuer>();
        var loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
        loadRepo.GetByIdAsync(_load.Id, Arg.Any<CancellationToken>()).Returns(_load);
        _tenantUow.Repository<Load>().Returns(loadRepo);
        var handler = new LoadStatusChangedCustomerUpdateHandler(_tenantUow, enqueuer);
        _load.Dispatch();
        _load.ConfirmPickup();
        _load.ConfirmDelivery();

        await handler.Handle(new LoadStatusChangedEvent(_load.Id, LoadStatus.Dispatched, LoadStatus.PickedUp), default);
        await handler.Handle(new LoadStatusChangedEvent(_load.Id, LoadStatus.PickedUp, LoadStatus.Delivered), default);

        enqueuer.Received(1).Enqueue(Arg.Any<SendCustomerStatusUpdateCommand>());
        enqueuer.Received(1).Enqueue(
            Arg.Is<SendCustomerStatusUpdateCommand>(c => c.LoadId == _load.Id && c.NewStatus == LoadStatus.Delivered));
    }

    #endregion

    private CustomerUser PortalUser(string email, bool isActive = true) => new()
    {
        UserId = Guid.NewGuid(),
        CustomerId = _load.CustomerId,
        Email = email,
        IsActive = isActive
    };
}
