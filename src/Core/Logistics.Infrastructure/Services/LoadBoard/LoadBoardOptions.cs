namespace Logistics.Infrastructure.Services;

public record LoadBoardOptions
{
    public DatOptions? Dat { get; set; }
    public TruckstopOptions? Truckstop { get; set; }
    public OneTwo3LoadboardOptions? OneTwo3Loadboard { get; set; }
}

public record DatOptions
{
    public string BaseUrl { get; set; } = "https://freight.api.dat.com";
}

public record TruckstopOptions
{
    public string BaseUrl { get; set; } = "https://api.truckstop.com";
}

public record OneTwo3LoadboardOptions
{
    public string BaseUrl { get; set; } = "https://api.123loadboard.com";
}
