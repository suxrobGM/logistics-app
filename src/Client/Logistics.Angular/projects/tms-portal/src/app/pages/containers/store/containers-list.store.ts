import { getContainers, type ContainerDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the containers list page.
 */
export const ContainersListStore = createListStore<ContainerDto>(getContainers, {
  defaultSortField: "-CreatedAt",
  defaultPageSize: 10,
});
