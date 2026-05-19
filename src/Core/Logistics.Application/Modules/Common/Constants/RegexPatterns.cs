namespace Logistics.Application.Modules.Common.Constants;

public static class RegexPatterns
{
    public const string TenantName = @"^[a-z]+\d*";
    public const string CreditCardNumber = @"^(\d{4} \d{4} \d{4} \d{4})$"; // "^[0-9]{16}$";
    public const string CardExpirationDate = "(0[1-9]|1[0-2])/[0-9]{2}"; // MM/YY format
    public const string CardCvc = "^[0-9]{3,4}$"; // CVC 3 or 4 digits
    public const string BankRoutingNumber = "^[0-9]{9}$"; // 9 digit routing number

    // EU/UK VAT: 2-letter ISO country prefix + 5–12 alphanumerics (uppercased).
    // Minimum 5 covers IE (7 body chars) down to legacy short formats; rejects "DE12" etc.
    public const string VatNumber = @"^[A-Z]{2}[0-9A-Z]{5,12}$";

    // EORI: 2-letter ISO country prefix + 1–15 alphanumerics (uppercased).
    public const string EoriNumber = @"^[A-Z]{2}[0-9A-Z]{1,15}$";

    // FMCSA Motor Carrier: optional "MC" / "MC-" prefix + 4–8 digits.
    public const string McNumber = @"^(MC-?)?\d{4,8}$";
}
