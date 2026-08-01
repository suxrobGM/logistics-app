import { computed, Injectable, signal } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { fromEvent } from "rxjs";
import { debounceTime, map, startWith } from "rxjs/operators";

const MOBILE_BREAKPOINT = 768;
/** Tailwind `lg`. Below it there is no room for a multi-column layout beside the sidebar. */
const COMPACT_BREAKPOINT = 1024;

@Injectable({ providedIn: "root" })
export class LayoutService {
  private readonly windowWidth = signal(typeof window !== "undefined" ? window.innerWidth : 1024);

  /** True when viewport is below 768px */
  public readonly isMobile = computed(() => this.windowWidth() < MOBILE_BREAKPOINT);

  /** True below 1024px - narrower than a sidebar plus a multi-column layout needs. */
  public readonly isCompact = computed(() => this.windowWidth() < COMPACT_BREAKPOINT);

  /** Mobile navigation drawer visibility */
  public readonly mobileMenuOpen = signal(false);

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
}
