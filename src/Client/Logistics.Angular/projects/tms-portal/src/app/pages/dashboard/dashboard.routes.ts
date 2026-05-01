import type { Routes } from "@angular/router";

/**
 * The /dashboard page was merged into /home (role-aware panel registry).
 * This route preserves bookmarks/deep-links by redirecting to /home.
 */
export const dashboardRoutes: Routes = [{ path: "", redirectTo: "/home", pathMatch: "full" }];
