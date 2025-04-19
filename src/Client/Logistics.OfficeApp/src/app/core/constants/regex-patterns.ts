export const REGEX_PATTERNS = {
  cardNumber: "^(\\d{4} \\d{4} \\d{4} \\d{4})$", //'^[0-9]{16}$';
  cardExpDate: "(0[1-9]|1[0-2])/[0-9]{2}", // MM/YY format
  cardCvc: "^[0-9]{3,4}$", // CVV 3 or 4 digits
  usBankRoutingNumber: "^[0-9]{9}$", // 9 digit routing number
} as const;
