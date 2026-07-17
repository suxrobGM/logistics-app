namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Commands;

/// <summary>
/// The rate fields shared by the create and update commands, so both inherit one set of
/// validation rules.
/// </summary>
public interface IIftaTaxRateFields
{
    string CountryCode { get; }
    string? Region { get; }
    int Year { get; }
    int Quarter { get; }
    decimal RatePerGallon { get; }
    decimal? SurchargeRatePerGallon { get; }
}
