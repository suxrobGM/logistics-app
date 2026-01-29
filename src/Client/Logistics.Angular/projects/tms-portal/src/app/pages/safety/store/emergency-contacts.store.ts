import { getEmergencyContacts } from "@logistics/shared/api";
import type { EmergencyContactDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the emergency contacts list page.
 */
export const EmergencyContactsStore = createListStore<EmergencyContactDto>(getEmergencyContacts, {
  defaultSortField: "Priority",
  defaultPageSize: 10,
});
