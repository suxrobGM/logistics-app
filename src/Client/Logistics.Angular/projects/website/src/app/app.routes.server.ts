import { RenderMode, type ServerRoute } from "@angular/ssr";

export const serverRoutes: ServerRoute[] = [
  // Static pages - prerender for SEO
  { path: "", renderMode: RenderMode.Prerender },
  { path: "about", renderMode: RenderMode.Prerender },
  { path: "careers", renderMode: RenderMode.Prerender },
  { path: "contact", renderMode: RenderMode.Prerender },
  { path: "privacy", renderMode: RenderMode.Prerender },
  { path: "terms", renderMode: RenderMode.Prerender },
  { path: "cookies", renderMode: RenderMode.Prerender },

  // Blog posts - SSR for dynamic content
  { path: "blog", renderMode: RenderMode.Server },
  { path: "blog/:slug", renderMode: RenderMode.Server },

  // Fallback
  { path: "**", renderMode: RenderMode.Server },
];
