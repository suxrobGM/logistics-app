import { getContactSubmissions } from "@logistics/shared/api";
import type { ContactSubmissionDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the contact submissions list page.
 */
export const ContactSubmissionsListStore = createListStore<ContactSubmissionDto>(getContactSubmissions, {
  defaultSortField: "-CreatedAt",
  defaultPageSize: 10,
});
