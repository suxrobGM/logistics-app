import type { Region } from "@logistics/shared/api";

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

/**
 * Returns the ISO-3166-1 alpha-2 country codes that belong to the given region.
 * Mirrors the backend `RegionCountries` set in `Logistics.Domain`.
 */
export function regionAllowedCountries(region: Region | null | undefined): readonly string[] {
  return region === "eu" ? EUROPEAN_COUNTRIES : US_COUNTRIES;
}
