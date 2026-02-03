import { Component, computed, inject, input } from "@angular/core";
import type { TenantFeature } from "../../api/generated/models/tenant-feature";
import { FeatureService } from "../../services/feature.service";

/**
 * A reusable component that conditionally renders its content based on feature availability.
 *
 * Usage:
 * ```html
 * <lib-feature-guard [feature]="'eld'">
 *   <button>Configure ELD</button>
 * </lib-feature-guard>
 * ```
 *
 * The consuming application must provide the FEATURE_PROVIDER token:
 * ```typescript
 * providers: [
 *   { provide: FEATURE_PROVIDER, useFactory: () => inject(TmsFeatureProvider) }
 * ]
 * ```
 */
@Component({
  selector: "lib-feature-guard",
  template: `
    @if (isEnabled()) {
      <ng-content />
    }
  `,
})
export class FeatureGuard {
  private readonly featureService = inject(FeatureService);

  /**
   * The feature to check.
   */
  public readonly feature = input.required<TenantFeature>();

  /**
   * Whether the feature is enabled.
   */
  protected readonly isEnabled = computed(() => {
    return this.featureService.isEnabled(this.feature());
  });
}
