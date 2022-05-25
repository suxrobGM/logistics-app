namespace Logistics.Application.Options;

public class TenantsSettings
{
    private string? _databaseProvider;
    public string? DatabaseProvider 
    { 
        get => _databaseProvider; 
        set => _databaseProvider = value?.ToLower()?.Trim();
    }

    public string? DatabaseHost { get; set; }
    public string? DatabaseUserId { get; set; }
}
