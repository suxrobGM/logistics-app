/**
 * Country-specific label for the State / Region / Province field on
 * <ui-address-form>. Looked up by ISO-3166-1 alpha-2 country code.
 * Countries not listed get the generic fallback "State / Region".
 */
export const STATE_FIELD_LABELS: Record<string, string> = {
  US: "State",
  CA: "Province",
  AU: "State",
  MX: "State",
  BR: "State",
  IN: "State",
  DE: "Bundesland",
  AT: "Bundesland",
  CH: "Canton",
  IT: "Province",
  ES: "Province",
  FR: "Region",
  GB: "County",
  NL: "Province",
  BE: "Province",
  IE: "County",
  PL: "Voivodeship",
  PT: "District",
};

export const DEFAULT_STATE_FIELD_LABEL = "State / Region";
