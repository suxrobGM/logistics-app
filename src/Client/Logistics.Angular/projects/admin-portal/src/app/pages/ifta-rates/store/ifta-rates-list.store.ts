import { getIftaTaxRates, type IftaTaxRateDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the IFTA tax rates list page.
 */
export const IftaRatesListStore = createListStore<IftaTaxRateDto>(getIftaTaxRates, {
  defaultSortField: "Jurisdiction.CountryCode",
  defaultPageSize: 25,
});
