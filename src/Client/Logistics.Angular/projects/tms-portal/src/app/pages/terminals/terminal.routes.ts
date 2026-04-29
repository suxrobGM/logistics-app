import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { TerminalAdd } from "./terminal-add/terminal-add";
import { TerminalEdit } from "./terminal-edit/terminal-edit";
import { TerminalsList } from "./terminals-list/terminals-list";

export const terminalRoutes: Routes = [
  {
    path: "",
    component: TerminalsList,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Terminal.View,
    },
  },
  {
    path: "add",
    component: TerminalAdd,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permission.Terminal.Manage,
    },
  },
  {
    path: ":id",
    component: TerminalEdit,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Details",
      permission: Permission.Terminal.View,
    },
  },
];
