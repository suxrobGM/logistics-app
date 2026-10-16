using System.Linq.Expressions;
using Logistics.Application.Abstractions.Eld;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Compliance.Eld.Commands;
using Logistics.Application.Modules.Compliance.Eld.Services;
using Logistics.Application.Modules.Integrations.Webhooks.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Logistics.Application.Tests.Eld;

public class ProcessEldWebhookHandlerTests
{
    private const string Body = """{"eventId":"evt-1","eventType":"AlertIncident"}""";

    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IEldProviderFactory _factory = Substitute.For<IEldProviderFactory>();
    private readonly IEldProviderService _provider = Substitute.For<IEldProviderService>();
    private readonly IEldSyncService _sync = Substitute.For<IEldSyncService>();
    private readonly IWebhookEventTracker _events = Substitute.For<IWebhookEventTracker>();
    private readonly IFeatureService _features = Substitute.For<IFeatureService>();
    private readonly ITenantRepository<EldProviderConfiguration, Guid> _configs = Substitute.For<ITenantRepository<EldProviderConfiguration, Guid>>();
    private readonly ITenantRepository<EldDriverMapping, Guid> _mappings = Substitute.For<ITenantRepository<EldDriverMapping, Guid>>();

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly EldProviderConfiguration _config = new()
    {
        ProviderType = EldProviderType.Samsara,
        ApiKey = "token",
        WebhookSecret = "rGoy+beNph0qGBLj6Aqoydj6SGA="
    };
    private readonly EldDriverMapping _mapping = new()
    {
        EmployeeId = Guid.NewGuid(),
        ProviderConfigurationId = Guid.NewGuid(),
        ProviderType = EldProviderType.Samsara,
        ExternalDriverId = "45646"
    };
    private readonly ProcessEldWebhookHandler _sut;

    public ProcessEldWebhookHandlerTests()
    {
        _tenantUow.SetCurrentTenantByIdAsync(_tenantId).Returns(TestTenant.Create());
        _tenantUow.Repository<EldProviderConfiguration>().Returns(_configs);
        _tenantUow.Repository<EldDriverMapping>().Returns(_mappings);
        _configs.GetAsync(Any<EldProviderConfiguration>(), Arg.Any<CancellationToken>()).Returns(_config);
        _mappings.GetAsync(Any<EldDriverMapping>(), Arg.Any<CancellationToken>()).Returns(_mapping);
        _features.IsFeatureEnabledAsync(_tenantId, TenantFeature.Eld).Returns(true);
        _factory.GetProvider(_config).Returns(_provider);

        _sut = new ProcessEldWebhookHandler(
            _tenantUow, _factory, _sync, _events, _features, NullLogger<ProcessEldWebhookHandler>.Instance);
    }

    [Fact]
    public async Task Handle_UnknownTenant_FailsWithoutReadingTheBody()
    {
        _tenantUow.SetCurrentTenantByIdAsync(_tenantId).ThrowsAsync(new InvalidOperationException("not found"));

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _provider.DidNotReceive().ProcessWebhookAsync(Arg.Any<EldWebhookRequestDto>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task Handle_BadSignature_FailsAndRecordsNothing()
    {
        ProviderReturns(new EldWebhookResultDto { EventType = EldWebhookEventType.Unknown, IsValid = false, ErrorMessage = "Invalid webhook signature" });

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _events.DidNotReceive().MarkHandledAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesTimestampAndTenantSecretToProvider()
    {
        ProviderReturns(Valid(EldWebhookEventType.Unknown));

        await _sut.Handle(Command(), CancellationToken.None);

        await _provider.Received(1).ProcessWebhookAsync(
            new EldWebhookRequestDto(Body, "v1=abc", "1587597109"), _config.WebhookSecret);
    }

    [Fact]
    public async Task Handle_Replay_IsAcknowledgedWithoutSyncing()
    {
        ProviderReturns(Valid(EldWebhookEventType.ViolationCreated));
        _events.WasAlreadyHandledAsync("Samsara", $"{_tenantId}:evt-1", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _sync.DidNotReceive().SyncDriverViolationsAsync(
            Arg.Any<IEldProviderService>(), Arg.Any<EldDriverMapping>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HosViolationForMappedDriver_SyncsViolationsAndRecordsEvent()
    {
        ProviderReturns(Valid(EldWebhookEventType.ViolationCreated));

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _sync.Received(1).SyncDriverViolationsAsync(_provider, _mapping, Arg.Any<CancellationToken>());
        await _events.Received(1).MarkHandledAsync("Samsara", $"{_tenantId}:evt-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownEventType_IsAcknowledged()
    {
        ProviderReturns(Valid(EldWebhookEventType.Unknown));

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _events.Received(1).MarkHandledAsync("Samsara", $"{_tenantId}:evt-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EldFeatureOff_AcknowledgesWithoutProcessing()
    {
        _features.IsFeatureEnabledAsync(_tenantId, TenantFeature.Eld).Returns(false);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _provider.DidNotReceive().ProcessWebhookAsync(Arg.Any<EldWebhookRequestDto>(), Arg.Any<string?>());
    }

    private ProcessEldWebhookCommand Command() => new()
    {
        TenantId = _tenantId,
        ProviderType = EldProviderType.Samsara,
        RequestBodyJson = Body,
        Signature = "v1=abc",
        Timestamp = "1587597109"
    };

    private void ProviderReturns(EldWebhookResultDto result) =>
        _provider.ProcessWebhookAsync(Arg.Any<EldWebhookRequestDto>(), Arg.Any<string?>()).Returns(result);

    private static EldWebhookResultDto Valid(EldWebhookEventType type) => new()
    {
        EventType = type,
        IsValid = true,
        ExternalEventId = "evt-1",
        ExternalDriverId = "45646"
    };

    private static Expression<Func<T, bool>> Any<T>() => Arg.Any<Expression<Func<T, bool>>>();
}
