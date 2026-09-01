import { ChangeDetectionStrategy, Component, inject } from "@angular/core";
import { LICENSE_CONTACT_HREF } from "../../../constants/common";
import { ProductLicenseService } from "../../../services/product-license.service";
import { Icon } from "../../icons/icon/icon";

/**
 * One-line bar shown in every portal while the deployment runs without a commercial license
 * key. Not dismissible by design: removing it is a deliberate act under the license terms.
 * Mounts at the root layout of the TMS, admin, and customer portals.
 */
@Component({
  selector: "ui-license-banner",
  templateUrl: "./license-banner.html",
  imports: [Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LicenseBanner {
  protected readonly license = inject(ProductLicenseService);

  protected readonly contactHref = LICENSE_CONTACT_HREF;

  constructor() {
    // The banner is the only consumer of the discovery document, so it owns the fetch.
    void this.license.load();
  }
}
