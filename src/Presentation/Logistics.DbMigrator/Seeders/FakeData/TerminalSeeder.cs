using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds intermodal terminals from the active region profile.
/// </summary>
internal class TerminalSeeder(ILogger<TerminalSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(TerminalSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 125;

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Terminal>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var region = context.Region ?? throw new InvalidOperationException("Region profile not set");
        var repo = context.TenantUnitOfWork.Repository<Terminal>();

        var created = new List<Terminal>();
        foreach (var seed in region.Terminals)
        {
            var terminal = new Terminal
            {
                Name = seed.Name,
                Code = seed.Code,
                CountryCode = seed.CountryCode,
                Type = seed.Type,
                Address = seed.Address
            };
            await repo.AddAsync(terminal, cancellationToken);
            created.Add(terminal);
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        context.CreatedTerminals = created;
        LogCompleted(created.Count);
    }
}
