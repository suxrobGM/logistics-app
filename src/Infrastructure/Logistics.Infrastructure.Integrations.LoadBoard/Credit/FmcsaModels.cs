namespace Logistics.Infrastructure.Integrations.LoadBoard.Credit;

internal record FmcsaDocketResponse
{
    public List<FmcsaContentItem>? Content { get; set; }
}

internal record FmcsaContentItem
{
    public FmcsaCarrier? Carrier { get; set; }
}

internal record FmcsaCarrier
{
    public string? LegalName { get; set; }

    /// <summary>
    /// "Y" when the carrier/broker is allowed to operate, "N" otherwise.
    /// </summary>
    public string? AllowedToOperate { get; set; }

    public long? DotNumber { get; set; }

    public string? StatusCode { get; set; }
}
