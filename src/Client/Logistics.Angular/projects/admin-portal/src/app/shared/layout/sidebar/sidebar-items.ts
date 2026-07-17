import { Permission } from "@logistics/shared";
import type { IconName } from "@logistics/shared/ui";

export interface AdminNavSection {
  label: string;
  items: AdminNavItem[];
}

export interface AdminNavItem {
  label: string;
  /** Canonical `<ui-icon name>`. Typed, so a bad name is a compile error, not a blank glyph. */
  icon: IconName;
  route: string;
  permission?: string;
}

export const sidebarSections: AdminNavSection[] = [
  {
    label: "Overview",
    items: [{ label: "Dashboard", icon: "house", route: "/home" }],
  },
  {
    label: "Tenant Management",
    items: [
      {
        label: "Tenants",
        icon: "building-2",
        route: "/tenants",
        permission: Permission.Tenant.View,
      },
      {
        label: "Features",
        icon: "layout-grid",
        route: "/features",
        permission: Permission.Tenant.Manage,
      },
      {
        label: "AI Settings",
        icon: "bot",
        route: "/ai-settings",
        permission: Permission.Tenant.Manage,
      },
    ],
  },
  {
    label: "Billing",
    items: [
      { label: "Subscription Plans", icon: "credit-card", route: "/subscription-plans" },
      {
        label: "Subscriptions",
        icon: "users",
        route: "/subscriptions",
        permission: Permission.Tenant.View,
      },
    ],
  },
  {
    label: "Users & Content",
    items: [
      { label: "Users", icon: "user", route: "/users", permission: Permission.User.Manage },
      { label: "Admins", icon: "shield", route: "/admins", permission: Permission.AppRole.Manage },
      {
        label: "Blog Posts",
        icon: "file-pen-line",
        route: "/blog-posts",
        permission: Permission.BlogPost.Manage,
      },
    ],
  },
  {
    label: "Compliance",
    items: [
      {
        label: "IFTA Tax Rates",
        icon: "fuel",
        route: "/ifta-rates",
        permission: Permission.IftaRate.View,
      },
    ],
  },
  {
    label: "Inbox",
    items: [
      { label: "Demo Requests", icon: "inbox", route: "/demo-requests" },
      { label: "Contact Submissions", icon: "mail", route: "/contact-submissions" },
      { label: "Data Requests", icon: "shield-check", route: "/data-requests" },
    ],
  },
];
