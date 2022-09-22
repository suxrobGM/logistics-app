namespace Logistics.Domain.Shared.Constraints;

public static class LoadConsts
{
    public const double MinDeliveryCost = 0;
    public const double MaxDeliveryCost = 1000000d;
    public const double MinDistance = 0;
    public const double MaxDistance = 50000d;
    public const int AddressLength = 256;
    public const int NameLength = 64;
}
