namespace Logistics.Application.Constants;

public static class RegexPatterns
{
    public const string TenantName = @"^[a-z]+\d*";
    public const string CreditCardNumber = @"^(\d{4} \d{4} \d{4} \d{4})$"; // "^[0-9]{16}$";
    public const string CardExpirationDate = "(0[1-9]|1[0-2])/[0-9]{2}"; // MM/YY format
    public const string CardCvc = "^[0-9]{3,4}$"; // CVC 3 or 4 digits
    public const string BankRoutingNumber = "^[0-9]{9}$"; // 9 digit routing number
}
