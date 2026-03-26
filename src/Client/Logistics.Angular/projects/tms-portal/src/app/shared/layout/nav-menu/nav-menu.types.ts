import type { TenantFeature } from "@logistics/shared/api";

export interface NavSection {
  id: string;
  label: string;
  items: NavItem[];
  pinToBottom?: boolean;
}

export interface NavItem {
  id: string;
  label: string;
  icon?: string;
  route?: string;
  feature?: TenantFeature;
  children?: NavItem[];
  badge?: () => number | null;
}
