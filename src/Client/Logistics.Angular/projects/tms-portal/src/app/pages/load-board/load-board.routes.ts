import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { LoadBoardDashboardComponent } from "./load-board-dashboard/load-board-dashboard";
import { LoadBoardProvidersComponent } from "./load-board-providers/load-board-providers";
import { LoadBoardSearchComponent } from "./load-board-search/load-board-search";
import { PostedTrucksComponent } from "./posted-trucks/posted-trucks";

export const loadBoardRoutes: Routes = [
  {
    path: "",
    component: LoadBoardDashboardComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.LoadBoard.View,
    },
  },
  {
    path: "providers",
    component: LoadBoardProvidersComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Providers",
      permission: Permission.LoadBoard.Manage,
    },
  },
  {
    path: "search",
    component: LoadBoardSearchComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Search Loads",
      permission: Permission.LoadBoard.Search,
    },
  },
  {
    path: "posted-trucks",
    component: PostedTrucksComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Posted Trucks",
      permission: Permission.LoadBoard.Post,
    },
  },
];
