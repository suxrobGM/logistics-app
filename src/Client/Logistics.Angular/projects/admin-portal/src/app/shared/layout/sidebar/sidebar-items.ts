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
    items: [{ label: "Dashboard", icon: "pi pi-home", route: "/home" }],
  },
  {
    label: "Tenant Management",
    items: [
      { label: "Tenants", icon: "pi pi-building", route: "/tenants", permission: Permission.Tenant.View },
      { label: "Features", icon: "pi pi-th-large", route: "/features", permission: Permission.Tenant.Manage },
      { label: "AI Quotas", icon: "pi pi-sparkles", route: "/tenants/quotas", permission: Permission.Tenant.Manage },
    ],
  },
  {
    label: "Billing",
    items: [
      { label: "Subscription Plans", icon: "pi pi-credit-card", route: "/subscription-plans" },
      { label: "Subscriptions", icon: "pi pi-users", route: "/subscriptions", permission: Permission.Tenant.View },
    ],
  },
  {
    label: "Users & Content",
    items: [
      { label: "Users", icon: "pi pi-user", route: "/users", permission: Permission.User.Manage },
      { label: "Blog Posts", icon: "pi pi-file-edit", route: "/blog-posts", permission: Permission.BlogPost.Manage },
    ],
  },
  {
    label: "Inbox",
    items: [
      { label: "Demo Requests", icon: "pi pi-inbox", route: "/demo-requests" },
      { label: "Contact Submissions", icon: "pi pi-envelope", route: "/contact-submissions" },
    ],
  },
];
