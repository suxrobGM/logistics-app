import {
  DestroyRef,
  Directive,
  ElementRef,
  afterNextRender,
  effect,
  inject,
  input,
} from "@angular/core";
import type { Map } from "mapbox-gl";

/**
 * Directive that handles Mapbox GL map resizing when container dimensions change.
 *
 * Mapbox GL maps need to be explicitly told when their container size changes
 * (except for window resize which ngx-mapbox-gl handles automatically).
 * This directive uses ResizeObserver to detect container size changes and
 * calls map.resize() with debouncing.
 *
 * Usage:
 * ```html
 * <div [appMapResize]="mapInstance()">
 *   <mgl-map (mapLoad)="onMapLoad($event)">...</mgl-map>
 * </div>
 * ```
 */
@Directive({
  selector: "[appMapResize]",
  standalone: true,
})
export class MapResizeDirective {
  private readonly el = inject(ElementRef);
  private readonly destroyRef = inject(DestroyRef);

  private resizeObserver: ResizeObserver | null = null;
  private resizeTimeoutId: ReturnType<typeof setTimeout> | null = null;
  private mapInstance: Map | null = null;

  /** The Mapbox GL map instance to resize */
  public readonly map = input<Map | null>(null, { alias: "appMapResize" });

  constructor() {
    afterNextRender(() => {
      this.setupResizeObserver();
    });

    effect(() => {
      const map = this.map();
      if (map) {
        this.mapInstance = map;
        this.triggerInitialResize(map);
      }
    });
  }

  /**
   * Triggers multiple resize calls after map load to handle various timing scenarios:
   * - Immediate: in case container is already properly sized
   * - Next frame: after CSS has been applied
   * - Delayed: after gridster or other layout systems have finished calculating
   */
  private triggerInitialResize(map: Map): void {
    map.resize();

    requestAnimationFrame(() => {
      map.resize();
    });

    setTimeout(() => {
      map.resize();
    }, 150);
  }

  private setupResizeObserver(): void {
    const element = this.el.nativeElement as HTMLElement;

    this.resizeObserver = new ResizeObserver(() => {
      this.debouncedResize();
    });

    this.resizeObserver.observe(element);

    this.destroyRef.onDestroy(() => {
      this.resizeObserver?.disconnect();
      if (this.resizeTimeoutId) {
        clearTimeout(this.resizeTimeoutId);
      }
    });
  }

  /**
   * Debounced resize call to prevent excessive updates during drag operations.
   * Uses 100ms debounce which balances responsiveness with performance.
   */
  private debouncedResize(): void {
    if (this.resizeTimeoutId) {
      clearTimeout(this.resizeTimeoutId);
    }

    this.resizeTimeoutId = setTimeout(() => {
      this.mapInstance?.resize();
    }, 100);
  }
}
