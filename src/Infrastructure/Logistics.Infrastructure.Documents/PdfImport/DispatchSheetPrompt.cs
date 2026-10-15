namespace Logistics.Infrastructure.Documents.PdfImport;

/// <summary>
///     System prompt instructing the LLM to extract structured load data from a dispatch sheet.
///     The JSON shape mirrors <c>DispatchSheetResponseParser.ParsedLoad</c>.
/// </summary>
internal static class DispatchSheetPrompt
{
    public const string System =
        """
        You extract structured load data from carrier/broker dispatch sheets (vehicle transport, freight).
        Return ONLY a single JSON object - no markdown, no commentary - matching this exact shape:

        {
          "orderId": string | null,
          "vehicles": [
            {
              "vin": string | null,
              "year": number | null,
              "make": string | null,
              "model": string | null,
              "color": string | null,
              "lotNumber": string | null,
              "operable": boolean | null,
              "price": string | null
            }
          ],
          "originAddress": { "line1": string, "city": string, "state": string, "zipCode": string, "country": string } | null,
          "destinationAddress": { "line1": string, "city": string, "state": string, "zipCode": string, "country": string } | null,
          "originContactName": string | null,
          "originContactPhone": string | null,
          "destinationContactName": string | null,
          "destinationContactPhone": string | null,
          "pickupDate": string | null,
          "deliveryDate": string | null,
          "paymentAmount": string | null,
          "shipperName": string | null,
          "shipperPhone": string | null,
          "shipperEmail": string | null
        }

        Rules:
        - Use ISO 8601 (YYYY-MM-DD) for dates. Convert any other date format.
        - paymentAmount is the total carrier pay as a plain number string (e.g. "1250.00") - strip currency symbols and thousands separators.
        - List every vehicle on the sheet, one object each; use an empty array when there are none.
        - lotNumber is the auction lot, stock or buyer number of that vehicle.
        - operable is false only when the sheet marks the vehicle inoperable (INOP, non-running); true when it says operable; null otherwise.
        - price is that vehicle's own carrier pay, formatted like paymentAmount, only when the sheet breaks pay down per vehicle.
        - State is the 2-letter code when possible. Default country to "USA" if not stated.
        - shipperName is the broker/shipper/company that posted the load.
        - Use null for any field you cannot find. Do not invent values.
        """;
}
