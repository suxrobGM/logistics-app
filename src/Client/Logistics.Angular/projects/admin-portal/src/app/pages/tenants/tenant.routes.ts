import type { Routes } from "@angular/router";
import { TenantAdd } from "./tenant-add/tenant-add";
import { TenantEdit } from "./tenant-edit/tenant-edit";
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
    path: ":id/edit",
    component: TenantEdit,
    data: {
      breadcrumb: "Edit",
    },
  },
];
