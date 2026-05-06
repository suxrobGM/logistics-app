import type { Region } from "../api";

const US_COUNTRIES = ["US"];

const EUROPEAN_COUNTRIES = [
  // EU-27
  "AT",
  "BE",
  "BG",
  "HR",
  "CY",
  "CZ",
  "DK",
  "EE",
  "FI",
  "FR",
  "DE",
  "GR",
  "HU",
  "IE",
  "IT",
  "LV",
  "LT",
  "LU",
  "MT",
  "NL",
  "PL",
  "PT",
  "RO",
  "SK",
  "SI",
  "ES",
  "SE",
  // EEA / EFTA
  "IS",
  "LI",
  "NO",
  "CH",
  // United Kingdom
  "GB",
  // Western Balkans
  "AL",
  "BA",
  "ME",
  "MK",
  "RS",
  "XK",
  // Eastern Europe (selected)
  "MD",
  "UA",
  // Microstates
  "AD",
  "MC",
  "SM",
  "VA",
];

// Subset of EUROPEAN_COUNTRIES that are EU-27 member states. Mirrors
// EuVatRules.EuMemberStates in Logistics.Infrastructure.Tax. Reverse-charge
// for cross-border B2B applies only to this set.
const EU_MEMBER_STATES = new Set([
  "AT",
  "BE",
  "BG",
  "HR",
  "CY",
  "CZ",
  "DK",
  "EE",
  "FI",
  "FR",
  "DE",
  "GR",
  "HU",
  "IE",
  "IT",
  "LV",
  "LT",
  "LU",
  "MT",
  "NL",
  "PL",
  "PT",
  "RO",
  "SK",
  "SI",
  "ES",
  "SE",
]);

/**
 * Returns the ISO-3166-1 alpha-2 country codes that belong to the given region.
 * Mirrors the backend `RegionCountries` set in `Logistics.Domain`.
 */
export function regionAllowedCountries(region: Region | null | undefined): readonly string[] {
  return region === "eu" ? EUROPEAN_COUNTRIES : US_COUNTRIES;
}

/**
 * True when the country is an EU-27 member state. Used by tax forms to decide
 * whether a customer Tax ID is required for VAT reverse-charge eligibility.
 */
export function isEuCountry(countryCode: string | null | undefined): boolean {
  return !!countryCode && EU_MEMBER_STATES.has(countryCode.toUpperCase());
}
