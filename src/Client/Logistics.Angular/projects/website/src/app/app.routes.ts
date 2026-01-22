import { type Routes } from "@angular/router";

export const routes: Routes = [
  {
    path: "",
    loadComponent: () => import("./pages/home/home").then((m) => m.Home),
  },
  {
    path: "**",
    redirectTo: "",
  },
];
