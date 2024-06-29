namespace Logistics.Shared;

public static class RegexPatterns
{
    public const string TENANT_NAME = @"^[a-z]+\d*";
    public const string CREDIT_CARD_NUMBER = @"^(\d{4} \d{4} \d{4} \d{4})$"; // "^[0-9]{16}$";
    public const string CARD_EXPIRATION_DATE = "(0[1-9]|1[0-2])/[0-9]{2}"; // MM/YY format
    public const string CARD_CVV = "^[0-9]{3,4}$"; // CVV 3 or 4 digits
    public const string BANK_ROUTING_NUMBER = "^[0-9]{9}$"; // 9 digit routing number
}
