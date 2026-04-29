import { getTerminals, type TerminalDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the terminals list page.
 */
export const TerminalsListStore = createListStore<TerminalDto>(getTerminals, {
  defaultSortField: "Code",
  defaultPageSize: 10,
});
