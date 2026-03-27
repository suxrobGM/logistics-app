import type { Routes } from "@angular/router";
import { TenantAdd } from "./tenant-add/tenant-add";
import { TenantEdit } from "./tenant-edit/tenant-edit";
import { TenantQuotas } from "./tenant-quotas/tenant-quotas";
import { TenantsList } from "./tenants-list/tenants-list";

export const tenantRoutes: Routes = [
  {
    path: "",
    component: TenantsList,
    data: {
      breadcrumb: "Tenants",
    },
  },
  {
    path: "add",
    component: TenantAdd,
    data: {
      breadcrumb: "Add",
    },
  },
  {
    path: "quotas",
    component: TenantQuotas,
    data: {
      breadcrumb: "AI Quotas",
    },
  },
  {
    path: ":id/edit",
    component: TenantEdit,
    data: {
      breadcrumb: "Edit",
    },
  },
];
