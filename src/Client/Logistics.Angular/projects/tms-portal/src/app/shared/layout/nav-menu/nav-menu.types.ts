import type { TenantFeature } from "@logistics/shared/api";
import type { IconName } from "@logistics/shared/ui";

export interface NavSection {
  id: string;
  label: string;
  items: NavItem[];
  pinToBottom?: boolean;
}

export interface NavItem {
  id: string;
  label: string;
  /** Canonical `<ui-icon name>`. Typed, so a bad name is a compile error, not a blank glyph. */
  icon?: IconName;
  route?: string;
  feature?: TenantFeature;
  children?: NavItem[];
  badge?: () => number | null;
}
