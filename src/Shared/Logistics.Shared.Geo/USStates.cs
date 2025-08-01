namespace Logistics.Shared.Geo;

public static class USStates
{
    private static readonly Dictionary<string, State> CodeLookup;

    public static readonly State[] AllStates =
    [
        new("Alabama", "AL"),
        new("Alaska", "AK"),
        new("Arizona", "AZ"),
        new("Arkansas", "AR"),
        new("California", "CA"),
        new("Colorado", "CO"),
        new("Connecticut", "CT"),
        new("Delaware", "DE"),
        new("District of Columbia", "DC"),
        new("Florida", "FL"),
        new("Georgia", "GA"),
        new("Hawaii", "HI"),
        new("Idaho", "ID"),
        new("Illinois", "IL"),
        new("Indiana", "IN"),
        new("Iowa", "IA"),
        new("Kansas", "KS"),
        new("Kentucky", "KY"),
        new("Louisiana", "LA"),
        new("Maine", "ME"),
        new("Maryland", "MD"),
        new("Massachusetts", "MA"),
        new("Michigan", "MI"),
        new("Minnesota", "MN"),
        new("Mississippi", "MS"),
        new("Missouri", "MO"),
        new("Montana", "MT"),
        new("Nebraska", "NE"),
        new("Nevada", "NV"),
        new("New Hampshire", "NH"),
        new("New Jersey", "NJ"),
        new("New Mexico", "NM"),
        new("New York", "NY"),
        new("North Carolina", "NC"),
        new("North Dakota", "ND"),
        new("Ohio", "OH"),
        new("Oklahoma", "OK"),
        new("Oregon", "OR"),
        new("Pennsylvania", "PA"),
        new("Rhode Island", "RI"),
        new("South Carolina", "SC"),
        new("South Dakota", "SD"),
        new("Tennessee", "TN"),
        new("Texas", "TX"),
        new("Utah", "UT"),
        new("Vermont", "VT"),
        new("Virginia", "VA"),
        new("Washington", "WA"),
        new("West Virginia", "WV"),
        new("Wisconsin", "WI"),
        new("Wyoming", "WY"),
    ];
    
    static USStates()
    {
        CodeLookup = AllStates.ToDictionary(state => state.Code, state => state);
    }
    
    public static State? GetStateByCode(string code)
    {
        CodeLookup.TryGetValue(code, out var state);
        return state;
    }
    
    /// <summary>
    /// Finds a state by its name or code.
    /// </summary>
    /// <param name="stateName">The name or code of the state.</param>
    /// <returns>The country object if found; otherwise, null.</returns>
    public static State? FindState(string stateName)
    {
        var state = GetStateByCode(stateName) ?? 
                      AllStates.FirstOrDefault(c => c.DisplayName.Equals(stateName, StringComparison.OrdinalIgnoreCase));

        return state;
    }
}
