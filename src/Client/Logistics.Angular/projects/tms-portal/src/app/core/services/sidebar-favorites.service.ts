import { isPlatformBrowser } from "@angular/common";
import { Injectable, PLATFORM_ID, inject, signal } from "@angular/core";
import { UserRole } from "@logistics/shared";

const STORAGE_KEY = "tms-sidebar-favorites";
const MAX_FAVORITES = 6;

const ROLE_DEFAULTS: Record<string, string[]> = {
  [UserRole.Driver]: ["home", "messages"],
  [UserRole.Dispatcher]: ["loads", "trips", "messages"],
  [UserRole.Manager]: ["home", "loads", "reports"],
  [UserRole.Owner]: ["dashboard", "loads", "reports", "expenses"],
};

/**
 * Service to manage user's favorite sidebar items.
 * Stores a list of favorite item IDs, persisted in localStorage.
 */
@Injectable({ providedIn: "root" })
export class SidebarFavoritesService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly _favoriteIds = signal<string[]>([]);

  public readonly favoriteIds = this._favoriteIds.asReadonly();

  constructor() {
    this.loadFromStorage();
  }

  initWithRole(role: string): void {
    // Only set defaults if no stored favorites exist
    if (this._favoriteIds().length === 0) {
      const defaults = ROLE_DEFAULTS[role] ?? ROLE_DEFAULTS[UserRole.Owner];
      this._favoriteIds.set(defaults);
      this.persist();
    }
  }

  add(itemId: string): void {
    const current = this._favoriteIds();
    if (current.includes(itemId) || current.length >= MAX_FAVORITES) return;
    this._favoriteIds.set([...current, itemId]);
    this.persist();
  }

  remove(itemId: string): void {
    this._favoriteIds.update((ids) => ids.filter((id) => id !== itemId));
    this.persist();
  }

  isFavorite(itemId: string): boolean {
    return this._favoriteIds().includes(itemId);
  }

  isFull(): boolean {
    return this._favoriteIds().length >= MAX_FAVORITES;
  }

  private loadFromStorage(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      try {
        const parsed = JSON.parse(stored);
        if (Array.isArray(parsed)) {
          this._favoriteIds.set(parsed.slice(0, MAX_FAVORITES));
        }
      } catch {
        // Ignore corrupt data
      }
    }
  }

  private persist(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this._favoriteIds()));
    }
  }
}
