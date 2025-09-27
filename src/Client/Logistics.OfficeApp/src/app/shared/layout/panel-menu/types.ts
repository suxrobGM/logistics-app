import { MenuItem as PrimeNgMenuItem } from "primeng/api";

export interface MenuItem extends PrimeNgMenuItem {
  route?: string;
}
