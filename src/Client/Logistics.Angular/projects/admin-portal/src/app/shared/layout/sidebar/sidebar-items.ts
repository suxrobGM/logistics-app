import { Permission } from "@logistics/shared";

export interface AdminNavSection {
  label: string;
  items: AdminNavItem[];
}

export interface AdminNavItem {
  label: string;
  icon: string;
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
      { label: "Tenants", icon: "building", route: "/tenants", permission: Permission.Tenant.View },
      {
        label: "Features",
        icon: "layout-grid",
        route: "/features",
        permission: Permission.Tenant.Manage,
      },
      {
        label: "AI Quotas",
        icon: "sparkles",
        route: "/tenants/quotas",
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
      {
        label: "Blog Posts",
        icon: "file-pen-line",
        route: "/blog-posts",
        permission: Permission.BlogPost.Manage,
      },
    ],
  },
  {
    label: "Inbox",
    items: [
      { label: "Demo Requests", icon: "inbox", route: "/demo-requests" },
      { label: "Contact Submissions", icon: "mail", route: "/contact-submissions" },
    ],
  },
];
