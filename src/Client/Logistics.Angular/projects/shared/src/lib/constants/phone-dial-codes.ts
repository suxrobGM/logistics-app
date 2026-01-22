export interface PhoneCountry {
  name: string;
  iso: string;
  dialCode: string;
  mask: string;
}

export const PHONE_COUNTRIES: PhoneCountry[] = [
  { name: "United States", iso: "US", dialCode: "+1", mask: "(999) 999-9999" },
  { name: "Canada", iso: "CA", dialCode: "+1", mask: "(999) 999-9999" },
  { name: "Mexico", iso: "MX", dialCode: "+52", mask: "99 9999 9999" },
  { name: "United Kingdom", iso: "GB", dialCode: "+44", mask: "9999 999999" },
  { name: "Germany", iso: "DE", dialCode: "+49", mask: "999 99999999" },
  { name: "France", iso: "FR", dialCode: "+33", mask: "9 99 99 99 99" },
  { name: "Italy", iso: "IT", dialCode: "+39", mask: "999 999 9999" },
  { name: "Spain", iso: "ES", dialCode: "+34", mask: "999 999 999" },
  { name: "Australia", iso: "AU", dialCode: "+61", mask: "9 9999 9999" },
  { name: "Brazil", iso: "BR", dialCode: "+55", mask: "(99) 99999-9999" },
  { name: "India", iso: "IN", dialCode: "+91", mask: "99999 99999" },
  { name: "China", iso: "CN", dialCode: "+86", mask: "999 9999 9999" },
  { name: "Japan", iso: "JP", dialCode: "+81", mask: "99 9999 9999" },
  { name: "South Korea", iso: "KR", dialCode: "+82", mask: "99 9999 9999" },
  { name: "Russia", iso: "RU", dialCode: "+7", mask: "(999) 999-99-99" },
  { name: "Poland", iso: "PL", dialCode: "+48", mask: "999 999 999" },
  { name: "Netherlands", iso: "NL", dialCode: "+31", mask: "99 999 9999" },
  { name: "Belgium", iso: "BE", dialCode: "+32", mask: "999 99 99 99" },
  { name: "Switzerland", iso: "CH", dialCode: "+41", mask: "99 999 99 99" },
  { name: "Austria", iso: "AT", dialCode: "+43", mask: "999 9999999" },
  { name: "Sweden", iso: "SE", dialCode: "+46", mask: "99 999 99 99" },
  { name: "Norway", iso: "NO", dialCode: "+47", mask: "999 99 999" },
  { name: "Denmark", iso: "DK", dialCode: "+45", mask: "99 99 99 99" },
  { name: "Finland", iso: "FI", dialCode: "+358", mask: "99 999 9999" },
  { name: "Ireland", iso: "IE", dialCode: "+353", mask: "99 999 9999" },
  { name: "Portugal", iso: "PT", dialCode: "+351", mask: "999 999 999" },
  { name: "Greece", iso: "GR", dialCode: "+30", mask: "999 999 9999" },
  { name: "Turkey", iso: "TR", dialCode: "+90", mask: "999 999 9999" },
  { name: "Ukraine", iso: "UA", dialCode: "+380", mask: "99 999 9999" },
  { name: "Czech Republic", iso: "CZ", dialCode: "+420", mask: "999 999 999" },
  { name: "Romania", iso: "RO", dialCode: "+40", mask: "999 999 999" },
  { name: "Hungary", iso: "HU", dialCode: "+36", mask: "99 999 9999" },
  { name: "Argentina", iso: "AR", dialCode: "+54", mask: "99 9999 9999" },
  { name: "Colombia", iso: "CO", dialCode: "+57", mask: "999 999 9999" },
  { name: "Chile", iso: "CL", dialCode: "+56", mask: "9 9999 9999" },
  { name: "Peru", iso: "PE", dialCode: "+51", mask: "999 999 999" },
  { name: "Venezuela", iso: "VE", dialCode: "+58", mask: "999 999 9999" },
  { name: "South Africa", iso: "ZA", dialCode: "+27", mask: "99 999 9999" },
  { name: "Egypt", iso: "EG", dialCode: "+20", mask: "99 9999 9999" },
  { name: "Nigeria", iso: "NG", dialCode: "+234", mask: "999 999 9999" },
  { name: "Kenya", iso: "KE", dialCode: "+254", mask: "999 999999" },
  { name: "Saudi Arabia", iso: "SA", dialCode: "+966", mask: "99 999 9999" },
  { name: "United Arab Emirates", iso: "AE", dialCode: "+971", mask: "99 999 9999" },
  { name: "Israel", iso: "IL", dialCode: "+972", mask: "99 999 9999" },
  { name: "Singapore", iso: "SG", dialCode: "+65", mask: "9999 9999" },
  { name: "Hong Kong", iso: "HK", dialCode: "+852", mask: "9999 9999" },
  { name: "Taiwan", iso: "TW", dialCode: "+886", mask: "999 999 999" },
  { name: "Thailand", iso: "TH", dialCode: "+66", mask: "99 999 9999" },
  { name: "Vietnam", iso: "VN", dialCode: "+84", mask: "99 999 9999" },
  { name: "Philippines", iso: "PH", dialCode: "+63", mask: "999 999 9999" },
  { name: "Indonesia", iso: "ID", dialCode: "+62", mask: "999 9999 9999" },
  { name: "Malaysia", iso: "MY", dialCode: "+60", mask: "99 999 9999" },
  { name: "New Zealand", iso: "NZ", dialCode: "+64", mask: "99 999 9999" },
  { name: "Pakistan", iso: "PK", dialCode: "+92", mask: "999 9999999" },
  { name: "Bangladesh", iso: "BD", dialCode: "+880", mask: "9999 999999" },
];

export const DEFAULT_PHONE_COUNTRY = PHONE_COUNTRIES[0]; // United States

export function findPhoneCountry(iso: string): PhoneCountry | undefined {
  return PHONE_COUNTRIES.find((c) => c.iso === iso);
}

export function findPhoneCountryByDialCode(dialCode: string): PhoneCountry | undefined {
  return PHONE_COUNTRIES.find((c) => c.dialCode === dialCode);
}
