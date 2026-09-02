import { computed, inject, Injectable, signal } from "@angular/core";
import { Api } from "../api/generated/api";
import { getProductLicenseDiscovery } from "../api/generated/fn/product-license/get-product-license-discovery";
import type { ProductLicenseDiscoveryDto } from "../api/generated/models/product-license-discovery-dto";

/**
 * Reads the public discovery document (/.well-known/logisticsx.json) once per app start and
 * drives `<ui-license-banner>`. Anonymous on purpose: tenant and customer users cannot read the
 * full license status, but every portal shows the noncommercial banner until a key is installed.
 */
@Injectable({ providedIn: "root" })
export class ProductLicenseService {
  private readonly api = inject(Api);

  private readonly state = signal<ProductLicenseDiscoveryDto | null>(null);
  private pending: Promise<void> | null = null;

  /** Nothing is shown before the server has answered, to avoid a flash. */
  public readonly showBanner = computed(() => {
    const state = this.state();
    return state !== null && !state.licensed;
  });

  /** Fetches the discovery document once; later calls share the same request. */
  public load(): Promise<void> {
    this.pending ??= this.fetch();
    return this.pending;
  }

  /** Re-reads the document, e.g. after a key was installed. */
  public refresh(): Promise<void> {
    this.pending = this.fetch();
    return this.pending;
  }

  private async fetch(): Promise<void> {
    try {
      this.state.set(await this.api.invoke(getProductLicenseDiscovery));
    } catch {
      // An unreachable API must not block the app or show a misleading banner.
      this.state.set(null);
    }
  }
}
