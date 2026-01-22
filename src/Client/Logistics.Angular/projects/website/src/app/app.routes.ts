import { type Routes } from "@angular/router";

export const routes: Routes = [
  {
    path: "",
    loadComponent: () => import("./pages/home/home").then((m) => m.Home),
  },
  {
    path: "about",
    loadComponent: () => import("./pages/about/about").then((m) => m.About),
  },
  {
    path: "careers",
    loadComponent: () => import("./pages/careers/careers").then((m) => m.Careers),
  },
  {
    path: "contact",
    loadComponent: () => import("./pages/contact/contact").then((m) => m.Contact),
  },
  {
    path: "blog",
    loadComponent: () => import("./pages/blog/blog").then((m) => m.Blog),
  },
  {
    path: "privacy",
    loadComponent: () => import("./pages/legal/privacy/privacy").then((m) => m.Privacy),
  },
  {
    path: "terms",
    loadComponent: () => import("./pages/legal/terms/terms").then((m) => m.Terms),
  },
  {
    path: "cookies",
    loadComponent: () => import("./pages/legal/cookies/cookies").then((m) => m.Cookies),
  },
  {
    path: "**",
    redirectTo: "",
  },
];
