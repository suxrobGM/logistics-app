import type { TenantFeature } from "@logistics/shared/api";
import type { MenuItem as PrimeNgMenuItem } from "primeng/api";

export interface MenuItem extends PrimeNgMenuItem {
  route?: string;
  /**
   * The feature this menu item requires. If the feature is disabled, the item will be hidden.
   */
  feature?: TenantFeature;
}
