import { isPlatformBrowser } from "@angular/common";
import { Injectable, PLATFORM_ID, inject, signal } from "@angular/core";
import { NavigationEnd, Router } from "@angular/router";
import { filter } from "rxjs";

const STORAGE_KEY = "tms-recent-pages";
const MAX_RECENT = 5;

export interface RecentPage {
  route: string;
  label: string;
  timestamp: number;
}

/**
 * Service to track recently visited pages for quick access.
 * Stores a list of recent routes with display labels, persisted in localStorage.
 * The sidebar component can provide route -> label mappings for better display names.
 */
@Injectable({ providedIn: "root" })
export class RecentPagesService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly router = inject(Router);
  private readonly _recentPages = signal<RecentPage[]>([]);

  public readonly recentPages = this._recentPages.asReadonly();

  /** Label map to resolve route -> display name. Set externally by sidebar. */
  private routeLabels = new Map<string, string>();

  constructor() {
    this.loadFromStorage();
    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe((event) => this.trackNavigation(event.urlAfterRedirects));
  }

  setRouteLabels(labels: Map<string, string>): void {
    this.routeLabels = labels;
  }

  private trackNavigation(url: string): void {
    // Strip query params and fragments for matching
    const route = url.split("?")[0].split("#")[0];

    // Skip auth/error routes
    if (route === "/" || route === "/unauthorized" || route === "/404") {
      return;
    }

    const label = this.routeLabels.get(route) ?? this.inferLabel(route);

    this._recentPages.update((pages) => {
      const filtered = pages.filter((p) => p.route !== route);
      return [{ route, label, timestamp: Date.now() }, ...filtered].slice(0, MAX_RECENT);
    });
    this.persist();
  }

  private inferLabel(route: string): string {
    const segments = route.split("/").filter(Boolean);
    const last = segments[segments.length - 1] ?? "Page";
    return last.replace(/-/g, " ").replace(/\b\w/g, (c) => c.toUpperCase());
  }

  private loadFromStorage(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      try {
        const parsed = JSON.parse(stored);
        if (Array.isArray(parsed)) {
          this._recentPages.set(parsed.slice(0, MAX_RECENT));
        }
      } catch {
        // Ignore corrupt data
      }
    }
  }

  private persist(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this._recentPages()));
    }
  }
}
