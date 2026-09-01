using System.Linq.Expressions;
using Logistics.Application.Modules.Platform.ProductLicense.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Platform.ProductLicense;

public class RecordProductLicenseHeartbeatHandlerTests
{
    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly IMasterRepository<LicenseHeartbeat, Guid> repo = Substitute.For<IMasterRepository<LicenseHeartbeat, Guid>>();
    private readonly RecordProductLicenseHeartbeatHandler sut;

    public RecordProductLicenseHeartbeatHandlerTests()
    {
        masterUow.Repository<LicenseHeartbeat>().Returns(repo);
        sut = new RecordProductLicenseHeartbeatHandler(masterUow);
    }

    [Fact]
    public async Task Handle_NewInstance_AddsRow()
    {
        repo.GetAsync(Arg.Any<Expression<Func<LicenseHeartbeat, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((LicenseHeartbeat?)null);
        var command = Command();

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await repo.Received(1).AddAsync(
            Arg.Is<LicenseHeartbeat>(h =>
                h.InstanceId == command.InstanceId
                && h.Hostname == "box-1"
                && h.Version == "1.2.3"
                && h.TenantCount == 4
                && h.FirstSeenAt == h.LastSeenAt),
            Arg.Any<CancellationToken>());
        await masterUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KnownInstance_UpdatesLastSeenOnly()
    {
        var firstSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var existing = new LicenseHeartbeat
        {
            InstanceId = Guid.NewGuid(),
            Hostname = "old",
            Version = "1.0.0",
            FirstSeenAt = firstSeen,
            LastSeenAt = firstSeen
        };
        repo.GetAsync(Arg.Any<Expression<Func<LicenseHeartbeat, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existing);
        var command = Command(existing.InstanceId);

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(firstSeen, existing.FirstSeenAt);
        Assert.True(existing.LastSeenAt > firstSeen);
        Assert.Equal("box-1", existing.Hostname);
        Assert.Equal("1.2.3", existing.Version);
        Assert.Equal("Acme", existing.Licensee);
        await repo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        repo.Received(1).Update(existing);
        await masterUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static RecordProductLicenseHeartbeatCommand Command(Guid? instanceId = null) => new()
    {
        InstanceId = instanceId ?? Guid.NewGuid(),
        Hostname = "box-1",
        Version = "1.2.3",
        KeyId = "2026-09",
        Licensee = "Acme",
        TenantCount = 4
    };
}
