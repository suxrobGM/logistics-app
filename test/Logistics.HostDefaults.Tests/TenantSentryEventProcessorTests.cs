using Logistics.Application.Abstractions.Tenancy;
using Logistics.Domain.Entities;
using Logistics.Domain.Exceptions;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Sentry;
using Xunit;

namespace Logistics.HostDefaults.Tests;

public class TenantSentryEventProcessorTests
{
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly ICurrentTenantAccessor _tenantAccessor = Substitute.For<ICurrentTenantAccessor>();
    private readonly TenantSentryEventProcessor _sut;

    public TenantSentryEventProcessorTests()
    {
        var services = new ServiceCollection().AddSingleton(_tenantAccessor).BuildServiceProvider();
        _httpContextAccessor.HttpContext.Returns(new DefaultHttpContext { RequestServices = services });
        _sut = new TenantSentryEventProcessor(_httpContextAccessor);
    }

    [Fact]
    public void Process_TenantResolved_TagsTenantId()
    {
        var tenant = CreateTenant();
        _tenantAccessor.GetCurrentTenant().Returns(tenant);

        var result = _sut.Process(new SentryEvent());

        Assert.NotNull(result);
        Assert.Equal(tenant.Id.ToString(), result.Tags[TenantSentryEventProcessor.TenantIdTag]);
    }

    [Fact]
    public void Process_TenantResolutionThrows_SendsEventUntagged()
    {
        _tenantAccessor.GetCurrentTenant().Returns(_ => throw new InvalidTenantException("missing"));

        var result = _sut.Process(new SentryEvent());

        Assert.NotNull(result);
        Assert.False(result.Tags.ContainsKey(TenantSentryEventProcessor.TenantIdTag));
    }

    [Fact]
    public void Process_NoHttpContext_SendsEventUntagged()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        var result = _sut.Process(new SentryEvent());

        Assert.NotNull(result);
        Assert.Empty(result.Tags);
        _tenantAccessor.DidNotReceive().GetCurrentTenant();
    }

    private static Tenant CreateTenant() => new()
    {
        Name = "acme",
        BillingEmail = "billing@acme.test",
        ConnectionString = "Host=localhost",
        CompanyAddress = new Address { Line1 = "1 Main St", City = "Boston", State = "MA", ZipCode = "02101", Country = "US" }
    };
}
