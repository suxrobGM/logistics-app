export interface PhoneCountry {
  name: string;
  iso: string;
  dialCode: string;
  mask: string;
}

export const PHONE_COUNTRIES: PhoneCountry[] = [
  // North America
  { name: "United States", iso: "US", dialCode: "+1", mask: "(999) 999-9999" },
  { name: "Canada", iso: "CA", dialCode: "+1", mask: "(999) 999-9999" },
  { name: "Mexico", iso: "MX", dialCode: "+52", mask: "99 9999 9999" },

  // Central America & Caribbean
  { name: "Bahamas", iso: "BS", dialCode: "+1242", mask: "999-9999" },
  { name: "Barbados", iso: "BB", dialCode: "+1246", mask: "999-9999" },
  { name: "Belize", iso: "BZ", dialCode: "+501", mask: "999-9999" },
  { name: "Costa Rica", iso: "CR", dialCode: "+506", mask: "9999 9999" },
  { name: "Cuba", iso: "CU", dialCode: "+53", mask: "9 9999999" },
  { name: "Dominican Republic", iso: "DO", dialCode: "+1809", mask: "999-9999" },
  { name: "El Salvador", iso: "SV", dialCode: "+503", mask: "9999 9999" },
  { name: "Guatemala", iso: "GT", dialCode: "+502", mask: "9999 9999" },
  { name: "Haiti", iso: "HT", dialCode: "+509", mask: "9999 9999" },
  { name: "Honduras", iso: "HN", dialCode: "+504", mask: "9999 9999" },
  { name: "Jamaica", iso: "JM", dialCode: "+1876", mask: "999-9999" },
  { name: "Nicaragua", iso: "NI", dialCode: "+505", mask: "9999 9999" },
  { name: "Panama", iso: "PA", dialCode: "+507", mask: "9999 9999" },
  { name: "Puerto Rico", iso: "PR", dialCode: "+1787", mask: "999-9999" },
  { name: "Trinidad and Tobago", iso: "TT", dialCode: "+1868", mask: "999-9999" },

  // South America
  { name: "Argentina", iso: "AR", dialCode: "+54", mask: "99 9999 9999" },
  { name: "Bolivia", iso: "BO", dialCode: "+591", mask: "9 9999999" },
  { name: "Brazil", iso: "BR", dialCode: "+55", mask: "(99) 99999-9999" },
  { name: "Chile", iso: "CL", dialCode: "+56", mask: "9 9999 9999" },
  { name: "Colombia", iso: "CO", dialCode: "+57", mask: "999 999 9999" },
  { name: "Ecuador", iso: "EC", dialCode: "+593", mask: "99 999 9999" },
  { name: "Guyana", iso: "GY", dialCode: "+592", mask: "999 9999" },
  { name: "Paraguay", iso: "PY", dialCode: "+595", mask: "999 999999" },
  { name: "Peru", iso: "PE", dialCode: "+51", mask: "999 999 999" },
  { name: "Suriname", iso: "SR", dialCode: "+597", mask: "999 9999" },
  { name: "Uruguay", iso: "UY", dialCode: "+598", mask: "9 999 9999" },
  { name: "Venezuela", iso: "VE", dialCode: "+58", mask: "999 999 9999" },

  // Western Europe
  { name: "Austria", iso: "AT", dialCode: "+43", mask: "999 9999999" },
  { name: "Belgium", iso: "BE", dialCode: "+32", mask: "999 99 99 99" },
  { name: "Denmark", iso: "DK", dialCode: "+45", mask: "99 99 99 99" },
  { name: "Finland", iso: "FI", dialCode: "+358", mask: "99 999 9999" },
  { name: "France", iso: "FR", dialCode: "+33", mask: "9 99 99 99 99" },
  { name: "Germany", iso: "DE", dialCode: "+49", mask: "999 99999999" },
  { name: "Greece", iso: "GR", dialCode: "+30", mask: "999 999 9999" },
  { name: "Iceland", iso: "IS", dialCode: "+354", mask: "999 9999" },
  { name: "Ireland", iso: "IE", dialCode: "+353", mask: "99 999 9999" },
  { name: "Italy", iso: "IT", dialCode: "+39", mask: "999 999 9999" },
  { name: "Luxembourg", iso: "LU", dialCode: "+352", mask: "999 999 999" },
  { name: "Malta", iso: "MT", dialCode: "+356", mask: "9999 9999" },
  { name: "Netherlands", iso: "NL", dialCode: "+31", mask: "99 999 9999" },
  { name: "Norway", iso: "NO", dialCode: "+47", mask: "999 99 999" },
  { name: "Portugal", iso: "PT", dialCode: "+351", mask: "999 999 999" },
  { name: "Spain", iso: "ES", dialCode: "+34", mask: "999 999 999" },
  { name: "Sweden", iso: "SE", dialCode: "+46", mask: "99 999 99 99" },
  { name: "Switzerland", iso: "CH", dialCode: "+41", mask: "99 999 99 99" },
  { name: "United Kingdom", iso: "GB", dialCode: "+44", mask: "9999 999999" },

  // Eastern Europe
  { name: "Albania", iso: "AL", dialCode: "+355", mask: "99 999 9999" },
  { name: "Belarus", iso: "BY", dialCode: "+375", mask: "99 999 99 99" },
  { name: "Bosnia and Herzegovina", iso: "BA", dialCode: "+387", mask: "99 999 999" },
  { name: "Bulgaria", iso: "BG", dialCode: "+359", mask: "99 999 9999" },
  { name: "Croatia", iso: "HR", dialCode: "+385", mask: "99 999 9999" },
  { name: "Czech Republic", iso: "CZ", dialCode: "+420", mask: "999 999 999" },
  { name: "Estonia", iso: "EE", dialCode: "+372", mask: "9999 9999" },
  { name: "Georgia", iso: "GE", dialCode: "+995", mask: "999 99 99 99" },
  { name: "Hungary", iso: "HU", dialCode: "+36", mask: "99 999 9999" },
  { name: "Kosovo", iso: "XK", dialCode: "+383", mask: "99 999 999" },
  { name: "Latvia", iso: "LV", dialCode: "+371", mask: "99 999 999" },
  { name: "Lithuania", iso: "LT", dialCode: "+370", mask: "999 99999" },
  { name: "Moldova", iso: "MD", dialCode: "+373", mask: "99 999 999" },
  { name: "Montenegro", iso: "ME", dialCode: "+382", mask: "99 999 999" },
  { name: "North Macedonia", iso: "MK", dialCode: "+389", mask: "99 999 999" },
  { name: "Poland", iso: "PL", dialCode: "+48", mask: "999 999 999" },
  { name: "Romania", iso: "RO", dialCode: "+40", mask: "999 999 999" },
  { name: "Russia", iso: "RU", dialCode: "+7", mask: "(999) 999-99-99" },
  { name: "Serbia", iso: "RS", dialCode: "+381", mask: "99 999 9999" },
  { name: "Slovakia", iso: "SK", dialCode: "+421", mask: "999 999 999" },
  { name: "Slovenia", iso: "SI", dialCode: "+386", mask: "99 999 999" },
  { name: "Turkey", iso: "TR", dialCode: "+90", mask: "999 999 9999" },
  { name: "Ukraine", iso: "UA", dialCode: "+380", mask: "99 999 9999" },

  // Middle East
  { name: "Bahrain", iso: "BH", dialCode: "+973", mask: "9999 9999" },
  { name: "Iraq", iso: "IQ", dialCode: "+964", mask: "999 999 9999" },
  { name: "Iran", iso: "IR", dialCode: "+98", mask: "999 999 9999" },
  { name: "Israel", iso: "IL", dialCode: "+972", mask: "99 999 9999" },
  { name: "Jordan", iso: "JO", dialCode: "+962", mask: "9 9999 9999" },
  { name: "Kuwait", iso: "KW", dialCode: "+965", mask: "9999 9999" },
  { name: "Lebanon", iso: "LB", dialCode: "+961", mask: "99 999 999" },
  { name: "Oman", iso: "OM", dialCode: "+968", mask: "9999 9999" },
  { name: "Palestine", iso: "PS", dialCode: "+970", mask: "999 999 999" },
  { name: "Qatar", iso: "QA", dialCode: "+974", mask: "9999 9999" },
  { name: "Saudi Arabia", iso: "SA", dialCode: "+966", mask: "99 999 9999" },
  { name: "Syria", iso: "SY", dialCode: "+963", mask: "999 999 999" },
  { name: "United Arab Emirates", iso: "AE", dialCode: "+971", mask: "99 999 9999" },
  { name: "Yemen", iso: "YE", dialCode: "+967", mask: "999 999 999" },

  // East Asia
  { name: "China", iso: "CN", dialCode: "+86", mask: "999 9999 9999" },
  { name: "Hong Kong", iso: "HK", dialCode: "+852", mask: "9999 9999" },
  { name: "Japan", iso: "JP", dialCode: "+81", mask: "99 9999 9999" },
  { name: "Macau", iso: "MO", dialCode: "+853", mask: "9999 9999" },
  { name: "Mongolia", iso: "MN", dialCode: "+976", mask: "9999 9999" },
  { name: "South Korea", iso: "KR", dialCode: "+82", mask: "99 9999 9999" },
  { name: "Taiwan", iso: "TW", dialCode: "+886", mask: "999 999 999" },

  // Southeast Asia
  { name: "Brunei", iso: "BN", dialCode: "+673", mask: "999 9999" },
  { name: "Cambodia", iso: "KH", dialCode: "+855", mask: "99 999 999" },
  { name: "Indonesia", iso: "ID", dialCode: "+62", mask: "999 9999 9999" },
  { name: "Laos", iso: "LA", dialCode: "+856", mask: "99 99 999 999" },
  { name: "Malaysia", iso: "MY", dialCode: "+60", mask: "99 999 9999" },
  { name: "Myanmar", iso: "MM", dialCode: "+95", mask: "9 999 9999" },
  { name: "Philippines", iso: "PH", dialCode: "+63", mask: "999 999 9999" },
  { name: "Singapore", iso: "SG", dialCode: "+65", mask: "9999 9999" },
  { name: "Thailand", iso: "TH", dialCode: "+66", mask: "99 999 9999" },
  { name: "Timor-Leste", iso: "TL", dialCode: "+670", mask: "9999 9999" },
  { name: "Vietnam", iso: "VN", dialCode: "+84", mask: "99 999 9999" },

  // South Asia
  { name: "Afghanistan", iso: "AF", dialCode: "+93", mask: "99 999 9999" },
  { name: "Bangladesh", iso: "BD", dialCode: "+880", mask: "9999 999999" },
  { name: "Bhutan", iso: "BT", dialCode: "+975", mask: "99 99 99 99" },
  { name: "India", iso: "IN", dialCode: "+91", mask: "99999 99999" },
  { name: "Maldives", iso: "MV", dialCode: "+960", mask: "999 9999" },
  { name: "Nepal", iso: "NP", dialCode: "+977", mask: "99 9999 9999" },
  { name: "Pakistan", iso: "PK", dialCode: "+92", mask: "999 9999999" },
  { name: "Sri Lanka", iso: "LK", dialCode: "+94", mask: "99 999 9999" },

  // Central Asia
  { name: "Kazakhstan", iso: "KZ", dialCode: "+7", mask: "(999) 999-99-99" },
  { name: "Kyrgyzstan", iso: "KG", dialCode: "+996", mask: "999 999 999" },
  { name: "Tajikistan", iso: "TJ", dialCode: "+992", mask: "99 999 9999" },
  { name: "Turkmenistan", iso: "TM", dialCode: "+993", mask: "99 999999" },
  { name: "Uzbekistan", iso: "UZ", dialCode: "+998", mask: "99 999 9999" },

  // North Africa
  { name: "Algeria", iso: "DZ", dialCode: "+213", mask: "999 99 99 99" },
  { name: "Egypt", iso: "EG", dialCode: "+20", mask: "99 9999 9999" },
  { name: "Libya", iso: "LY", dialCode: "+218", mask: "99 999 9999" },
  { name: "Morocco", iso: "MA", dialCode: "+212", mask: "99 999 9999" },
  { name: "Sudan", iso: "SD", dialCode: "+249", mask: "99 999 9999" },
  { name: "Tunisia", iso: "TN", dialCode: "+216", mask: "99 999 999" },

  // West Africa
  { name: "Cameroon", iso: "CM", dialCode: "+237", mask: "9 99 99 99 99" },
  { name: "Côte d'Ivoire", iso: "CI", dialCode: "+225", mask: "99 99 99 99" },
  { name: "Ghana", iso: "GH", dialCode: "+233", mask: "99 999 9999" },
  { name: "Nigeria", iso: "NG", dialCode: "+234", mask: "999 999 9999" },
  { name: "Senegal", iso: "SN", dialCode: "+221", mask: "99 999 9999" },

  // East Africa
  { name: "Ethiopia", iso: "ET", dialCode: "+251", mask: "99 999 9999" },
  { name: "Kenya", iso: "KE", dialCode: "+254", mask: "999 999999" },
  { name: "Rwanda", iso: "RW", dialCode: "+250", mask: "999 999 999" },
  { name: "Tanzania", iso: "TZ", dialCode: "+255", mask: "999 999 999" },
  { name: "Uganda", iso: "UG", dialCode: "+256", mask: "999 999999" },

  // Southern Africa
  { name: "Angola", iso: "AO", dialCode: "+244", mask: "999 999 999" },
  { name: "Botswana", iso: "BW", dialCode: "+267", mask: "99 999 999" },
  { name: "Mozambique", iso: "MZ", dialCode: "+258", mask: "99 999 9999" },
  { name: "Namibia", iso: "NA", dialCode: "+264", mask: "99 999 9999" },
  { name: "South Africa", iso: "ZA", dialCode: "+27", mask: "99 999 9999" },
  { name: "Zambia", iso: "ZM", dialCode: "+260", mask: "99 999 9999" },
  { name: "Zimbabwe", iso: "ZW", dialCode: "+263", mask: "99 999 9999" },

  // Oceania
  { name: "Australia", iso: "AU", dialCode: "+61", mask: "9 9999 9999" },
  { name: "Fiji", iso: "FJ", dialCode: "+679", mask: "999 9999" },
  { name: "New Zealand", iso: "NZ", dialCode: "+64", mask: "99 999 9999" },
  { name: "Papua New Guinea", iso: "PG", dialCode: "+675", mask: "999 9999" },

  // Other
  { name: "Armenia", iso: "AM", dialCode: "+374", mask: "99 999999" },
  { name: "Azerbaijan", iso: "AZ", dialCode: "+994", mask: "99 999 99 99" },
  { name: "Cyprus", iso: "CY", dialCode: "+357", mask: "99 999999" },
];

export const DEFAULT_PHONE_COUNTRY = PHONE_COUNTRIES[0]; // United States

export function findPhoneCountry(iso: string): PhoneCountry | undefined {
  return PHONE_COUNTRIES.find((c) => c.iso === iso);
}

export function findPhoneCountryByDialCode(dialCode: string): PhoneCountry | undefined {
  return PHONE_COUNTRIES.find((c) => c.dialCode === dialCode);
}
