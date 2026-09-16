using System.Linq.Expressions;
using Logistics.Application.Modules.Platform.ProductLicense.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Platform.ProductLicense;

public class RecordProductLicenseHeartbeatHandlerTests
{
    private readonly IMasterUnitOfWork _masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly IMasterRepository<ProductLicenseHeartbeat, Guid> _repo = Substitute.For<IMasterRepository<ProductLicenseHeartbeat, Guid>>();
    private readonly RecordProductLicenseHeartbeatHandler _sut;

    public RecordProductLicenseHeartbeatHandlerTests()
    {
        _masterUow.Repository<ProductLicenseHeartbeat>().Returns(_repo);
        _sut = new RecordProductLicenseHeartbeatHandler(_masterUow);
    }

    [Fact]
    public async Task Handle_NewInstance_AddsRow()
    {
        _repo.GetAsync(Arg.Any<Expression<Func<ProductLicenseHeartbeat, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((ProductLicenseHeartbeat?)null);
        var command = Command();

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _repo.Received(1).AddAsync(
            Arg.Is<ProductLicenseHeartbeat>(h =>
                h.InstanceId == command.Report.InstanceId
                && h.Hostname == "box-1"
                && h.Version == "1.2.3"
                && h.TenantCount == 4
                && h.FirstSeenAt == h.LastSeenAt),
            Arg.Any<CancellationToken>());
        await _masterUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KnownInstance_UpdatesLastSeenOnly()
    {
        var firstSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var existing = new ProductLicenseHeartbeat
        {
            InstanceId = Guid.NewGuid(),
            Hostname = "old",
            Version = "1.0.0",
            FirstSeenAt = firstSeen,
            LastSeenAt = firstSeen
        };
        _repo.GetAsync(Arg.Any<Expression<Func<ProductLicenseHeartbeat, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existing);
        var command = Command(existing.InstanceId);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(firstSeen, existing.FirstSeenAt);
        Assert.True(existing.LastSeenAt > firstSeen);
        Assert.Equal("box-1", existing.Hostname);
        Assert.Equal("1.2.3", existing.Version);
        Assert.Equal("Acme", existing.Licensee);
        await _repo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        _repo.Received(1).Update(existing);
        await _masterUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static RecordProductLicenseHeartbeatCommand Command(Guid? instanceId = null) => new(
        new ProductLicenseHeartbeatDto
        {
            InstanceId = instanceId ?? Guid.NewGuid(),
            Hostname = "box-1",
            Version = "1.2.3",
            KeyId = "2026-09",
            Licensee = "Acme",
            TenantCount = 4
        });
}
