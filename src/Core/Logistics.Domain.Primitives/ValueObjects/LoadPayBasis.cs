namespace Logistics.Domain.Primitives.ValueObjects;

/// <param name="Distance">Metres.</param>
public sealed record LoadPayBasis(
    decimal Linehaul, decimal FuelSurcharge, decimal Accessorials, double Distance, int DepartedStops = 0);
