import type { Routes } from "@angular/router";
import { authGuard } from "@/core/auth";

export const aiSettingsRoutes: Routes = [
  {
    path: "",
    loadComponent: () => import("./general/ai-settings").then((m) => m.AiSettings),
    canActivate: [authGuard],
    data: {
      breadcrumb: "AI Settings",
    },
  },
];
