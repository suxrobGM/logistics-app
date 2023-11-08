export abstract class RegexPatterns {
  static readonly CREDIT_CARD_NUMBER = '^[0-9]{16}$';
  static readonly CARD_EXPIRATION_DATE = '(0[1-9]|1[0-2])/[0-9]{2}'; // MM/YY format
  static readonly CARD_CVV = '^[0-9]{3,4}$'; // CVV 3 or 4 digits
  static readonly ROUTING_NUMBER = '^[0-9]{9}$'; // 9 digit routing number
}
