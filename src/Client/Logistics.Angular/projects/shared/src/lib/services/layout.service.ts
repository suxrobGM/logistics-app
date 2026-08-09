import { computed, Injectable, signal } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { fromEvent } from "rxjs";
import { debounceTime, map, startWith } from "rxjs/operators";
import { persistValue, readStoredBoolean } from "../utils/local-storage";

const MOBILE_BREAKPOINT = 768;
/** Tailwind `lg`. Below it there is no room for a multi-column layout beside the sidebar. */
const COMPACT_BREAKPOINT = 1024;
const NavRailCollapsedKey = "layout.nav-rail-collapsed";

@Injectable({ providedIn: "root" })
export class LayoutService {
  private readonly windowWidth = signal(typeof window !== "undefined" ? window.innerWidth : 1024);

  /** True when viewport is below 768px */
  public readonly isMobile = computed(() => this.windowWidth() < MOBILE_BREAKPOINT);

  /** True below 1024px - narrower than a sidebar plus a multi-column layout needs. */
  public readonly isCompact = computed(() => this.windowWidth() < COMPACT_BREAKPOINT);

  /** Mobile navigation drawer visibility */
  public readonly mobileMenuOpen = signal(false);

  /**
   * The desktop nav rail, collapsed to icons. Named for the shell rather than "sidebar", which
   * pages also use for their own panels. Survives a reload; the mobile drawer does not.
   */
  public readonly navRailCollapsed = signal(readStoredBoolean(NavRailCollapsedKey));

  constructor() {
    if (typeof window !== "undefined") {
      fromEvent(window, "resize")
        .pipe(
          debounceTime(100),
          map(() => window.innerWidth),
          startWith(window.innerWidth),
          takeUntilDestroyed(),
        )
        .subscribe((width) => {
          this.windowWidth.set(width);
          if (width >= MOBILE_BREAKPOINT) {
            this.mobileMenuOpen.set(false);
          }
        });
    }
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((open) => !open);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  toggleNavRail(): void {
    const collapsed = !this.navRailCollapsed();
    this.navRailCollapsed.set(collapsed);
    persistValue(NavRailCollapsedKey, collapsed);
  }
}
