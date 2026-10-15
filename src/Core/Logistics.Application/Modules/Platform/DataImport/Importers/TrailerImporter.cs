using Logistics.Application.Modules.Platform.DataImport.Constants;
using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using static Logistics.Application.Modules.Platform.DataImport.Constants.ImportFieldKeys;

namespace Logistics.Application.Modules.Platform.DataImport.Importers;

/// <summary>
///     Trailers match on number. Length is stored in metres, read as feet unless the header or the
///     tenant's region says metres.
/// </summary>
internal sealed class TrailerImporter(ITenantUnitOfWork tenantUow) : EntityImporter<TrailerImporter.Draft>
{
    /// <summary>
    ///     Equipment codes and spellings other systems and load boards use.
    /// </summary>
    internal static readonly IReadOnlyDictionary<string, TrailerType> TypeAliases = new Dictionary<string, TrailerType>
    {
        ["Van"] = TrailerType.DryVan, ["V"] = TrailerType.DryVan, ["Dry"] = TrailerType.DryVan,
        ["53 Van"] = TrailerType.DryVan, ["R"] = TrailerType.Reefer, ["Refrigerated"] = TrailerType.Reefer,
        ["Refer"] = TrailerType.Reefer, ["F"] = TrailerType.Flatbed, ["FB"] = TrailerType.Flatbed,
        ["Flat"] = TrailerType.Flatbed, ["SD"] = TrailerType.StepDeck, ["Step"] = TrailerType.StepDeck,
        ["Drop Deck"] = TrailerType.StepDeck, ["RGN"] = TrailerType.Lowboy, ["Low Boy"] = TrailerType.Lowboy,
        ["Tank"] = TrailerType.Tanker, ["T"] = TrailerType.Tanker, ["Auto Carrier"] = TrailerType.CarHauler,
        ["Car Carrier"] = TrailerType.CarHauler, ["Auto"] = TrailerType.CarHauler,
        ["Container Chassis"] = TrailerType.Chassis, ["C"] = TrailerType.Chassis,
        ["Curtain"] = TrailerType.Conestoga, ["Curtainside"] = TrailerType.Conestoga
    };

    private static readonly IReadOnlyDictionary<string, TrailerStatus> StatusAliases = new Dictionary<string, TrailerStatus>
    {
        ["In Service"] = TrailerStatus.Active, ["Available"] = TrailerStatus.Active,
        ["OOS"] = TrailerStatus.OutOfService, ["Repair"] = TrailerStatus.OutOfService,
        ["Maintenance"] = TrailerStatus.OutOfService, ["Retired"] = TrailerStatus.Inactive,
        ["Sold"] = TrailerStatus.Inactive, ["Disabled"] = TrailerStatus.Inactive
    };

    public override ImportEntityType EntityType => ImportEntityType.Trailers;

    protected override Draft Parse(ImportRow row, ImportOptions options)
    {
        var draft = new Draft(row, row.Get(Number) ?? "");
        draft.ReadRequired(Number, "Trailer number");
        if (draft.Key.Length > 50)
        {
            draft.Errors.Add("Trailer number is longer than 50 characters.");
        }

        draft.Type = draft.ReadEnum(ImportFieldKeys.Type, "Trailer type", TypeAliases);
        if (row.Get(ImportFieldKeys.Type) is null)
        {
            draft.Notices.Add("No trailer type given; a new trailer defaults to Dry Van.");
        }

        draft.Status = draft.ReadEnum(Status, "Status", StatusAliases);
        draft.Year = draft.ReadInt(Year, "Year");
        if (draft.ReadDecimal(Length, "Length") is { } length)
        {
            draft.LengthMeters = ImportValueParser.ToMeters(length, row.Headers.GetValueOrDefault(Length), options.Region);
        }

        return draft;
    }

    protected override async Task ResolveAsync(IReadOnlyList<Draft> drafts, ImportOptions options, CancellationToken ct)
    {
        var numbers = drafts.Select(d => d.Key).ToArray();
        var existing = await tenantUow.Repository<Trailer>().Query()
            .Where(t => numbers.Contains(t.Number))
            .ToDictionaryAsync(t => t.Number, ct);

        foreach (var draft in drafts)
        {
            draft.Existing = existing.GetValueOrDefault(draft.Key);
            draft.Exists = draft.Existing is not null;
        }
    }

    protected override async Task WriteAsync(IReadOnlyList<Draft> drafts, ImportOptions options, CancellationToken ct)
    {
        var repository = tenantUow.Repository<Trailer>();
        foreach (var draft in drafts)
        {
            var row = draft.Row;
            var trailer = draft.Existing ?? new Trailer { Number = draft.Key, Type = TrailerType.DryVan };

            trailer.Type = draft.Type ?? trailer.Type;
            trailer.Status = draft.Status ?? trailer.Status;
            trailer.Vin = row.Get(Vin) ?? trailer.Vin;
            trailer.Make = row.Get(Make) ?? trailer.Make;
            trailer.Model = row.Get(Model) ?? trailer.Model;
            trailer.Year = draft.Year ?? trailer.Year;
            trailer.LicensePlate = row.Get(LicensePlate) ?? trailer.LicensePlate;
            trailer.LicensePlateState = row.Get(LicensePlateState) ?? trailer.LicensePlateState;
            trailer.LengthMeters = draft.LengthMeters ?? trailer.LengthMeters;
            trailer.Notes = row.Get(Notes) ?? trailer.Notes;

            if (draft.Existing is null)
            {
                await repository.AddAsync(trailer, ct);
            }
        }

        await tenantUow.SaveChangesAsync(ct);
    }

    internal sealed class Draft(ImportRow row, string key) : ImportDraft(row, key)
    {
        public TrailerType? Type { get; set; }
        public TrailerStatus? Status { get; set; }
        public int? Year { get; set; }
        public decimal? LengthMeters { get; set; }
        public Trailer? Existing { get; set; }
    }
}
