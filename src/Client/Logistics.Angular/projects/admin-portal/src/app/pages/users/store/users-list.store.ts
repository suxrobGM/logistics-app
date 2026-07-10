import { getUsers, type UserDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the users list page.
 */
export const UsersListStore = createListStore<UserDto>(getUsers, {
  defaultSortField: "Email",
  defaultPageSize: 10,
});
