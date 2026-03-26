import { Injectable, computed, signal } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { fromEvent } from "rxjs";
import { debounceTime, map, startWith } from "rxjs/operators";

const MOBILE_BREAKPOINT = 768;

@Injectable({ providedIn: "root" })
export class LayoutService {
  private readonly windowWidth = signal(typeof window !== "undefined" ? window.innerWidth : 1024);

  /** True when viewport is below 768px */
  public readonly isMobile = computed(() => this.windowWidth() < MOBILE_BREAKPOINT);

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
