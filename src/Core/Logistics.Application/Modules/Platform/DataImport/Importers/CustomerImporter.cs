using Logistics.Application.Modules.Platform.DataImport.Constants;
using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore;
using static Logistics.Application.Modules.Platform.DataImport.Constants.ImportFieldKeys;

namespace Logistics.Application.Modules.Platform.DataImport.Importers;

/// <summary>
///     Customers match on name. An MC number has no column of its own, so it is appended to the notes.
/// </summary>
internal sealed class CustomerImporter(ITenantUnitOfWork tenantUow) : EntityImporter<CustomerImporter.Draft>
{
    public override ImportEntityType EntityType => ImportEntityType.Customers;

    protected override Draft Parse(ImportRow row, ImportOptions options)
    {
        var draft = new Draft(row, row.Get(Name) ?? "");
        draft.ReadRequired(Name, "Customer name");
        draft.Address = draft.ReadAddress(null, "Address", options, required: false);

        var mc = row.Get(McNumber);
        var notes = row.Get(Notes);
        draft.NotesText = mc is null ? notes : string.Join("\n", new[] { notes, $"MC# {mc}" }.Where(n => n is not null));
        return draft;
    }

    protected override async Task ResolveAsync(IReadOnlyList<Draft> drafts, ImportOptions options, CancellationToken ct)
    {
        var names = drafts.Select(d => d.Key.ToLower()).ToArray();
        var existing = await tenantUow.Repository<Customer>().Query()
            .Where(c => names.Contains(c.Name.ToLower()))
            .ToListAsync(ct);
        var byName = existing
            .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var draft in drafts)
        {
            draft.Existing = byName.GetValueOrDefault(draft.Key);
            draft.Exists = draft.Existing is not null;
        }
    }

    protected override async Task WriteAsync(IReadOnlyList<Draft> drafts, ImportOptions options, CancellationToken ct)
    {
        var repository = tenantUow.Repository<Customer>();
        foreach (var draft in drafts)
        {
            var row = draft.Row;
            if (draft.Existing is { } customer)
            {
                customer.Email = row.Get(Email) ?? customer.Email;
                customer.Phone = row.Get(Phone) ?? customer.Phone;
                customer.TaxId = row.Get(TaxId) ?? customer.TaxId;
                customer.Address = draft.Address ?? customer.Address;
                customer.Notes = draft.NotesText ?? customer.Notes;
                continue;
            }

            await repository.AddAsync(new Customer
            {
                Name = draft.Key,
                Email = row.Get(Email),
                Phone = row.Get(Phone),
                TaxId = row.Get(TaxId),
                Address = draft.Address,
                Notes = draft.NotesText
            }, ct);
        }

        await tenantUow.SaveChangesAsync(ct);
    }

    internal sealed class Draft(ImportRow row, string key) : ImportDraft(row, key)
    {
        public Address? Address { get; set; }

        /// <summary>
        ///     The customer's notes column, distinct from <see cref="ImportDraft.Notices"/> (messages for the user).
        /// </summary>
        public string? NotesText { get; set; }

        public Customer? Existing { get; set; }
    }
}
