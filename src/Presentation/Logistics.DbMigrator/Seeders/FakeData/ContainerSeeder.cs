using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Utils;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds intermodal containers — mixed ISO types and statuses, ~70% sitting at a terminal.
/// </summary>
internal class ContainerSeeder(ILogger<ContainerSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(ContainerSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 135;
    public override IReadOnlyList<string> DependsOn => [nameof(TerminalSeeder)];

    private static readonly ContainerIsoType[] CommonIsoTypes =
    [
        ContainerIsoType.Gp20, ContainerIsoType.Gp40, ContainerIsoType.Gp40,
        ContainerIsoType.Hc40, ContainerIsoType.Hc40, ContainerIsoType.Hc45,
        ContainerIsoType.Rf40, ContainerIsoType.Tk20
    ];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Container>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var region = context.Region ?? throw new InvalidOperationException("Region profile not set");
        var terminals = context.CreatedTerminals ?? throw new InvalidOperationException("Terminals not seeded");
        if (terminals.Count == 0)
        {
            logger.LogWarning("No terminals available — skipping containers");
            context.CreatedContainers = [];
            LogCompleted(0);
            return;
        }

        var repo = context.TenantUnitOfWork.Repository<Container>();
        var ownerCodes = region.ContainerOwnerCodes;
        var created = new List<Container>();

        for (var i = 0; i < 30; i++)
        {
            var ownerCode = random.Pick((IList<string>)ownerCodes);
            var isoType = random.Pick(CommonIsoTypes);
            var roll = random.NextDouble();

            var container = new Container
            {
                Number = IsoContainerNumber.Generate(ownerCode, random),
                IsoType = isoType,
                SealNumber = $"SL{random.Next(100000, 999999)}",
                BookingReference = $"BK-{DateTime.UtcNow:yyyyMMdd}-{i + 1:D3}",
                IsLaden = false,
                GrossWeight = isoType is ContainerIsoType.Gp40 or ContainerIsoType.Hc40 or ContainerIsoType.Hc45
                    ? random.Next(18000, 30000)
                    : random.Next(8000, 18000)
            };
            await repo.AddAsync(container, cancellationToken);

            // Initial state mix:
            //  ~30% sitting empty in a depot, ~40% laden at a port, ~30% empty (no terminal yet).
            if (roll < 0.30)
            {
                container.MarkAtPort(random.Pick(terminals));
            }
            else if (roll < 0.70)
            {
                container.MarkAtPort(random.Pick(terminals));
                container.MarkAsLoaded();
            }
            // else: leave as default Empty status with no terminal

            created.Add(container);
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        context.CreatedContainers = created;
        LogCompleted(created.Count);
    }
}
