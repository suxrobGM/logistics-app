using Logistics.Application.Modules.Platform.DataImport.Constants;
using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Application.Validators;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.ValueObjects;
using static Logistics.Application.Modules.Platform.DataImport.Constants.ImportFieldKeys;

namespace Logistics.Application.Modules.Platform.DataImport.Importers;

/// <summary>Typed cell reads that record an error on the draft instead of throwing.</summary>
internal static class ImportCells
{
    private static readonly AddressValidator AddressValidator = new();

    extension(ImportDraft draft)
    {
        public string? ReadRequired(string key, string label)
        {
            var value = draft.Row.Get(key);
            if (value is null)
            {
                draft.Errors.Add($"{label} is required.");
            }

            return value;
        }

        public decimal? ReadDecimal(string key, string label)
        {
            var raw = draft.Row.Get(key);
            var value = ImportValueParser.ParseDecimal(raw);
            if (raw is not null && value is null)
            {
                draft.Errors.Add($"{label} '{raw}' is not a number.");
            }

            return value;
        }

        public int? ReadInt(string key, string label)
        {
            var raw = draft.Row.Get(key);
            var value = ImportValueParser.ParseInt(raw);
            if (raw is not null && value is null)
            {
                draft.Errors.Add($"{label} '{raw}' is not a whole number.");
            }

            return value;
        }

        public DateTime? ReadDate(string key, string label, ImportOptions options)
        {
            var raw = draft.Row.Get(key);
            var value = ImportValueParser.ParseDate(raw, options.DateFormat);
            if (raw is not null && value is null)
            {
                draft.Errors.Add($"{label} '{raw}' is not a date this import understands (try YYYY-MM-DD).");
            }

            return value;
        }

        public TEnum? ReadEnum<TEnum>(
            string key, string label, IReadOnlyDictionary<string, TEnum>? aliases = null)
            where TEnum : struct, System.Enum
        {
            var raw = draft.Row.Get(key);
            var value = ImportValueParser.ParseEnum(raw, aliases);
            if (raw is not null && value is null)
            {
                draft.Errors.Add($"{label} '{raw}' is not recognised.");
            }

            return value;
        }

        /// <summary>Validates like the form does, including against the tenant's region.</summary>
        public Address? ReadAddress(
            string? prefix, string label, ImportOptions options, bool required)
        {
            var row = draft.Row;
            var address = ImportAddressParser.Build(
                row.Get(ImportFieldKeys.Address(prefix, Street)),
                row.Get(ImportFieldKeys.Address(prefix, Street2)),
                row.Get(ImportFieldKeys.Address(prefix, City)),
                row.Get(ImportFieldKeys.Address(prefix, State)),
                row.Get(ImportFieldKeys.Address(prefix, Zip)),
                row.Get(ImportFieldKeys.Address(prefix, Country)),
                row.Get(ImportFieldKeys.Address(prefix, FullAddress)),
                options.DefaultCountry);

            if (address is null)
            {
                if (required)
                {
                    draft.Errors.Add($"{label} is required.");
                }

                return null;
            }

            var validation = AddressValidator.Validate(address);
            foreach (var error in validation.Errors)
            {
                draft.Errors.Add($"{label}: {error.ErrorMessage}");
            }

            if (validation.IsValid && !RegionCountries.IsAllowed(options.Region, address.Country))
            {
                draft.Errors.Add($"{label}: country '{address.Country}' is outside your company's {options.Region} region.");
            }

            return validation.IsValid ? address : null;
        }
    }

}
