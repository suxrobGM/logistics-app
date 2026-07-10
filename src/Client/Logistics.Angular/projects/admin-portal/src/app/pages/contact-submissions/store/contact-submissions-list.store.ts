import { getContactSubmissions, type ContactSubmissionDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the contact submissions list page.
 */
export const ContactSubmissionsListStore = createListStore<ContactSubmissionDto>(
  getContactSubmissions,
  {
    defaultSortField: "-CreatedAt",
    defaultPageSize: 10,
  },
);
