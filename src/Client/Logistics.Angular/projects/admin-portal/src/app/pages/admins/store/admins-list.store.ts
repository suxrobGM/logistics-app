import { getAdmins, type UserDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the admins list page (users holding an app-level role).
 */
export const AdminsListStore = createListStore<UserDto>(getAdmins, {
  defaultSortField: "Email",
  defaultPageSize: 10,
});
